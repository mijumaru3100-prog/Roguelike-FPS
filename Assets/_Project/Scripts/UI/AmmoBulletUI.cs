using System.Collections;
using System.Collections.Generic; // Listを使うために必要
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoBeltUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform bulletGrid; // 弾丸が並んでいる親オブジェクト（Horizontal Layout Group付き）
    [SerializeField] private TextMeshProUGUI ammoText; // 残弾数テキスト
    [SerializeField] private GameObject bulletPrefab;  // ★追加：並べる弾丸画像のプレハブ
    [SerializeField] private Image weaponIconImage;

    [Header("Settings")]
    [SerializeField] private float bulletSpacing = 40f; // 弾丸1発分の横幅＋隙間
    [SerializeField] private float moveDuration = 0.05f; // 通常射撃時の速度
    [SerializeField] private float shakeMagnitude = 5f;  // 発射時のガタつき

    private Vector2 baseGridPosition;
    private Coroutine moveCoroutine;
    
    // ★追加：生成した弾丸オブジェクトを管理するリスト
    private List<GameObject> activeBullets = new List<GameObject>();

    private void Awake()
    {
        if (bulletGrid != null)
        {
            baseGridPosition = bulletGrid.anchoredPosition;
        }
        
        // 念のため、起動時にインスペクターで手動配置された弾があればリストに入れておく
        foreach (Transform child in bulletGrid)
        {
            activeBullets.Add(child.gameObject);
        }
    }

    /// <summary>
    /// ★追加：武器持ち替え時などに呼び出し、maxAmmoの数だけ弾丸を生成・並べ替える
    /// </summary>
    /// <param name="currentAmmo">現在の弾数</param>
    /// <param name="maxAmmo">最大弾数</param>
    public void InitializeAmmoBelt(int currentAmmo, int maxAmmo, Sprite bulletSprite, Sprite weaponSprite) // ★Sprite引数を追加
    {
        if (bulletGrid == null || bulletPrefab == null) return;

        if (weaponIconImage != null && weaponSprite != null)
        {
            weaponIconImage.sprite = weaponSprite;
            // 縦横比を維持したい場合は、 weaponIconImage.preserveAspect = true; 
        }

        // 1. 既存の弾丸をすべて削除してリセット（以前と同じ）
        foreach (var bullet in activeBullets)
        {
            Destroy(bullet);
        }
        activeBullets.Clear();

        // 2. maxAmmoの数だけ新しい弾丸を生成
        for (int i = 0; i < maxAmmo; i++)
        {
            GameObject newBullet = Instantiate(bulletPrefab, bulletGrid);
            newBullet.name = $"Bullet_{i}";
            activeBullets.Add(newBullet);
            
            // ★追加★ 生成した弾丸のImageコンポーネントを取得し、スプライトを差し替える
            Image bulletImage = newBullet.GetComponent<Image>();
            if (bulletImage != null && bulletSprite != null)
            {
                bulletImage.sprite = bulletSprite;
            }
        }

        // 3. Horizontal Layout Groupの即時更新（以前と同じ）
        LayoutRebuilder.ForceRebuildLayoutImmediate(bulletGrid);

        // 4. 初期位置にパッと移動（以前と同じ）
        bulletGrid.anchoredPosition = baseGridPosition;

        // 5. テキストなども初期化同期（以前と同じ）
        SynchronizeAmmoUI(currentAmmo, maxAmmo, true, ammoText != null ? ammoText.color : Color.white);
    }
    /// <summary>
    /// 武器側から呼び出すメインの同期メソッド（以前から変更なし）
    /// </summary>
    public void SynchronizeAmmoUI(int currentAmmo, int maxAmmo, bool isReloading, Color textColor)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
            ammoText.color = textColor;
        }

        if (bulletGrid == null) return;

        // 目標位置の計算（減るほど左に流れる）
        int missingAmmo = maxAmmo - currentAmmo;
        float targetX = baseGridPosition.x - (missingAmmo * bulletSpacing);

        // アニメーション制御
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        bool shouldShake = !isReloading && currentAmmo < maxAmmo; // 射撃時かつ弾が減った時のみ揺らす
        float speed = isReloading ? 0.1f : moveDuration; 
        
        moveCoroutine = StartCoroutine(AnimateBelt(targetX, speed, shouldShake));
    }

    private IEnumerator AnimateBelt(float targetX, float duration, bool shouldShake)
    {
        Vector2 startPos = bulletGrid.anchoredPosition;
        Vector2 targetPos = new Vector2(targetX, startPos.y);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // スムーズに補間
            bulletGrid.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        bulletGrid.anchoredPosition = targetPos;

        if (shouldShake)
        {
            float shakeElapsed = 0f;
            float shakeDuration = 0.05f;

            while (shakeElapsed < shakeDuration)
            {
                shakeElapsed += Time.deltaTime;
                // 右から左に送られた反動で、一瞬だけ右にブレて戻るようなブレを表現
                float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude * 1.5f);
                bulletGrid.anchoredPosition = new Vector2(targetX + offsetX, targetPos.y);
                yield return null;
            }
            // 完全に元の位置に戻す
            bulletGrid.anchoredPosition = targetPos;
        }
    }
}