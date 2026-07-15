using UnityEngine;
using TMPro;
public class HealShop : MonoBehaviour
{
    [Header("Setup")]
    public PlayerManager manager;
    public PlayerHP hp;
    [Header("statsData")]
    public int healAmount;
    public int price;
    [Header("UI")]
    public TextMeshPro nameText;
    public TextMeshPro priceText;
    public TextMeshPro detailText;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;
    void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<PlayerManager>();
        }
        ShopSetting();
    }
    void Update()
    {
        if (isInside && Input.GetKeyDown(KeyCode.E))
        {
            buy();
        }
    }
    void ShopSetting()
    {
        if (priceText != null) priceText.text = price.ToString();
        UpdateBuyText();
    }
    void UpdateBuyText()
    {
        if (isInside && PlayerBuyText != null && buyTextData != null)
        {  
            float heal = healAmount / 2; 
            buyTextData.text = "Press E to buy Heal " + heal.ToString() + " for " + price.ToString();
        }
    }
    private bool isInside = false;
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(hp.currentHP == hp.maxHP) return;
            isInside = true;
            if (PlayerBuyText != null) PlayerBuyText.SetActive(true);
            UpdateBuyText();
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            isInside = false;
            PlayerBuyText.SetActive(false);
        }
    }
    public void buy()
    {
        if(hp.currentHP == hp.maxHP) return;
        if(manager != null && manager.TrySpendMoney(price))
        {
            if (hp != null)
            {
                hp.Heal(healAmount);
            }
        }
    }    
}
