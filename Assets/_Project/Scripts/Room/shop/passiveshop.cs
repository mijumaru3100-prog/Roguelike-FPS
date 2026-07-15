using UnityEngine;
using TMPro;

public class passiveshop : MonoBehaviour
{
    [Header("Setup")]
    public PlayerManager manager;
    public GameObject barrier;
    [Header("Item Data")]
    public PassiveDatabase itempool;
    public PassiveShopEntry selected;
    private GameObject model;
    [Header("UI")]
    public TextMeshPro nameText;
    public TextMeshPro priceText;
    public TextMeshPro detailText;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;
    public Transform modelSpawnPos;
    public bool spin = false;
    private bool used = false;
    private bool isInside = false; 

    void Start()
    {
        ShopSetting();
    }

    void Update()
    {
        if (model != null && spin)
        {
            model.transform.LookAt(manager.transform.position);
        }
        
        if (isInside && !used && Input.GetKeyDown(KeyCode.E))
        {
            buy();
        }
    }

    public void ShopSetting()
    {
        // 1. ショップの状態をリセット
        used = false; 
        ResetShop();
        
        // 2. 新しいアイテムを取得
        selected = itempool.GetRandom(manager);
        if (selected == null) return;

        // 3. UIの更新
        nameText.text = selected.effect.passiveName;
        priceText.text = selected.effect.price.ToString();
        detailText.text = selected.effect.detailtext;

        // 4. モデルの再生成
        if (model != null) Destroy(model);
        if (modelSpawnPos != null)
        {
            model = Instantiate(selected.effect.model, modelSpawnPos);
            model.transform.localPosition = Vector3.zero;
            model.transform.localScale = Vector3.one;
            model.transform.LookAt(manager.transform.position);
        }

        // 5. プレイヤーが既に範囲内にいる場合はテキストを即時更新
        if (isInside)
        {
            UpdateBuyText();
        }
    }

    public void ResetShop()
    {
        if (barrier != null) barrier.SetActive(true);
        PlayerBuyText.SetActive(false);
    }

    void UpdateBuyText()
    {
        if (PlayerBuyText != null && !used)
        {
            PlayerBuyText.SetActive(true);
            buyTextData.text = "Press E to buy for " + selected.effect.price.ToString();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (other.CompareTag("Player"))
        {
            isInside = true;
            UpdateBuyText();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInside = false;
            PlayerBuyText.SetActive(false);
        }
    }

    public void buy()
    {
        if (selected == null || used) return;
        
        if (manager.TrySpendMoney(selected.effect.price))
        {
            // パッシブ効果の適用
            if (selected.effect != null) manager.AddPassive(selected.effect);
            if (selected.additionalEffects != null)
            {
                foreach (var extra in selected.additionalEffects)
                {
                    if (extra != null) manager.AddPassive(extra);
                }
            }
            if (selected.statBuff != null)
            {
                manager.sharedStats.ApplyModifier(selected.statBuff, true);
                if (manager.currentWeapon != null) manager.currentWeapon.UpdateAmmoDisplay();
                
                PlayerHP hp = manager.GetComponent<PlayerHP>();
                if (hp != null) hp.SyncMaxHPWithStats();
            }

            used = true;
            if (barrier != null) barrier.SetActive(false);
            if (model != null)
            {
                Destroy(model);
                model = null;
            }
            
            DeliteText();
            PlayerBuyText.SetActive(false);
        }
    }

    void DeliteText()
    {
        if (nameText != null) nameText.text = "Sold Out";
        if (priceText != null) priceText.text = "";
        if (detailText != null) detailText.text = "";
    }
}