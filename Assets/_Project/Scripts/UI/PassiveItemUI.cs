using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class PassiveItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI stackCountText;
    public TextMeshProUGUI remainCountText;

    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private PassiveEffect passive;
private PassiveDescriptionUI descriptionUI;

    public void Setup(PassiveEffect passive, PassiveDescriptionUI descriptionUI)
{
    this.passive = passive;
    this.descriptionUI = descriptionUI;

    iconImage.sprite = passive.icon;
}
    public void UpdateDisplay(int stackCount, PassiveEffect passive)
    {
        iconImage.color = passive.running ? activeColor : inactiveColor;

        if (stackCount > 1)
        {
            stackCountText.text = $"x{stackCount}";
            stackCountText.gameObject.SetActive(true);
        }
        else
        {
            stackCountText.gameObject.SetActive(false);
        }

        if (passive.usecount)
        {
            remainCountText.text = passive.remainCount.ToString();
            remainCountText.gameObject.SetActive(true);
        }
        else
        {
            remainCountText.gameObject.SetActive(false);
        }
    }

   public void OnPointerEnter(PointerEventData eventData)
{
    descriptionUI.ShowPassive(passive);
}

public void OnPointerExit(PointerEventData eventData)
{
    descriptionUI.ShowDefault();
}
}