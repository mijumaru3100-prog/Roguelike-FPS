using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatsShop : MonoBehaviour
{
    public enum StatsType
    {
        bonusDamage,
        bonusMaxAmmo,
        bonusMaxHP,

        WeakPointBonus,

        damageMultiple,
        fireRateMultiple,
        reloadSpeedMultiple,
        recoilForceMultiple,
        MoveSpeedMultiple,
        JumpHeightMultiple,
    }

    [System.Serializable]
    public class StatData
    {
        public StatsType type;
        public string displayName;

        [Header("Balance")]
        public float buffValue;
        public int buffPrice;
    }

    [Header("Setup")]
    public PlayerManager manager;
    public PlayerStats baseStats;

    [Header("Stat Settings")]
    public List<StatData> statDatas = new();

    [Header("Random Range")]
    public float buffMaxRange = 2.0f;
    public float buffMinRange = 0.5f;

    [Header("Price")]
    public int price;
    public float nextpriceMultiple = 1.5f;

    [Header("UI")]
    public TextMeshPro priceText;
    public TextMeshPro BuffText;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;

    [Header("Current Item")]
    public PlayerStats SellStats;

    private bool isInside = false;
    private bool canBuy = true;

    private StatData currentBuff;

    void Awake()
    {
        // Initialize SellStats here so it's ready before any other Start() calls ShopSetting()
        if (baseStats != null)
        {
            SellStats = Instantiate(baseStats);
        }
    }

    void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<PlayerManager>();
        }
        PlayerBuyText = manager.PlayerBuyText;
        buyTextData = manager.buyTextData;

        ShopSetting();
    }

    void Update()
    {
        if (isInside && Input.GetKeyDown(KeyCode.E))
        {
            Buy();
        }
    }

    public void ShopSetting()
    {
        canBuy = true;
        SellStats.ResetToDefault(); 

        float buffRangeRate = Random.Range(buffMinRange, buffMaxRange);
        buffRangeRate = Mathf.Round(buffRangeRate * 10f) / 10f;

        SelectRandomStats();

        ApplyBuff(currentBuff, buffRangeRate);

        price = GetBuffPrice(currentBuff, buffRangeRate);

        UpdateUI();
    }

    void SelectRandomStats()
    {
        if (statDatas.Count == 0) return;

        currentBuff = statDatas[Random.Range(0, statDatas.Count)];
    }

    void ApplyBuff(StatData stat, float rangeRate)
    {
        float value = CalculateBuff(stat.type, stat.buffValue, rangeRate);
        SetStatValue(stat.type, value);
    }

    float CalculateBuff(StatsType type, float baseValue, float rate)
    {
        if (IsFlatStat(type))
        {
            return Mathf.Round(baseValue * rate * 100f) / 100f;
        }
        else
        {
            return Mathf.Round(baseValue * rate * 100f) / 100f;
        }
    }

    bool IsFlatStat(StatsType type)
    {
        return type == StatsType.bonusDamage ||
               type == StatsType.bonusMaxAmmo ||
               type == StatsType.bonusMaxHP ||
               type == StatsType.WeakPointBonus; 
    }

    void SetStatValue(StatsType type, float value)
    {
        switch (type)
        {
            case StatsType.bonusDamage:
                SellStats.bonusDamage = Mathf.RoundToInt(value);
                break;
            case StatsType.bonusMaxAmmo:
                SellStats.bonusMaxAmmo = Mathf.RoundToInt(value);
                break;
            case StatsType.bonusMaxHP:
                SellStats.bonusMaxHP = Mathf.RoundToInt(value);
                break;

            case StatsType.WeakPointBonus:
                SellStats.WeakPointBonus = value;
                break;

            case StatsType.damageMultiple:
                SellStats.damageMultiple = value;
                break;
            case StatsType.fireRateMultiple:
                SellStats.fireRateMultiple = value;
                break;
            case StatsType.reloadSpeedMultiple:
                SellStats.reloadSpeedMultiple = value;
                break;
            case StatsType.recoilForceMultiple:
                SellStats.recoilForceMultiple = value;
                break;
            case StatsType.MoveSpeedMultiple:
                SellStats.MoveSpeedMultiple = value;
                break;
            case StatsType.JumpHeightMultiple:
                SellStats.JumpHeightMultiple = value;
                break;

        }
    }

    float GetStatValue(StatsType type)
    {
        switch (type)
        {
            case StatsType.bonusDamage:
                return SellStats.bonusDamage;
            case StatsType.bonusMaxAmmo:
                return SellStats.bonusMaxAmmo;
            case StatsType.bonusMaxHP:
                return SellStats.bonusMaxHP;
            case StatsType.WeakPointBonus:
                return SellStats.WeakPointBonus;

            case StatsType.damageMultiple:
                return SellStats.damageMultiple;
            case StatsType.fireRateMultiple:
                return SellStats.fireRateMultiple;
            case StatsType.reloadSpeedMultiple:
                return SellStats.reloadSpeedMultiple;
            case StatsType.recoilForceMultiple:
                return SellStats.recoilForceMultiple;
            case StatsType.MoveSpeedMultiple:
                return SellStats.MoveSpeedMultiple;
            case StatsType.JumpHeightMultiple:
                return SellStats.JumpHeightMultiple;

            default:
                return 1f;
        }
    }

    int GetBuffPrice(StatData stat, float rangeRate)
    {
        return Mathf.RoundToInt(stat.buffPrice * rangeRate);
    }

    void UpdateUI()
    {
        if (priceText != null) priceText.text = canBuy ? price.ToString() : "SoldOut";

        if (currentBuff == null) return;

        float buffVal = GetStatValue(currentBuff.type);
        float displayBuff = IsFlatStat(currentBuff.type) ? buffVal : buffVal * 100f;
        string buffUnit = IsFlatStat(currentBuff.type) ? "" : "%";

        if (BuffText != null)
        {
            BuffText.text = $"{currentBuff.displayName} +{displayBuff}{buffUnit}";
        }

        UpdateBuyText();
    }

    public void UpdateBuyText()
    {
        if (isInside && PlayerBuyText != null && buyTextData != null)
        {
            if (!canBuy) { buyTextData.text = "SoldOut"; return; }
            buyTextData.text = "Press E to buy for " + price;
        }
    }

    public void ResetShop()
    {
        canBuy = true;
        ShopSetting();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInside = true;

            if (PlayerBuyText != null)
            {
                PlayerBuyText.SetActive(true);
            }

            UpdateBuyText();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isInside = false;

            if (PlayerBuyText != null)
            {
                PlayerBuyText.SetActive(false);
            }
        }
    }

    public void Buy()
    {
        
        if (manager != null && manager.TrySpendMoney(price))
        {
            manager.sharedStats.ApplyModifier(SellStats, true);

            price = Mathf.CeilToInt(price * nextpriceMultiple);

            if (manager.currentWeapon != null)
            {
                manager.currentWeapon.UpdateAmmoDisplay();
            }

            PlayerHP hp = manager.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.SyncMaxHPWithStats();
            }

            UpdateUI();

            Debug.Log($"[StatsShop] を購入した、よ！ 次の価格: {price}");
        }
    }
}