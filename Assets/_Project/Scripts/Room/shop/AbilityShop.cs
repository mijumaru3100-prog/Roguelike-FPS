using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AbilityShop : MonoBehaviour
{
    [Header("Setup")]
    public PlayerManager manager;
    public GameObject barrier;

    [Header("Item Data")]
    public List<WeaponAbility> customAbilityPool = new List<WeaponAbility>();
    public WeaponAbility selectedAbility;
    public int price = 100;

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
    public Image abilityImage;
    public GameObject PlayerBuyText;
    public TextMeshProUGUI buyTextData;

    [Header("Player Current Ability UI")]
    public Image currentAbilityImage;
    public TextMeshProUGUI currentAbilityNameText;
    public TextMeshProUGUI currentAbilityDetailText;

    private void Awake()
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
    }

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
            BuyAbility();
        }
    }

    // 商品を設定する
    public void Set()
    {
        used = false;

        if (barrier != null)
            barrier.SetActive(true);

        selectedAbility = null;

        List<WeaponAbility> candidates = new List<WeaponAbility>();

        // プールから未所持のアビリティを抽出
        if (customAbilityPool != null && customAbilityPool.Count > 0)
        {
            candidates.AddRange(customAbilityPool);
        }
        else if (manager != null && manager.DropAbilitys != null && manager.DropAbilitys.Count > 0)
        {
            candidates.AddRange(manager.DropAbilitys);
        }

        // 現在プレイヤーが所持しているアビリティは除外（重複購入防止）
        if (manager != null && manager.currentAbility != null)
        {
            candidates.RemoveAll(a => a == null || a.GetType() == manager.currentAbility.GetType());
        }

        if (candidates.Count > 0)
        {
            selectedAbility = candidates[Random.Range(0, candidates.Count)];
        }
        else
        {
            Debug.LogWarning("[AbilityShop] 選択可能なアビリティがありません");
        }

        UpdateDisplay();
        UpdatePlayerAbilityDisplay();
        UpdatePrompt();
    }

    // 商品情報をUIに反映する
    private void UpdateDisplay()
    {
        if (selectedAbility == null)
        {
            if (nameText != null) nameText.text = "Sold Out";
            if (priceText != null) priceText.text = "";
            if (detailText != null) detailText.text = "";
            if (abilityImage != null) abilityImage.sprite = null;
            return;
        }

        if (nameText != null)
            nameText.text = selectedAbility.abilityName;

        if (priceText != null)
            priceText.text = price.ToString();

        if (detailText != null)
            detailText.text = selectedAbility.description;

        if (abilityImage != null)
            abilityImage.sprite = selectedAbility.icon;
    }

    // プレイヤーが現在装備しているアビリティをUIに反映する
    public void UpdatePlayerAbilityDisplay()
    {
        WeaponAbility playerAbility = null;
        if (manager != null)
        {
            playerAbility = manager.currentAbility != null
                ? manager.currentAbility
                : (manager.currentWeapon != null ? manager.currentWeapon.ability : null);
        }

        if (playerAbility != null)
        {
            if (currentAbilityImage != null)
            {
                currentAbilityImage.sprite = playerAbility.icon;
                currentAbilityImage.enabled = playerAbility.icon != null;
            }

            if (currentAbilityNameText != null)
                currentAbilityNameText.text = playerAbility.abilityName;

            if (currentAbilityDetailText != null)
                currentAbilityDetailText.text = playerAbility.description;
        }
        else
        {
            if (currentAbilityImage != null)
            {
                currentAbilityImage.sprite = null;
                currentAbilityImage.enabled = false;
            }

            if (currentAbilityNameText != null)
                currentAbilityNameText.text = "None";

            if (currentAbilityDetailText != null)
                currentAbilityDetailText.text = "";
        }
    }

    // 購入プロンプトを更新する
    private void UpdatePrompt()
    {
        if (PlayerBuyText == null)
            return;

        bool shouldShow = isInside && !used && selectedAbility != null;
        PlayerBuyText.SetActive(shouldShow);

        if (shouldShow && buyTextData != null)
        {
            buyTextData.text =
                "Press E to buy " + selectedAbility.abilityName + " for " + price;
        }
    }

    // 購入処理
    private void BuyAbility()
    {
        if (selectedAbility == null || used || manager == null)
            return;

        if (!manager.TrySpendMoney(price))
            return;

        // 全武器共通のアビリティとしてプレイヤーに付与
        manager.currentAbility = selectedAbility;

        used = true;

        if (barrier != null)
            barrier.SetActive(false);

        if (nameText != null)
            nameText.text = "Sold Out";

        if (priceText != null)
            priceText.text = "";

        if (detailText != null)
            detailText.text = "";

        if (abilityImage != null)
            abilityImage.sprite = null;

        UpdatePlayerAbilityDisplay();
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
