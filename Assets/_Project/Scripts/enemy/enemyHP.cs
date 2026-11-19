using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public enum DamageType
{
    Normal,
    WeakPoint,
    Impact,
    Passive
}

public class EnemyHP : MonoBehaviour 
{
    public int maxHP = 100;
    private int currentHP;
    public int CurrentHP => currentHP;
    public PlayerManager pManager;
    [SerializeField] private LayerMask knockbackObstacleLayer;

    [Header("Effects")]
    public GameObject damageTextPrefab;
    [SerializeField] private ParticleSystem deathEffect; // ここに移動
    private TextMeshPro text;
    public float damageTextTime = 1f;
    private float lastDamageTime = 0f;
    private int totalDamage = 0;

    [Header("サウンド")]
    public AudioClip HitSound;
    public AudioClip HeadShotSound;
    public AudioClip KillClip;
    public List<AudioSource> AudioSources = new List<AudioSource>();
    private int currentIndex = 0;

    [Header("ライト演出")]
    public Light flashLight; 
    public float flashIntensity = 20f; 
    public float flashDuration = 0.1f;
    private Coroutine flashCoroutine;

    [Header("部位設定（複数対応）")]
    public List<Collider> weakPointColliders = new List<Collider>();
    public List<Collider> ignoredColliders = new List<Collider>();

    public float defaultWeakPointBonus = 2f;
    public float WeakPointBonus
{
    get
    {
        float totalMult = stats.WeakPointBonus;

Debug.Log($"[WeakPoint] Start | Gun:{name} | stats:{stats.WeakPointBonus}");

foreach (var p in pManager.activePassives.ToArray())
{
    float add = p.GetWeakPointBonus(pManager);

    Debug.Log($"[WeakPoint] Passive:{p.GetType().Name} | add:{add}");

    totalMult += add;
}

Debug.Log($"[WeakPoint] Before Modifier | total:{totalMult}");
        
        totalMult += defaultWeakPointBonus;
        totalMult = Mathf.Max(0.01f, totalMult);
        

        return totalMult;
    }
}

    public PlayerStats stats;
    public EnemyAI _enemyAI;

    private Vector3 knockbackVelocity;
    private bool isKnockback;

    [SerializeField] private float impactDamageMultiplier = 2f;
[SerializeField] private float minimumImpactSpeed = 3f;
[SerializeField] private float maxImpactDamage = 50f;



    void Start()
    {
        currentHP = maxHP;
        if (_enemyAI == null) _enemyAI = GetComponentInParent<EnemyAI>();
        
        if (damageTextPrefab != null)
        {
            text = damageTextPrefab.GetComponent<TextMeshPro>();
        }
    }

    private void Update()
{
    if (!isKnockback) return;

    Vector3 movement = knockbackVelocity * Time.deltaTime;

    if (movement.sqrMagnitude > 0f)
    {
        Vector3 direction = movement.normalized;
        float distance = movement.magnitude;

        Vector3 rayOrigin = transform.position + Vector3.up * 1f;

        if (Physics.Raycast(
            rayOrigin,
            direction,
            out RaycastHit hit,
            distance + 0.1f,
            knockbackObstacleLayer,
            QueryTriggerInteraction.Ignore))
        {
            // 衝突直前の速度を保存
            float impactSpeed = knockbackVelocity.magnitude;

            // 壁に当たった
            knockbackVelocity = Vector3.zero;

            // 壁の手前まで移動
            transform.position = hit.point - direction * 0.2f;

            ApplyImpactDamage(impactSpeed);

            EndKnockback();
            return;
        }
    }

    transform.position += movement;

    knockbackVelocity = Vector3.Lerp(
        knockbackVelocity,
        Vector3.zero,
        5f * Time.deltaTime
    );

    if (knockbackVelocity.magnitude < 0.1f)
    {
        EndKnockback();
    }
}

    public void OnHitBullet(float damage, Collider hitCollider)
    {
        if (hitCollider != null && ignoredColliders.Contains(hitCollider))
        {
            return;
        }

        DamageType CurrentDamageType = DamageType.Normal;
        if(hitCollider != null && weakPointColliders.Contains(hitCollider)) CurrentDamageType = DamageType.WeakPoint;

        if (pManager != null && pManager.BuffFlug != null)
        {
            pManager.BuffFlug.IsHeadShot = CurrentDamageType == DamageType.WeakPoint;
        }
        
        if (pManager != null)
        {
            foreach (var p in pManager.activePassives)
            {
                p.OnHitBullet(pManager, damage, this);
            }
        }

        TakeDamage(damage,CurrentDamageType);
    }

