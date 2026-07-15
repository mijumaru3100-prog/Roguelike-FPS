using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic; // Listを使うために追加
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
    public AudioClip KillClip; // タイポ修正 (KikllClip -> KillClip)
    public List<AudioSource> AudioSources = new List<AudioSource>();
    private int currentIndex = 0; // 追加: オーディオソースの巡回用インデックス

    [Header("ライト演出")]
    public Light flashLight; 
    public float flashIntensity = 20f; 
    public float flashDuration = 0.1f;
    private Coroutine flashCoroutine;

    [Header("弱点")]
    public Collider weakPointCollider;
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
            return 1 + totalMult;
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

    // 弾が当たった時の入り口（ここに判定を集約）
    public void OnHitBullet(float damage, Collider hitCollider)
    {
        // ヘッドショット判定
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
        // OnHitBulletを通らないダメージソース（爆風など）を考慮し、ここでも念のため更新
        if (pManager != null && pManager.BuffFlug != null)
        {
            pManager.BuffFlug.IsHeadShot = (hitCollider == weakPointCollider);
            
            foreach (var p in pManager.activePassives)
            {
                p.OnBeforeTakeDamage(pManager);
            }
        }

        // ダメージ計算
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

        // バフフラグ管理
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

        // パッシブ管理
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
        
        // コルーチンの重複実行を防ぐため、一度止めてから再スタートさせるのが安全
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
        // タイポ修正 (shotAudioSources -> AudioSources)
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