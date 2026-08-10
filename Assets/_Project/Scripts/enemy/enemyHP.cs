using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EnemyHP : MonoBehaviour 
{
    public int maxHP = 100;
    private int currentHP;
    public int CurrentHP => currentHP;
    public PlayerManager pManager;

    [Header("Effects")]
    public GameObject damageTextPrefab;
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

    [Header("部位設定")]
    public Collider weakPointCollider;
    
    [Tooltip("ここに登録されたコライダーに当たった場合、ダメージやヒット判定を完全に無視します")]
    public List<Collider> ignoredColliders = new List<Collider>();

    public float defaultWeakPointBonus = 2f;
    public float WeakPointBonus
    {
        get
        {
            float totalMult = defaultWeakPointBonus + stats.WeakPointBonus;
            if (pManager != null)
            {
                foreach (var p in pManager.activePassives) totalMult += p.GetWeakPointBonus(pManager);
            }
            totalMult = Mathf.Max(0.01f, totalMult);
            return totalMult;
        }
    }

    public PlayerStats stats;
    public EnemyAI _enemyAI;

    private bool isWeakPointDamage = false;

    void Start()
    {
        currentHP = maxHP;
        if (_enemyAI == null) _enemyAI = GetComponentInParent<EnemyAI>();
        
        if (damageTextPrefab != null)
        {
            text = damageTextPrefab.GetComponent<TextMeshPro>();
        }
    }

    public void OnHitBullet(float damage, Collider hitCollider)
    {
        if (hitCollider != null && ignoredColliders.Contains(hitCollider))
        {
            return;
        }

        if (pManager != null && pManager.BuffFlug != null)
        {
            pManager.BuffFlug.IsHeadShot = (hitCollider == weakPointCollider);
        }
        
        if (pManager != null)
        {
            foreach (var p in pManager.activePassives)
            {
                p.OnHitBullet(pManager, damage, this);
            }
        }

        TakeDamage(damage, hitCollider);
    }

    public void TakeDamage(float damage, Collider hitCollider)
    {
        if (hitCollider != null && ignoredColliders.Contains(hitCollider)) return;

        if (pManager != null && pManager.BuffFlug != null)
        {
            pManager.BuffFlug.IsHeadShot = (hitCollider == weakPointCollider);
            
            foreach (var p in pManager.activePassives)
            {
                p.OnBeforeTakeDamage(pManager);
            }
        }

        float finalDamage = damage;
        isWeakPointDamage = false;

        if (hitCollider == weakPointCollider && stats != null)
        {
            isWeakPointDamage = true;
            if (WeakPointBonus > 0)
            {
                float calculated = damage * WeakPointBonus;
                finalDamage = Mathf.Max(1, Mathf.RoundToInt(calculated));
                isWeakPointDamage = (WeakPointBonus >= 1.0f);
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

        if (damageTextPrefab != null && text != null)
        {
            HandleDamageText(Mathf.RoundToInt(finalDamage), isWeakPointDamage);
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

    private void HandleDamageText(int damage, bool isWeakPointDamage)
    {
        if (damageTextPrefab == null) return;
        
        if (Time.time - lastDamageTime > damageTextTime)
        {
            totalDamage = 0;
        }

        text.color = isWeakPointDamage ? Color.yellow : Color.red;

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

    [Header("Effects")]
[SerializeField] private ParticleSystem deathEffect;

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
        if (source != null && HitSound != null)
        {
            if(pManager.BuffFlug.IsHeadShot)
            {
                source.clip = HitSound;
            }
            else
            {
                source.clip = HeadShotSound;    
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
}