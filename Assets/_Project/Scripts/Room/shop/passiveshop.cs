using UnityEngine;
using TMPro;

public class passiveshop : MonoBehaviour
{
    [Header("Setup")]
    public PlayerManager manager;
    public GameObject barrier;
    [Header("Item Data")]
    public PassiveDatabase itempool;
    private PassiveEffect  effect;
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

    void Awake()
    {
        // Fetch references early so ShopSetting() called externally before Start() doesn't crash
        if (manager == null)
        {
            manager = FindObjectOfType<PlayerManager>();
        }
        if (manager != null)
        {
            PlayerBuyText = manager.PlayerBuyText;
            buyTextData = manager.buyTextData;
        }
    }

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
        used = false; 
        ResetShop();
        
        effect = itempool.GetRandom(manager);
        if (effect == null) return;

        nameText.text = effect.passiveName;
        priceText.text = effect.price.ToString();
        detailText.text = effect.detailtext;

        if (model != null) Destroy(model);
        if (modelSpawnPos != null)
        {
            model = Instantiate(effect.model, modelSpawnPos);
            model.transform.localPosition = Vector3.zero;
            model.transform.localScale = Vector3.one;
            model.transform.LookAt(manager.transform.position);
        }

        if (isInside)
        {
            UpdateBuyText();
        }
    }

    public void ResetShop()
    {
        if (barrier != null) barrier.SetActive(true);
        if (PlayerBuyText != null) PlayerBuyText.SetActive(false);
    }

    void UpdateBuyText()
    {
        if (PlayerBuyText != null && !used)
        {
            PlayerBuyText.SetActive(true);
            buyTextData.text = "Press E to buy for " + effect.price.ToString();
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
            if (PlayerBuyText != null) PlayerBuyText.SetActive(false);
        }
    }

    public void buy()
    {
        if (effect == null || used) return;
        
        if (manager.TrySpendMoney(effect.price))
        {
            if (effect != null) manager.AddPassive(effect);

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