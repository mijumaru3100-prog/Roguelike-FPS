using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoBeltUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform bulletGrid; 
    [SerializeField] private TextMeshProUGUI ammoText; 
    [SerializeField] private GameObject bulletPrefab;  
    [SerializeField] private Image weaponIconImage;

    [Header("Settings")]
    [SerializeField] private float bulletSpacing = 40f; 
    [SerializeField] private float moveDuration = 0.05f;
    [SerializeField] private float shakeMagnitude = 5f;  

    private Vector2 baseGridPosition;
    private Coroutine moveCoroutine;
    
    private List<GameObject> activeBullets = new List<GameObject>();

    private void Awake()
    {
        if (bulletGrid != null)
        {
            baseGridPosition = bulletGrid.anchoredPosition;
        }
        
        foreach (Transform child in bulletGrid)
        {
            activeBullets.Add(child.gameObject);
        }
    }

    public void InitializeAmmoBelt(int currentAmmo, int maxAmmo, Sprite bulletSprite, Sprite weaponSprite)
    {
        if (bulletGrid == null || bulletPrefab == null) return;

        if (weaponIconImage != null && weaponSprite != null)
        {
            weaponIconImage.sprite = weaponSprite;
        }

        foreach (var bullet in activeBullets)
        {
            Destroy(bullet);
        }
        activeBullets.Clear();

        for (int i = 0; i < maxAmmo; i++)
        {
            GameObject newBullet = Instantiate(bulletPrefab, bulletGrid);
            newBullet.name = $"Bullet_{i}";
            activeBullets.Add(newBullet);

            Image bulletImage = newBullet.GetComponent<Image>();
            if (bulletImage != null && bulletSprite != null)
            {
                bulletImage.sprite = bulletSprite;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(bulletGrid);

        bulletGrid.anchoredPosition = baseGridPosition;

        SynchronizeAmmoUI(currentAmmo, maxAmmo, true, ammoText != null ? ammoText.color : Color.white);
    }

    public void SynchronizeAmmoUI(int currentAmmo, int maxAmmo, bool isReloading, Color textColor)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
            ammoText.color = textColor;
        }

        if (bulletGrid == null) return;

        int missingAmmo = maxAmmo - currentAmmo;
        float targetX = baseGridPosition.x - (missingAmmo * bulletSpacing);

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        bool shouldShake = !isReloading && currentAmmo < maxAmmo;
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
                float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude * 1.5f);
                bulletGrid.anchoredPosition = new Vector2(targetX + offsetX, targetPos.y);
                yield return null;
            }
            bulletGrid.anchoredPosition = targetPos;
        }
    }
}