using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    public int baseMaxHP = 6;
    public int maxHP => Mathf.Max(1, Mathf.RoundToInt((baseMaxHP + pManager.sharedStats.bonusMaxHP) * (1+pManager.sharedStats.maxHPMultiple)));
    
    private int lastMaxHP; 
    private int _currentHP;
    public int  currentHP => _currentHP;
    
    [Tooltip("DOTweenで滑らかに変動させる表示用のHP")]
    public float displayHP; 
    private float ratio;
    public PlayerManager pManager;

    [SerializeField] private RawImage hpBarRawImage;
    [SerializeField] private RectTransform hpBarRoot;
    private Vector2 hpBarStartPos;
    [SerializeField] private float maxWidth; 
    private RectTransform barRect; 

    public TMP_Text hpText;
    [Header("TextColor")]
    [SerializeField] private Color defaltColor = Color.white;
    [SerializeField] private Color ChangedColor = Color.red;

    [Header("液体の色")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color damageColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private float DamagedLine = 0.5f;
    [SerializeField] private float LowLine = 0.2f; 
    [Header("容器")]
    [SerializeField]  private Image box;

    [Header("ピストン")]
    [SerializeField] private RectTransform piston;
    [SerializeField] private float pistonMoveDistance = 300f;

    [Header("泡")]
    [SerializeField] private RectTransform bubbleParent;
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private float spawnInterval = 0.3f;
    [SerializeField] private float stPosition_y = -40f;
    [SerializeField] private float minRise = 80f;
    [SerializeField] private float maxRise = 160f;
    [SerializeField] private float minDuration = 1.0f;
    [SerializeField] private float maxDuration = 1.8f;

    [SerializeField] private float minDrift = -40f;
    [SerializeField] private float maxDrift = -10f;
    private float spawnTimer;

    private Vector2 pistonStartPos;
    
    [Header("エフェクト設定")]
    public float invincibleTime = 1.0f;
    private bool isInvincible = false;
    private Coroutine invicibleCoroutine;
    
    [SerializeField] private CanvasGroup invincibleEffectUI;
    [SerializeField] private CanvasGroup damageEffectUI;

    private Sequence hpUiSequence;

    void Start()
    {
        pistonStartPos = piston.anchoredPosition;
        hpBarStartPos = hpBarRoot.anchoredPosition;
        barRect = hpBarRawImage.rectTransform;

        if (maxWidth <= 0)
        {
            maxWidth = barRect.sizeDelta.x;
        }
        
        lastMaxHP = maxHP;
        _currentHP = maxHP;
        displayHP = maxHP;

        if (damageEffectUI != null) damageEffectUI.alpha = 0f;
        if (invincibleEffectUI != null) invincibleEffectUI.alpha = 0f;

        UpdateHPUI(currentHP, 0f);
    }

    public void Heal(int amount)
    {
        _currentHP = Mathf.Clamp(_currentHP + amount, 0, maxHP);
        UpdateHPUI(_currentHP);
        foreach (var p in pManager.activePassives.ToArray()) p.OnHeal(pManager,amount,this);
    }

    private void PlayDamageEffect()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashText());

        if (damageEffectUI != null)
        {
            damageEffectUI.DOKill();
            damageEffectUI.alpha = 1.0f;
            damageEffectUI.DOFade(0f, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    private Coroutine flashCoroutine;
    IEnumerator FlashText()
    {
        hpText.color = ChangedColor;
        yield return new WaitForSeconds(invincibleTime);
        hpText.color = defaltColor;
        flashCoroutine = null;
    }

public void TakeDamage(int damage)
{
    if (isInvincible) return;
    _currentHP = Mathf.Clamp(_currentHP - damage, 0, maxHP);
    UpdateHPUI(_currentHP);
    barRect.DOShakeAnchorPos(0.15f, new Vector2(6f, 0f), 20);

    if (_currentHP > 0)
    {
        PlayDamageEffect();
        GrantInvincibility(invincibleTime, false);
    }
    else { Debug.Log("death"); }
    
    foreach (var p in pManager.activePassives.ToArray()) p.OnGetDamage(pManager);
}

public void GrantInvincibility(float duration, bool showEffect = true)
{
    if (invicibleCoroutine != null) StopCoroutine(invicibleCoroutine);
    invicibleCoroutine = StartCoroutine(InvincibilityRoutine(duration, showEffect));
}

private IEnumerator InvincibilityRoutine(float duration, bool showEffect)
{
    isInvincible = true;
    
    if (showEffect && invincibleEffectUI != null)
    {
        invincibleEffectUI.alpha = 1.0f;
    }

    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float remainingRatio = 1.0f - (elapsed / duration);

        if (showEffect && invincibleEffectUI != null)
        {
            if (remainingRatio <= 0.2f)
            {
                invincibleEffectUI.alpha = (Mathf.Sin(Time.time * 20f) > 0) ? 0.5f : 0.2f;
            }
            else
            {
                invincibleEffectUI.alpha = remainingRatio;
            }
        }

        yield return null;
    }

    if (showEffect && invincibleEffectUI != null)
    {
        invincibleEffectUI.DOFade(0, 0.2f);
    }

    isInvincible = false;
    invicibleCoroutine = null;
}

    private Tween shakeTween;

    void Update()
    {
        float hpRatio = Mathf.Clamp01(displayHP / maxHP);

        hpText.text = $"{Mathf.CeilToInt(displayHP)} / {maxHP}";

        UpdateBarColor(hpRatio);

        float interval = spawnInterval * Mathf.Max(0.1f, hpRatio); 

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= interval)
        {
            SpawnBubble(hpRatio);
            spawnTimer = 0f;
        }

        if (hpRatio <= LowLine)
        {
            if (shakeTween == null || !shakeTween.IsActive())
            {
                shakeTween = hpBarRoot
                    .DOShakeAnchorPos(
                        999f,
                        new Vector2(0.1f, 0.1f),
                        20,
                        90,
                        false,
                        false
                    );
            }
        }
        else
        {
            shakeTween?.Kill();
            hpBarRoot.anchoredPosition = hpBarStartPos;
        }
    }

    private void UpdateHPUI(float targetHP, float duration = 0.25f)
    {
        ratio = Mathf.Clamp01(targetHP / (float)maxHP);

        hpUiSequence?.Kill();

        if (duration <= 0)
        {
            displayHP = targetHP;
            barRect.sizeDelta = new Vector2(maxWidth * ratio, barRect.sizeDelta.y);
            hpBarRawImage.uvRect = new Rect(0, 0, ratio, 1);
        }
        else
        {
            hpUiSequence = DOTween.Sequence();
            
            hpUiSequence.Join(DOTween.To(() => displayHP, x => displayHP = x, targetHP, duration).SetEase(Ease.OutCubic));

            Vector2 targetBarSize = new Vector2(maxWidth * ratio, barRect.sizeDelta.y);
            hpUiSequence.Join(barRect.DOSizeDelta(targetBarSize, duration).SetEase(Ease.OutCubic));
            
            hpUiSequence.Join(DOTween.To(() => hpBarRawImage.uvRect.width, 
                                         x => hpBarRawImage.uvRect = new Rect(0, 0, x, 1), 
                                         ratio, duration).SetEase(Ease.OutCubic));
                                         
            Vector2 targetPistonPos = pistonStartPos;
            targetPistonPos.x += pistonMoveDistance * (1f - ratio);
            hpUiSequence.Join(piston.DOAnchorPos(targetPistonPos, duration).SetEase(Ease.OutExpo));
        }
    }

    private void UpdateBarColor(float currentRatio)
    {
        if (currentRatio <= LowLine)
        {
            hpBarRawImage.color = lowColor;
        }
        else if (currentRatio <= DamagedLine)
        {
            hpBarRawImage.color = damageColor;
        }
        else
        {
            hpBarRawImage.color = normalColor;
        }
    }

    void SpawnBubble(float currentRatio)
    {
        if (bubblePrefab == null || bubbleParent == null) return;

        GameObject b = Instantiate(bubblePrefab, bubbleParent);
        RectTransform rt = b.GetComponent<RectTransform>();

        float rise = Random.Range(minRise, maxRise);
        float duration = Random.Range(minDuration, maxDuration);
        float drift = Random.Range(minDrift, maxDrift);

        float currentWidth = barRect.sizeDelta.x;
        float radius = rt.rect.width * 0.5f;

        if (currentWidth <= radius * 2f)
        {
            Destroy(b);
            return;
        }

        float x = Random.Range(radius, currentWidth - radius);
        rt.anchoredPosition = new Vector2(x, stPosition_y);

        float targetX = Mathf.Clamp(x + drift, radius, currentWidth - radius);
        Vector2 targetPos = new Vector2(targetX, stPosition_y + rise);

        float sizeMul = Random.Range(0.8f, 1.3f);
        rt.localScale = Vector3.zero;

        CanvasGroup cg = b.GetComponent<CanvasGroup>();
        if (cg == null) cg = b.AddComponent<CanvasGroup>();

        Sequence seq = DOTween.Sequence();

        seq.Append(rt.DOScale(Vector3.one * sizeMul, 0.15f).SetEase(Ease.OutBack));
        seq.Join(rt.DOAnchorPos(targetPos, duration).SetEase(Ease.OutSine));
        seq.Insert(duration * 0.6f, cg.DOFade(0f, duration * 0.4f));

        seq.OnComplete(() => Destroy(b));
    }

    public void SyncMaxHPWithStats()
    {
        int currentMaxHP = maxHP;
        if (currentMaxHP > lastMaxHP) _currentHP += (currentMaxHP - lastMaxHP);
        _currentHP = Mathf.Clamp(_currentHP, 0, currentMaxHP);
        lastMaxHP = currentMaxHP;
        UpdateHPUI(_currentHP);
    }
}