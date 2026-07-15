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
    public int money = 0;
    [SerializeField] private float countDuration = 0.5f; 
    private float displayMoney = 0; 
    private Coroutine countCoroutine;
    public event Action<int, int> OnMoneyChanged;

    public List<PassiveEffect> activePassives = new List<PassiveEffect>();
    public BuffFlug BuffFlug;
    public PlayerStats sharedStats;
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
        // ScriptableObjectの意図しない上書きを防ぐため、ゲーム開始時にステータスを初期化
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
                    foreach (var p in activePassives) p.OnMoving(this);
                    startStopTime = Time.time;
                }
                else
                {
                    foreach (var p in activePassives) p.OnStopping(this);
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

        foreach (var p in activePassives)
        {
            p.OnUpdate(this);
        }
    }

    void FixedUpdate() 
    {
        foreach (var p in activePassives)
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
                foreach (var p in activePassives) p.OnEnemyNear(this);
                Debug.Log("<color=orange>[Proximity] 敵が接近しました。</color>");
            }
            else
            {
                foreach (var p in activePassives) p.OnEnemyAway(this);
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

        // すでに立ち止まっている状態なら、追加した瞬間に OnStopping を適用する
        if (playerMove != null && !playerMove.isMoving)
        {
            newPassive.OnStopping(this);
        }
    
        foreach (var p in this.activePassives)
        {
            p.OnGetPassive(this);
        }
        Debug.Log($"{newPassive.passiveName} を追加しました。");
    }

    public void RemovePassive(PassiveEffect p)
    {
        if (activePassives.Contains(p))
        {
            // 外す前に、もし立ち止まっていて効果が適用中なら、解除（OnMoving相当）させる
            if (playerMove != null && !playerMove.isMoving)
            {
                p.OnMoving(this); 
            }
            activePassives.Remove(p);
            Debug.Log($"{p.passiveName} を削除しました。");
        }
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        money += amount;
        
        // すでにカウント中なら一度止めて、新しい目標値へリスタート
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

    // 時間ベースで滑らかに数字を追いつかせるコルーチン
    private IEnumerator MoneyCountAnimation()
    {
        // 現在の表示金額から、実際の所持金（money）までの差分を計算
        float startMoney = displayMoney;
        float targetMoney = money;
        
        // 差分がある間はループ
        while (!Mathf.Approximately(displayMoney, targetMoney))
        {
            // 1秒あたりの変化量を計算（(目標までの総距離) ÷ 設定時間）
            float speed = Mathf.Abs(targetMoney - startMoney) / countDuration;

            // displayMoney を targetMoney に向けて一定速度で近づける
            displayMoney = Mathf.MoveTowards(displayMoney, targetMoney, speed * Time.deltaTime);

            // 画面のテキストを更新（整数にキャストして表示）
            moneyTextUI.text = $"{(int)displayMoney:00,000}";

            // 1フレーム待つ（これによってフリーズを防ぎ、ヌルヌル動く）
            yield return null; 
        }

        // 最後に完全に値を一致させる
        displayMoney = targetMoney;
        moneyTextUI.text = $"{(int)displayMoney:00,000}";
        countCoroutine = null;
    }

}