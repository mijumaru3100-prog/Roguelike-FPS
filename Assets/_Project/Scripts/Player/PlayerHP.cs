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
    public float currentHP;
    
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
    [SerializeField] private float LowLine = 0.2f; // 低HPのラインを調整
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
    
    [Header("無敵設定")]
    public float invincibleTime = 1.0f; 
    private bool isInvincible = false;

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
        currentHP = maxHP;
        displayHP = maxHP; // 初期値を合わせる


        // 初回は即座にUIを反映
        UpdateHPUI(currentHP, 0f);
    }
    private Tween shakeTween;

    void Update()
    {
        // 現在の表示用HPから割合を計算
        float hpRatio = Mathf.Clamp01(displayHP / maxHP);

        // テキストを常に displayHP の変動に合わせて更新
        hpText.text = $"{Mathf.CeilToInt(displayHP)} / {maxHP}";

        // 色の更新もリアルタイムに行う
        UpdateBarColor(hpRatio);

        // 泡の発生ロジック (HPの割合に応じて間隔を変化させる例)
        // hpRatioが0のときにエラーにならないよう Mathf.Max で保護
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
        // 1. まず現在の実数から目標の ratio を即座に計算
        ratio = Mathf.Clamp01(targetHP / (float)maxHP);

        hpUiSequence?.Kill();

        if (duration <= 0)
        {
            displayHP = targetHP;
            barRect.sizeDelta = new Vector2(maxWidth * ratio, barRect.sizeDelta.y);
            hpBarRawImage.uvRect = new Rect(0, 0, ratio, 1);
            
            Vector2 targetPistonPos = pistonStartPos;
            targetPistonPos.x += pistonMoveDistance * (1f - ratio);
        }
        else
        {
            hpUiSequence = DOTween.Sequence();
            
            // displayHP をターゲットの値まで滑らかに動かす（Updateでテキストに反映される）
            hpUiSequence.Join(DOTween.To(() => displayHP, x => displayHP = x, targetHP, duration).SetEase(Ease.OutCubic));

            // バーのサイズ変更
            Vector2 targetBarSize = new Vector2(maxWidth * ratio, barRect.sizeDelta.y);
            hpUiSequence.Join(barRect.DOSizeDelta(targetBarSize, duration).SetEase(Ease.OutCubic));
            
            // UV Rect のアニメーション
            hpUiSequence.Join(DOTween.To(() => hpBarRawImage.uvRect.width, 
                                         x => hpBarRawImage.uvRect = new Rect(0, 0, x, 1), 
                                         ratio, duration).SetEase(Ease.OutCubic));
                                         
            // ピストンの移動
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

    // 現在見えているバーの実サイズ
float currentWidth = barRect.sizeDelta.x;

// 泡の半径
float radius = rt.rect.width * 0.5f;

// バーが小さすぎる場合は生成しない
if (currentWidth <= radius * 2f)
{
    Destroy(b);
    return;
}

// バー内部のランダム位置
float x = Random.Range(
    radius,
    currentWidth - radius
);

    // 液体の下側から出現

    rt.anchoredPosition = new Vector2(x, stPosition_y);

    float targetX = Mathf.Clamp(
    x + drift,
    radius,
    currentWidth - radius
);

Vector2 targetPos = new Vector2(
    targetX,
    stPosition_y + rise
);
    // サイズランダム
    float sizeMul = Random.Range(0.8f, 1.3f);
    rt.localScale = Vector3.zero;

    CanvasGroup cg = b.GetComponent<CanvasGroup>();
    if (cg == null)
        cg = b.AddComponent<CanvasGroup>();

    Sequence seq = DOTween.Sequence();

    seq.Append(
        rt.DOScale(Vector3.one * sizeMul, 0.15f)
          .SetEase(Ease.OutBack)
    );

    seq.Join(
        rt.DOAnchorPos(targetPos, duration)
          .SetEase(Ease.OutSine)
    );

    seq.Insert(
        duration * 0.6f,
        cg.DOFade(0f, duration * 0.4f)
    );

    seq.OnComplete(() => Destroy(b));
}

    public void SyncMaxHPWithStats()
    {
        int currentMaxHP = maxHP;
        if (currentMaxHP > lastMaxHP) currentHP += (currentMaxHP - lastMaxHP);
        currentHP = Mathf.Clamp(currentHP, 0, currentMaxHP);
        lastMaxHP = currentMaxHP;
        UpdateHPUI(currentHP);
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        UpdateHPUI(currentHP);
        foreach (var p in pManager.activePassives) p.OnHeal(pManager);
    }

    private Coroutine flashCoroutine;
    public void TakeDamage(float damage)
    {
        if (isInvincible) return;
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        UpdateHPUI(currentHP);
        barRect.DOShakeAnchorPos(0.15f,new Vector2(6f, 0f),20);

        if (currentHP > 0)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashText());
        }
        else { Debug.Log("death"); }
        
        foreach (var p in pManager.activePassives) p.OnGetDamage(pManager);
    }

    IEnumerator FlashText()
    {
        isInvincible = true;
        hpText.color = ChangedColor;
        yield return new WaitForSeconds(invincibleTime);
        hpText.color = defaltColor;
        isInvincible = false;
        flashCoroutine = null;
    }
}