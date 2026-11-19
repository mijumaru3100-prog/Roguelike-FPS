using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AbilityPoseUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerManager pManager;
    public Image AbilityIcon;
    public TextMeshProUGUI AbilityNameText;
    public TextMeshProUGUI descriptionText;

    private string originalAbilityName;
    private string originalDescription;

    private void Awake()
    {
        originalAbilityName = AbilityNameText.text;
        originalDescription = descriptionText.text;
    }

    private WeaponAbility GetAbility()
    {
        if (pManager == null) return null;
        if (pManager.currentAbility != null) return pManager.currentAbility;
        if (pManager.currentWeapon != null) return pManager.currentWeapon.ability;
        return null;
    }

    public void Set()
    {
        WeaponAbility ability = GetAbility();

        if (ability == null)
        {
            AbilityIcon.sprite = null;
            return;
        }

        AbilityIcon.sprite = ability.icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        WeaponAbility ability = GetAbility();

        if (ability == null) return;

        AbilityNameText.text = ability.abilityName;
        descriptionText.text = ability.description;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AbilityNameText.text = originalAbilityName;
        descriptionText.text = originalDescription;
    }
}