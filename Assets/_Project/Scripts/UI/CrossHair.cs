using UnityEngine;
using DG.Tweening; 
public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Parts")]
    [SerializeField] private RectTransform topLine;
    [SerializeField] private RectTransform bottomLine;
    [SerializeField] private RectTransform leftLine;
    [SerializeField] private RectTransform rightLine;
    [Header("Settings")]
    [SerializeField] private float minSpread = 10f;
    [SerializeField] private float maxSpread = 100f;
    [SerializeField] private float returnDuration = 0.2f; 
    [SerializeField] private Ease returnEase = Ease.OutQuad; 
    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup; 
    private float currentSpread;
    private Tween spreadTween; 
    [Header("Hit Marker")]
    [SerializeField] private CanvasGroup hitMarkerCanvasGroup; 
    [SerializeField] private float hitEffectDuration = 0.15f; 
    [SerializeField] private Vector3 hitScaleEffect = new Vector3(1.2f, 1.2f, 1f); 
    private Tween hitTween;
    void Start()
    {
        currentSpread = minSpread;
        UpdateCrosshairPosition(minSpread);
        if (hitMarkerCanvasGroup != null) hitMarkerCanvasGroup.alpha = 0f;
    }
    public void AddSpread(float amount)
    {
        float targetSpread = Mathf.Min(maxSpread, currentSpread + amount);
        spreadTween.Kill();
        currentSpread = targetSpread;
        spreadTween = DOTween.To(() => currentSpread, x => {
            currentSpread = x;
            UpdateCrosshairPosition(currentSpread);
        }, minSpread, returnDuration)
        .SetEase(returnEase);
    }
    private void UpdateCrosshairPosition(float spread)
    {
        topLine.anchoredPosition = new Vector2(0f, spread);
        bottomLine.anchoredPosition = new Vector2(0f, -spread);
        leftLine.anchoredPosition = new Vector2(-spread, 0f);
        rightLine.anchoredPosition = new Vector2(spread, 0f);
    }
    public void SetVisible(bool visible, float duration = 0.1f)
    {
        if (canvasGroup == null) return;
        canvasGroup.DOFade(visible ? 1f : 0f, duration);
    }
    public void OnHit()
    {
        if (hitMarkerCanvasGroup == null) return;
        hitTween.Kill();
        hitMarkerCanvasGroup.alpha = 1f;
        hitMarkerCanvasGroup.transform.localScale = hitScaleEffect;
        Sequence hitSeq = DOTween.Sequence();
        hitSeq.Append(hitMarkerCanvasGroup.DOFade(0f, hitEffectDuration).SetEase(Ease.InExpo));
        hitSeq.Join(hitMarkerCanvasGroup.transform.DOScale(1f, hitEffectDuration));
        hitTween = hitSeq;
    }
}