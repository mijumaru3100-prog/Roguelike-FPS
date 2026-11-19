
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

    [Header("Display")]
    public Transform modelSpawnPos;
    public bool spin = false;
    public float spinSpeed = 100f;

    private GameObject model;
    private bool used;
    private bool isInside;

    [Header("UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI detailText;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;

    private void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<PlayerManager>();
        }

        if (manager != null)
        {
            PlayerBuyText = manager.PlayerBuyText;
            buyTextData = manager.buyTextData;
        }

        Set();
    }

    private void Update()
    {
        if (model != null && spin)
        {
            model.transform.Rotate(
                Vector3.up,
                spinSpeed * Time.deltaTime
            );
        }

        if (isInside && !used &&
            Input.GetKeyDown(KeyCode.E))
        {
            BuyWeapon();
        }
    }

    // 商品を設定する
    public void Set()
    {
        used = false;

        if (barrier != null)
            barrier.SetActive(true);

        selected = null;

        if (itempool == null)
        {
            Debug.LogError("[weaponshop] itempoolがnull");
            return;
        }

        selected = itempool.GetRandom(manager);

        if (selected == null)
        {
            Debug.LogError("[weaponshop] 商品を取得できませんでした");
            RefreshModel();
            return;
        }

        // 表示用モデルと商品情報を更新
        RefreshModel();
        UpdateDisplay();
        UpdatePrompt();
    }

    // 商品モデルを再生成する
    private void RefreshModel()
    {
        if (model != null)
        {
            Destroy(model);
            model = null;
        }

        if (selected == null || selected.model == null)
            return;

        Transform parent = modelSpawnPos != null
            ? modelSpawnPos
            : transform;

        model = Instantiate(selected.model, parent);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
    }

    // 商品情報をUIに反映する
    private void UpdateDisplay()
    {
        if (selected == null)
            return;

        if (nameText != null)
            nameText.text = selected.weaponName;

        if (priceText != null)
            priceText.text = selected.price.ToString();

        if (detailText != null)
            detailText.text = selected.detailtext;
    }

    // 購入プロンプトを更新する
    private void UpdatePrompt()
    {
        if (PlayerBuyText == null)
            return;

        bool shouldShow = isInside && !used && selected != null;
        PlayerBuyText.SetActive(shouldShow);

        if (shouldShow && buyTextData != null)
        {
            buyTextData.text =
                "Press E to buy " + selected.weaponName + " for " + selected.price;
        }
    }

    // 購入処理
    private void BuyWeapon()
    {
        if (selected == null || used || manager == null)
            return;

        if (!manager.TrySpendMoney(selected.price))
            return;

        manager.ChangeWeapon(selected.weaponPrefab);

        used = true;

        if (barrier != null)
            barrier.SetActive(false);

        RefreshModel();

        if (nameText != null)
            nameText.text = "Sold Out";

        if (priceText != null)
            priceText.text = "";

        if (detailText != null)
            detailText.text = "";

        UpdatePrompt();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isInside = true;
        UpdatePrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isInside = false;
        UpdatePrompt();
    }

    private void OnDisable()
    {
        if (PlayerBuyText != null)
            PlayerBuyText.SetActive(false);
    }
}
