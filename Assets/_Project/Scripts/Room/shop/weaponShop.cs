using UnityEngine;
using TMPro;

public class weaponshop : MonoBehaviour
{
    [Header("Setup")]
    public PlayerManager manager;
    public GameObject barrier;
    [Header("Item Data")]
    public WeaponDatabase itempool;
    public WeaponShopEntry selected;
    private GameObject model;
    [Header("UI")]
    public TextMeshPro nameText;
    public TextMeshPro priceText;
    public TextMeshPro detailText;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;
    
    [Header("Options")]
    public Transform modelSpawnPos;
    public bool spin = false;
    public float spinSpeed = 100f; // 回転速度を少し速く調整
    
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
            model.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }

        if (isInside && !used && Input.GetKeyDown(KeyCode.E))
        {
            buy();
        }
    }

    public void ShopSetting()
    {
        // 1. ショップのリセット
        used = false;
        if (barrier != null) barrier.SetActive(true);
        if (model != null) Destroy(model);

        // 2. アイテムの設定
        selected = itempool.GetRandom(manager);
        if (selected == null) return;

        nameText.text = selected.weaponName;
        priceText.text = selected.price.ToString();
        detailText.text = selected.detailtext;

        // 3. モデルの生成
        Transform parent = (modelSpawnPos != null) ? modelSpawnPos : transform;
        model = Instantiate(selected.model, parent);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        // 4. プレイヤーが範囲内にいる場合のテキスト更新
        if (isInside)
        {
            UpdateBuyText();
        }
    }

    void UpdateBuyText()
    {
        if (PlayerBuyText != null && !used)
        {
            PlayerBuyText.SetActive(true);
            buyTextData.text = "Press E to buy for " + selected.price.ToString();
        }
    }

    public void buy()
    {
        if (selected == null || used) return;
        
        if (manager.TrySpendMoney(selected.price))
        {
            manager.ChangeWeapon(selected.weaponPrefab);
            
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
}