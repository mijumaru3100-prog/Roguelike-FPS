using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

using IEnumerator = System.Collections.IEnumerator;

public class PlayerManager : MonoBehaviour
{
    public GameObject Player;
    public PlayerHP playerHP;
    
    public GunBase currentWeapon;
    public Transform weaponHolder;
    private int money = 0;
    [SerializeField] private float countDuration = 0.5f; 
    private float displayMoney = 0; 
    private Coroutine countCoroutine;
    public event Action<int, int> OnMoneyChanged;

    public List<PassiveEffect> activePassives = new List<PassiveEffect>();
    public BuffFlug BuffFlug;
    public PlayerStats sharedStats;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;
    public TextMeshProUGUI ammoTextUI;
    public AmmoBeltUI ammoBeltUI;
    public Color DefaultTextColor;  
    public Color HarfTextColor;
    public Color LowTextColor;    
    public TextMeshProUGUI moneyTextUI;
    public Camera mainCamera;
    public Transform adsPivot;
    public Transform recoilPivot;
    public CrosshairController crosshair;
    public DungeonManager dungeonManager;
    public PassiveUIManager passiveUIManager;

    public Move playerMove;
    private bool wasMoving = false;
    public float startStopTime;

    [Header("Proximity Settings")]
    public LayerMask enemyLayer;
    public float proximityRadius = 10f;
    public bool isEnemyNear = false;
    public float minEnemyDistance { get; private set; } = float.MaxValue;
    private float proximityCheckTimer = 0f;
    private const float PROXIMITY_CHECK_INTERVAL = 0.1f;
    private Collider[] proximityBuffer = new Collider[16];

    [Header("オブジェクトプール")]
    public ObjectPool bulletPool;
    public ObjectPool NormalShellPool;
    public ObjectPool ShotGunShellPool;
    public ObjectPool tracerPool;
    public ObjectPool coinPool;
    public ObjectPool BulletMarkPool;
    public Material[] bulletHoleMats;

    public ObjectPool HitEffectPool;

    void Awake()
    {
        if (sharedStats != null)
        {
            sharedStats.ResetToDefault();
            Debug.Log("[PlayerManager] PlayerStats has been reset to default values.");
        }
    }

    void Start()
    {   
        if (currentWeapon != null)
        {
            currentWeapon.pManager = this;
            currentWeapon.stats = sharedStats;
            currentWeapon.ammoBeltUI = ammoBeltUI;
            currentWeapon.ammoText = ammoTextUI;
            currentWeapon.playerCamera = mainCamera;
            currentWeapon.adsPivot = adsPivot;
            currentWeapon.recoilPivot = recoilPivot;
            currentWeapon.UpdateAmmoDisplay();
        }

        sharedStats.pManager = this;
        moneyTextUI.text =$"{money:00,000}";
    }

    public void ChangeWeapon(GameObject newWeaponPrefab)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        GameObject newWeaponObj = Instantiate(newWeaponPrefab, weaponHolder);
        
        currentWeapon = newWeaponObj.GetComponent<GunBase>();
        if (currentWeapon != null)
        {
            newWeaponObj.transform.localPosition = currentWeapon.offsetPosition;
            newWeaponObj.transform.localRotation = Quaternion.Euler(currentWeapon.offsetRotation);
            currentWeapon.pManager = this;
            currentWeapon.stats = sharedStats;
            currentWeapon.ammoBeltUI = ammoBeltUI; 
            currentWeapon.ammoText = ammoTextUI;  
            currentWeapon.playerCamera = mainCamera;
            currentWeapon.adsPivot = adsPivot;
            currentWeapon.recoilPivot = recoilPivot;

            currentWeapon.UpdateAmmoDisplay();
        }
        
