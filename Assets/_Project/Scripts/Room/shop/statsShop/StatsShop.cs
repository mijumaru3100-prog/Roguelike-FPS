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
        public float debuffValue;
        public int buffPrice;
        public int debuffPrice;
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
    public TextMeshPro DebuffText;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;

    [Header("Current Item")]
    public PlayerStats SellStats;

    private bool isInside = false;
    private bool canBuy = true;

    private StatData currentBuff;
    private StatData currentDebuff;

    void Start()
    {
        PlayerBuyText = manager.PlayerBuyText;
        buyTextData = manager.buyTextData;
        if (manager == null)
        {
            manager = FindObjectOfType<PlayerManager>();
        }
        SellStats = Instantiate(baseStats);

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
        float debuffRangeRate = Random.Range(buffMinRange, buffMaxRange);

        buffRangeRate = Mathf.Round(buffRangeRate * 10f) / 10f;
        debuffRangeRate = Mathf.Round(debuffRangeRate * 10f) / 10f;

        SelectRandomStats();

        ApplyBuff(currentBuff, buffRangeRate);
        ApplyDebuff(currentDebuff, debuffRangeRate);

        int buffPrice = GetBuffPrice(currentBuff, buffRangeRate);
        int debuffPrice = GetDebuffPrice(currentDebuff, debuffRangeRate);

        price = Mathf.Max(0, buffPrice - debuffPrice);

        UpdateUI();
    }

    void SelectRandomStats()
    {
        if (statDatas.Count == 0) return;

        currentBuff = statDatas[Random.Range(0, statDatas.Count)];
        currentDebuff = statDatas[Random.Range(0, statDatas.Count)];

        if (statDatas.Count > 1)
        {
            while (currentBuff.type == currentDebuff.type)
            {
                currentDebuff = statDatas[Random.Range(0, statDatas.Count)];
            }
        }
    }

    void ApplyBuff(StatData stat, float rangeRate)
    {
        float value = CalculateBuff(stat.type, stat.buffValue, rangeRate);
        SetStatValue(stat.type, value);
    }

    void ApplyDebuff(StatData stat, float rangeRate)
    {
        float value = CalculateDebuff(stat.type, stat.debuffValue, rangeRate);
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

    float CalculateDebuff(StatsType type, float baseValue, float rate)
{
    if (IsFlatStat(type))
    {
        return Mathf.Round(-baseValue * rate * 100f) / 100f;
    }
    else
    {
        return -Mathf.Round(baseValue * rate * 100f) / 100f;
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
    int GetDebuffPrice(StatData stat, float rangeRate)
    {
        return Mathf.RoundToInt(stat.debuffPrice * rangeRate);
    }

void UpdateUI()
    {
        if (priceText != null) priceText.text = canBuy ? price.ToString() : "SoldOut";

        float buffVal = GetStatValue(currentBuff.type);
        float debuffVal = GetStatValue(currentDebuff.type);

        float absDebuff = Mathf.Abs(debuffVal);

        float displayBuff = IsFlatStat(currentBuff.type) ? buffVal : buffVal * 100f;
        float displayDebuff = IsFlatStat(currentDebuff.type) ? absDebuff : absDebuff * 100f;

        string buffUnit = IsFlatStat(currentBuff.type) ? "" : "%";
        string debuffUnit = IsFlatStat(currentDebuff.type) ? "" : "%";

        BuffText.text = $"{currentBuff.displayName} +{displayBuff}{buffUnit}";
        DebuffText.text = $"{currentDebuff.displayName} -{displayDebuff}{debuffUnit}";

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