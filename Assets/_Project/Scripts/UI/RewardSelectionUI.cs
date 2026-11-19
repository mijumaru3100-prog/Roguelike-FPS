using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RewardSelectionUI : MonoBehaviour
{
    public TMP_FontAsset customFont;
    private bool selected = false;
    
    public void BuildAndShow(PlayerManager manager, List<PassiveEffect> entries)
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        gameObject.AddComponent<GraphicRaycaster>();
        
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(transform, false);
        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "報酬を選択";
        titleTxt.fontSize = 70;
        titleTxt.alignment = TextAlignmentOptions.Center;
        if (customFont != null) titleTxt.font = customFont;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.9f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(1000, 120);
        titleRect.anchoredPosition = Vector2.zero;
        
        // Card Container
        GameObject container = new GameObject("Container");
        container.transform.SetParent(transform, false);
        HorizontalLayoutGroup hg = container.AddComponent<HorizontalLayoutGroup>();
        hg.childAlignment = TextAnchor.MiddleCenter;
        hg.spacing = 80;
        hg.childControlHeight = false;
        hg.childControlWidth = false;
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(1600, 700);
        containerRect.anchoredPosition = new Vector2(0, -50);
        
        foreach (var entry in entries)
        {
            CreateCard(container.transform, entry, manager);
        }
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void CreateCard(Transform parent, PassiveEffect entry, PlayerManager manager)
    {
        GameObject cardObj = new GameObject("Card_" + entry.passiveName);
        cardObj.transform.SetParent(parent, false);
        Image cardImg = cardObj.AddComponent<Image>();
        cardImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        Button btn = cardObj.AddComponent<Button>();
        
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        cb.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        cb.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        cb.selectedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        btn.colors = cb;
        
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(400, 600);
        
        if (entry.icon != null)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(cardObj.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = entry.icon;
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(160, 160);
            iconRect.anchoredPosition = new Vector2(0, -50);
        }
        
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(cardObj.transform, false);
        TextMeshProUGUI nameTxt = nameObj.AddComponent<TextMeshProUGUI>();
        nameTxt.text = entry.passiveName;
        nameTxt.fontSize = 36;
        nameTxt.alignment = TextAlignmentOptions.Center;
        if (customFont != null) nameTxt.font = customFont;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1f);
        nameRect.anchorMax = new Vector2(1, 1f);
        nameRect.pivot = new Vector2(0.5f, 1f);
        nameRect.sizeDelta = new Vector2(-40, 60);
        nameRect.anchoredPosition = new Vector2(0, -230);
        
        GameObject detailObj = new GameObject("Detail");
        detailObj.transform.SetParent(cardObj.transform, false);
        TextMeshProUGUI detailTxt = detailObj.AddComponent<TextMeshProUGUI>();
        detailTxt.text = entry.detailtext;
        detailTxt.fontSize = 24;
        detailTxt.alignment = TextAlignmentOptions.TopLeft;
        detailTxt.enableWordWrapping = true;
        if (customFont != null) detailTxt.font = customFont;
        RectTransform detailRect = detailObj.GetComponent<RectTransform>();
        detailRect.anchorMin = new Vector2(0, 0);
        detailRect.anchorMax = new Vector2(1, 0);
        detailRect.pivot = new Vector2(0.5f, 0);
        detailRect.sizeDelta = new Vector2(-40, 260); 
        detailRect.anchoredPosition = new Vector2(0, 30);
        
        btn.onClick.AddListener(() => {
            SelectReward(entry, manager);
        });
    }
    
    void SelectReward(PassiveEffect entry, PlayerManager manager)
    {
        if (selected) return;
        selected = true;
        
        if (entry != null) manager.AddPassive(entry);
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Destroy(gameObject);
    }
}