        Debug.Log("武器を変更しました：" + newWeaponObj.name);
    }

    void Update() 
    {
        if (playerMove != null)
        {
            bool isCurrentlyMoving = playerMove.isMoving;
            if (isCurrentlyMoving != wasMoving)
            {
                if (isCurrentlyMoving)
                {
                    foreach (var p in activePassives.ToArray()) p.OnMoving(this);
                    startStopTime = Time.time;
                }
                else
                {
                    foreach (var p in activePassives.ToArray()) p.OnStopping(this);
                    startStopTime = 0f;
                }    
                wasMoving = isCurrentlyMoving;
            }
        }

        proximityCheckTimer += Time.deltaTime;
        if (proximityCheckTimer >= PROXIMITY_CHECK_INTERVAL)
        {
            proximityCheckTimer = 0f;
            UpdateProximityStatus();
        }

        foreach (var p in activePassives.ToArray())
        {
            p.OnUpdate(this);
        }
    }

    void FixedUpdate() 
    {
        foreach (var p in activePassives.ToArray())
        {
            p.OnFixedUpdate(this);
        }
    }

    private void UpdateProximityStatus()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, proximityRadius, proximityBuffer, enemyLayer);
        bool currentlyNear = count > 0;

        float minDist = float.MaxValue;
        if (currentlyNear)
        {
            for (int i = 0; i < count; i++)
            {
                float d = Vector3.Distance(transform.position, proximityBuffer[i].transform.position);
                if (d < minDist) minDist = d;
            }
        }
        minEnemyDistance = minDist;

        if (currentlyNear != isEnemyNear)
        {
            isEnemyNear = currentlyNear;
            if (isEnemyNear)
            {
                foreach (var p in activePassives.ToArray()) p.OnEnemyNear(this);
                Debug.Log("<color=orange>[Proximity] 敵が接近しました。</color>");
            }
            else
            {
                foreach (var p in activePassives.ToArray()) p.OnEnemyAway(this);
                Debug.Log("<color=cyan>[Proximity] 敵が離れました。</color>");
            }
        }
    }

    public void ToggleWeaponVisibility(bool isVisible)
    {
        if (weaponHolder != null)
        {
            weaponHolder.gameObject.SetActive(isVisible);
        }
    }

    public void AddPassive(PassiveEffect newPassive)
    {
        if (newPassive == null) return;

        activePassives.Add(newPassive);

        newPassive.OnGetThisPassive(this);

        if (playerMove != null && !playerMove.isMoving)
        {
            newPassive.OnStopping(this);
        }
    
        foreach (var p in this.activePassives)
        {
            p.OnGetPassive(this);
        }

        if (passiveUIManager != null) passiveUIManager.RefreshUI();

        Debug.Log($"{newPassive.passiveName} を追加しました。");
    }

    public void RemovePassive(PassiveEffect p)
    {
        if (activePassives.Contains(p))
        {
            if (playerMove != null && !playerMove.isMoving)
            {
                p.OnMoving(this); 
            }
            activePassives.Remove(p);
            if (passiveUIManager != null) passiveUIManager.RefreshUI();
            
            Debug.Log($"{p.passiveName} を削除しました。");
        }
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        money += amount;
        
        if (countCoroutine != null) StopCoroutine(countCoroutine);
        countCoroutine = StartCoroutine(MoneyCountAnimation());
    }

    public bool TrySpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;

            if (countCoroutine != null) StopCoroutine(countCoroutine);
            countCoroutine = StartCoroutine(MoneyCountAnimation());
            return true;
        }
        else
        {
            Debug.Log("所持金が不足しています。");
            return false;
        }
    }

    private IEnumerator MoneyCountAnimation()
    {
        float startMoney = displayMoney;
        float targetMoney = money;
        
        while (!Mathf.Approximately(displayMoney, targetMoney))
        {
            float speed = Mathf.Abs(targetMoney - startMoney) / countDuration;

            displayMoney = Mathf.MoveTowards(displayMoney, targetMoney, speed * Time.deltaTime);

            moneyTextUI.text = $"{(int)displayMoney:00,000}";

            yield return null; 
        }

        displayMoney = targetMoney;
        moneyTextUI.text = $"{(int)displayMoney:00,000}";
        countCoroutine = null;
    }

}