    public void TakeDamage(float damage,DamageType CurrentDamageType)
    {
        if (pManager != null && pManager.BuffFlug != null)
        {   
            foreach (var p in pManager.activePassives)
            {
                p.OnBeforeTakeDamage(pManager);
            }
        }

        float finalDamage = damage;

        if (CurrentDamageType == DamageType.WeakPoint && stats != null)
        {
            if (WeakPointBonus > 0)
            {
                float calculated = damage * WeakPointBonus;
                finalDamage = Mathf.Max(1, Mathf.RoundToInt(calculated));
            }
        }

        if (pManager != null && pManager.BuffFlug != null)
        {
            pManager.BuffFlug.IsOneShotKill = (currentHP <= finalDamage && maxHP <= currentHP);
            pManager.BuffFlug.MaxHP = (maxHP <= currentHP);
        }

        currentHP -= Mathf.Max(1, Mathf.RoundToInt(finalDamage));

        if (_enemyAI != null) _enemyAI.OnDamage();
        PlayFlash();
        PlayHitSound();
        pManager.crosshair.OnHit();

        if (damageTextPrefab != null && text != null)
        {
            HandleDamageText(Mathf.RoundToInt(finalDamage), CurrentDamageType);
        }
        

        if (pManager != null)
        {
            foreach (var p in pManager.activePassives)
            {
                p.OnTakeDamage(pManager, finalDamage, this);
            }
        }

        if (currentHP <= 0)
        {
            if (pManager != null)
            {
                foreach (var p in pManager.activePassives)
                {
                    p.OnKillEnemy(pManager);
                }
            }
            Death();
        }
    }

    private void HandleDamageText(int damage, DamageType CurrentDamageType)
    {
        if (damageTextPrefab == null) return;
        
        if (Time.time - lastDamageTime > damageTextTime)
        {
            totalDamage = 0;
        }
        
        if(CurrentDamageType == DamageType.WeakPoint) text.color = Color.yellow;
        else if(CurrentDamageType == DamageType.Impact) text.color = Color.white;
        else text.color = Color.red;

        totalDamage += damage;
        text.fontSize = Mathf.Min(100 + 0.1f * totalDamage, 200);
        lastDamageTime = Time.time;
        text.text = totalDamage.ToString();
        damageTextPrefab.SetActive(true);
        
        StopCoroutine(nameof(HideDamageText));
        StartCoroutine(nameof(HideDamageText));
    }

    IEnumerator HideDamageText()
    {
        yield return new WaitForSeconds(damageTextTime);
        if (Time.time - lastDamageTime > damageTextTime)
        {
            damageTextPrefab.SetActive(false);
        }
    }

    void Death()
    {
        AudioSource source = SelectSource();
        if (source != null)
        {
            source.clip = KillClip;
            source.Play();
        }
            
        if (deathEffect != null)
        {
            deathEffect.Play();
        }

        if (_enemyAI != null) _enemyAI.OnDie();
    }

    public void PlayFlash()
    {
        if (flashLight == null) return;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(LightFlashRoutine());
    }

    private IEnumerator LightFlashRoutine()
    {
        flashLight.color = Color.red;
        flashLight.intensity = flashIntensity * 0.5f;
        yield return new WaitForSeconds(flashDuration);

        flashLight.intensity = 0f;
    }

    private void PlayHitSound()
    {
        AudioSource source = SelectSource();
        if (source != null)
        {
            if (pManager != null && pManager.BuffFlug != null && pManager.BuffFlug.IsHeadShot)
            {
                if (HeadShotSound != null) source.clip = HeadShotSound;
            }
            else
            {
                if (HitSound != null) source.clip = HitSound;    
            }
            source.Play();
        }
    }

    private AudioSource SelectSource()
    {
        AudioSource source = null;
        if (AudioSources != null && AudioSources.Count > 0)
        {
            source = AudioSources[currentIndex];
            currentIndex = (currentIndex + 1) % AudioSources.Count;
        }
        else
        {
            source = GetComponent<AudioSource>();
        }

        return source;
    }


public void ApplyKnockback(Vector3 knockbackDirection, float knockbackForce)
{
    NormalEnemyAI normalAI = _enemyAI as NormalEnemyAI;

    if (normalAI == null)
    {
        Debug.LogWarning("NormalEnemyAIが見つからない");
        return;
    }

    Debug.Log("Knockback Start : " + knockbackForce);

    if (normalAI.agent != null)
    {
        normalAI.agent.enabled = false;
    }

    knockbackVelocity = knockbackDirection.normalized * knockbackForce;
    isKnockback = true;
}

private void EndKnockback()
{
    isKnockback = false;
    knockbackVelocity = Vector3.zero;

    NormalEnemyAI normalAI = _enemyAI as NormalEnemyAI;

    if (normalAI == null) return;

    if (normalAI.agent != null)
    {
        normalAI.agent.enabled = true;

        if (normalAI.agent.isOnNavMesh)
        {
            normalAI.agent.Warp(transform.position);
        }
    }
}

private void ApplyImpactDamage(float impactSpeed)
{
    if (impactSpeed < minimumImpactSpeed)
        return;

    float damage = (impactSpeed - minimumImpactSpeed)
                   * impactDamageMultiplier;

    damage = Mathf.Clamp(damage, 0f, maxImpactDamage);

    TakeDamage(damage,DamageType.Impact);
}
}