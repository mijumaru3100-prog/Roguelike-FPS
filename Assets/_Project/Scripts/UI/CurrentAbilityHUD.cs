using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// スクリーンスペースUI（HUD）上でプレイヤーの現在のアビリティアイコンを表示するスクリプト
/// Update()での毎フレーム監視は行わず、開始時およびショップでの購入（アビリティ変更時）のみ更新します。
/// </summary>
public class CurrentAbilityHUD : MonoBehaviour
{
    [Header("参照")]
    public PlayerManager pManager;

    [Header("UI要素")]
    [Tooltip("アビリティアイコンを表示するImage")]
    public Image abilityIcon;
    [Tooltip("アビリティ名を表示するテキスト（任意）")]
    public TextMeshProUGUI abilityNameText;
    [Tooltip("アビリティ未所持時に非表示にするオブジェクト（空欄の場合はabilityIconのGameObject）")]
    public GameObject rootDisplayObject;

    [Header("設定")]
    [Tooltip("アビリティ未所持時にUIを非表示にするか")]
    public bool hideWhenEmpty = true;
    [Tooltip("未所持時のデフォルトアイコン（hideWhenEmpty=falseの場合）")]
    public Sprite defaultEmptyIcon;

    private void Awake()
    {
        if (pManager == null)
        {
            pManager = FindObjectOfType<PlayerManager>();
        }

        if (rootDisplayObject == null && abilityIcon != null)
        {
            rootDisplayObject = abilityIcon.gameObject;
        }
    }

    private void OnEnable()
    {
        if (pManager != null)
        {
            pManager.OnAbilityChanged += HandleAbilityChanged;
        }
        UpdateDisplay();
    }

    private void OnDisable()
    {
        if (pManager != null)
        {
            pManager.OnAbilityChanged -= HandleAbilityChanged;
        }
    }

    private void Start()
    {
        if (pManager == null)
        {
            pManager = FindObjectOfType<PlayerManager>();
            if (pManager != null)
            {
                pManager.OnAbilityChanged -= HandleAbilityChanged;
                pManager.OnAbilityChanged += HandleAbilityChanged;
            }
        }
        UpdateDisplay();
    }

    private void HandleAbilityChanged(WeaponAbility newAbility)
    {
        UpdateDisplay(newAbility);
    }

    /// <summary>
    /// 現在のプレイヤーのアビリティを取得
    /// </summary>
    public WeaponAbility GetCurrentAbility()
    {
        if (pManager == null) return null;

        if (pManager.currentAbility != null)
            return pManager.currentAbility;

        if (pManager.currentWeapon != null && pManager.currentWeapon.ability != null)
            return pManager.currentWeapon.ability;

        return null;
    }

    /// <summary>
    /// UIの表示を更新する
    /// </summary>
    public void UpdateDisplay()
    {
        UpdateDisplay(GetCurrentAbility());
    }

    /// <summary>
    /// 指定されたアビリティ情報でUIを更新する
    /// </summary>
    public void UpdateDisplay(WeaponAbility ability)
    {
        if (ability != null)
        {
            if (rootDisplayObject != null)
                rootDisplayObject.SetActive(true);

            if (abilityIcon != null)
            {
                abilityIcon.enabled = ability.icon != null;
                abilityIcon.sprite = ability.icon;
            }

            if (abilityNameText != null)
            {
                abilityNameText.text = ability.abilityName;
            }
        }
        else
        {
            if (hideWhenEmpty)
            {
                if (rootDisplayObject != null)
                    rootDisplayObject.SetActive(false);
            }
            else
            {
                if (rootDisplayObject != null)
                    rootDisplayObject.SetActive(true);

                if (abilityIcon != null)
                {
                    abilityIcon.sprite = defaultEmptyIcon;
                    abilityIcon.enabled = defaultEmptyIcon != null;
                }

                if (abilityNameText != null)
                {
                    abilityNameText.text = "";
                }
            }
        }
    }
}
