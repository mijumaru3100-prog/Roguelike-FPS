using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

public class GunBase : MonoBehaviour
{
    public int baseMaxAmmo = 30;
    public virtual int maxAmmo => Mathf.RoundToInt(Mathf.Max(1,(baseMaxAmmo + stats.bonusMaxAmmo) * (1+stats.maxAmmoMultiple)));
    public void RefreshMaxAmmoUI()
    {
        if (isNPC) return;

        if (ammoBeltUI != null)
        {
            ammoBeltUI.InitializeAmmoBelt(currentAmmo, maxAmmo,uiBulletSprite,uiWeaponSprite);
        }
    }
    protected int _currentAmmo;

    public int currentAmmo => _currentAmmo;
    public float defaultRPM = 450;
    public virtual float fireRate
    {
        get
        {
            float totalMult = stats.fireRateMultiple;
            if (pManager != null)
            {
                foreach (var p in pManager.activePassives.ToArray()) totalMult += p.GetFireRateMultiplier(pManager);
            }
            totalMult = Mathf.Max(0.01f, totalMult);
            return ((60f / defaultRPM) / (1+totalMult));
        }
    }
    
    public int baseDamage = 1;
    public int damage
    {
        get
        {
            float totalMult = stats.damageMultiple;
            if (pManager != null)
            {
                foreach (var p in pManager.activePassives.ToArray()) totalMult += p.GetDamageMultiplier(pManager);
            }
            return Mathf.Max(1,Mathf.RoundToInt((baseDamage + stats.bonusDamage) * (1+totalMult)));
        }
    }

    [Header("カメラ反動")]
    public float recoilForce = 0.25f; 
    public float ADSMagnification = 0.5f;
    public float horizontalRecoilForce = 0.1f;

    private float camTargetX;
    private float camCurrentX;
    private float camTargetY;
    private float camCurrentY;

    public float GetTotalReloadSpeedMultiplier()
    {
        float totalMult = stats.reloadSpeedMultiple;
        if (pManager != null)
        {
            foreach (var p in pManager.activePassives.ToArray()) 
            totalMult += p.GetReloadSpeedMultiplier(pManager);
        }
        totalMult = 1 + Mathf.Max(0.01f, totalMult);
        return totalMult;
    }

    [Header("銃反動(見た目) ── 加算+Lerp方式")]
    [Tooltip("後退量(z)、横ブレ(x)、上下(y)")]
    public float kickBack  = 0.05f;
    public float kickUp    = 0.05f;
    public int maxRotX = 10;
    public float kickSide  = 2.0f;
    public int maxRotY = 10;
    [Tooltip("ADS時の銃リコイル倍率")]
    public float gunRotation_adsMagnification = 0.8f;
    [Tooltip("targetが0へ戻る速度")]
    public float returnSpeed   = 8f;
    [Tooltip("currentがtargetへ追従する速度")]
    public float snapSpeed = 16f;

    private Vector3 gunTargetPos;
    private Vector3 gunCurrentPos;
    private Vector3 gunTargetRot;
    private Vector3 gunCurrentRot;

    protected float lastFireTime;

    [Header("設定")]
    public PlayerManager pManager;
    public Camera playerCamera;
    public Transform adsPivot;
    public Transform recoilPivot;
    public CrosshairController crosshair;
    
    [Header("銃パーツのアニメーションリスト")]
    public List<GunPartSet> gunPartSets = new List<GunPartSet>();

    [Header("薬莢排出設定")]
    public bool useEject = false; 
    public enum EjectKind { Normal, ShotGun}
    public EjectKind currentEject = EjectKind.Normal;
    public Transform ejectPoint;
    public Vector2 ejectForceRange = new Vector2(3f, 5f);
    public Vector2 ejectTorqueRange = new Vector2(10f, 20f);

    [Header("UI弾丸画像設定")]
    public Sprite uiWeaponSprite; 
    public Sprite uiBulletSprite;

    [Header("ヒート設定")]
    public float currentHeat = 0f;
    public float heatPerShot = 1f;
    public float coolDownRate = 4f;

    [Header("UI設定")]
    public TextMeshProUGUI ammoText; 
    public AmmoBeltUI ammoBeltUI; 

    [Header("リロードのための武器パーツ")]
    public Transform gunTiltModel;
    public Transform muzzlePoint;
    public Transform magazinePoint;
    public GameObject magazineObject;
    public GameObject MoveArm;
    public GameObject MoveShoulder;
    public GameObject MoveTekubi;
    public List<GameObject>  SpecialParts =new List<GameObject>();

    [Header("プラグイン")]
    public shotMode shotMode;
    public shotAction shotAction; 
    public reloadAnimation reloadAnimation; 
    public PlayerStats stats;

    [SerializeField]
    private ExtraSetting PlugInSetting;
    [Serializable]
    public class ExtraSetting
    {
    public GameObject bulletPrefab;
    public float DefaltReloadTime = 3f;
    }

    [Header("音設定 (個別音源用)")]
    [Tooltip("射撃音用の音源。複数ある場合は交互に再生され、連射時の音途切れを防ぎます。")]
    public List<AudioSource> shotAudioSources = new List<AudioSource>();
    [Tooltip("リロード音（マガジン抜差、叩き込み、コッキング等）用の音源")]
    public AudioSource reloadAudioSource;
    [Tooltip("空撃ち（弾切れ）音用の音源")]
    public AudioSource dryFireAudioSource;
    [Tooltip("残弾減少時のキンキン金属音用の音源")]
    public AudioSource kinKinAudioSource;

    [Header("音設定")]
    [SerializeField] private AudioClip[] ShotSounds;
    
    [SerializeField, Range(0, 2)] private float pitchRandomness = 0.1f;

    [Header("----------空撃ち設定----------")]
    public AudioClip dryFireSound;

    [Header("----------残弾演出設定----------")]
    [SerializeField] private AudioClip kinKinSound;

    [Header("キンキン演出の調整（ミキサー）")]
    [Range(0f, 1f)] 
    public float changeStartThreshold = 0.3f;

    [Range(0.5f, 2.0f)] 
    public float maxPitchShift = 1.1f;

    [Range(0f, 1f)] 
    public float maxKinKinVolume = 0.8f;

    [Range(0f, 0.05f)]
    public float kinKinDelay = 0.02f;

    [Header("マズルフラッシュ設定（ライト式）")]
    public Light muzzleFlashLight;
    public float flashDuration = 0.05f;
    public float maxIntensity = 15f;

    [Header("覗き込み（ADS）設定")]
    public Vector3 adsPosition;
    public float adsSpeed = 0.1f;
    public float adsFieldOfView = 40f;
    public Vector3 adsRotation;

    [Header("銃取得時の配置設定")]
    public Vector3 offsetPosition;
    public Vector3 offsetRotation;

    [Header("Bobbing")]
    public float bobbingAmount = 0.05f;
    public float sideBobbingAmount = 0.03f;
    public float BackwardOffset = 0.1f;
    public float swayAmount = 1.5f;

    private Vector3 defaultGunPosition;
    private Vector3 defaultGunRotation;
    private float defaultFOV;
    public bool isAiming = false;
    public bool isReloading = false;

    public bool isNPC = false;
    [SerializeField]private bool useMuzzleFlashLight = true;
    
    protected virtual void Start()
    {
        if(isNPC)
        {
            if (pManager == null)
            {
                pManager = GameObject.FindObjectOfType<PlayerManager>();
            }
            return;
        }

        isReloading = false;
        if(!isNPC)
        {
            _currentAmmo = maxAmmo;
            crosshair = pManager.crosshair;
        
        defaultGunPosition = adsPivot.localPosition;
        defaultGunRotation = adsPivot.localEulerAngles;
        foreach (var set in gunPartSets) set.Init();

        if (playerCamera == null) 
        {
            playerCamera = Camera.main;
        }

        if (playerCamera != null)
        {
            defaultFOV = playerCamera.fieldOfView;

            var ml = playerCamera.GetComponent<MouseLook>();
            if (ml != null) ml.useDirectRecoil = true;
        }

        if (ammoBeltUI != null && isNPC == false)
        {
            ammoBeltUI.InitializeAmmoBelt(currentAmmo, maxAmmo,uiBulletSprite,uiWeaponSprite);
        }
        UpdateAmmoDisplay();
        }
        else
        {
            _currentAmmo = baseMaxAmmo;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        if(isNPC){return;}

        if (shotMode != null && shotMode.IsFiring())
        {
           tryFire();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(tryReload());
        }

        HandleADS();
        UpdateRecoil();

        if (currentHeat > 0) 
        {
            currentHeat = Mathf.Max(0, currentHeat - coolDownRate * Time.deltaTime);
        }
    }

    public void tryFire()
    {
        if (isReloading)
        {
            return;
        }

        if(Time.time < lastFireTime + fireRate)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            if (dryFireSound != null)
            {
                AudioSource source = dryFireAudioSource != null ? dryFireAudioSource : (reloadAudioSource != null ? reloadAudioSource : GetComponent<AudioSource>());
                if (source != null)
                {
                    source.pitch = 1.2f; 
                    source.PlayOneShot(dryFireSound);
                }
            }

            StartCoroutine(tryReload());
            return;
        }
        fire();
    }

    protected virtual void fire()
    {
       if(isNPC == false)
        {
            foreach (var p in pManager.activePassives.ToArray())
            {
                p.OnBeforeShot(pManager);
            }
        }
        
        if (shotAction == null && isNPC == false) 
        {
            Debug.Log("shotactionが未設定...だ、よ。ままならないね...");
            return;
        }

        _currentAmmo --;
        lastFireTime = Time.time;

        if(isNPC == false)
        {
            UpdateAmmoDisplay();
            ApplyGunRecoil();
        }

        shotAction.shot(this);
        currentHeat += heatPerShot;
        PlayShotSound();
        PlayMuzzleFlash();
        
        if(!isNPC)
        {
            foreach (var set in gunPartSets)
            {
                if (set.actionData != null)
                {
                    set.actionData.Execute(this,set.part, set.defaultPos, set.defaultRot);
                }
            }

             if (crosshair != null)
            {
                crosshair.AddSpread(10f); 
            }

            foreach (var p in pManager.activePassives.ToArray())
            {
                p.OnShotComplete(pManager);
            }
        }

        if(useEject) EjectShell();
    }
    
    private void UpdateRecoil()
    {
        if (isNPC) return;

        if (recoilPivot != null)
        {
            gunTargetPos = Vector3.Lerp(gunTargetPos, Vector3.zero, returnSpeed * Time.deltaTime);
            gunTargetRot = Vector3.Lerp(gunTargetRot, Vector3.zero, returnSpeed * Time.deltaTime);

            gunCurrentPos = Vector3.Lerp(gunCurrentPos, gunTargetPos, snapSpeed * Time.deltaTime);
            gunCurrentRot = Vector3.Lerp(gunCurrentRot, gunTargetRot, snapSpeed * Time.deltaTime);

            recoilPivot.localPosition = gunCurrentPos;
            recoilPivot.localRotation = Quaternion.Euler(gunCurrentRot);
        }

        var mouseLook = playerCamera != null ? playerCamera.GetComponent<MouseLook>() : null;
        if (mouseLook != null && mouseLook.useCameraRecoil)
        {
            camTargetX = Mathf.Lerp(camTargetX, 0f, returnSpeed * Time.deltaTime);
            camTargetY = Mathf.Lerp(camTargetY, 0f, returnSpeed * Time.deltaTime);
            camCurrentX = Mathf.Lerp(camCurrentX, camTargetX, snapSpeed * Time.deltaTime);
            camCurrentY = Mathf.Lerp(camCurrentY, camTargetY, snapSpeed * Time.deltaTime);
            mouseLook.SetRecoilDirect(camCurrentX, camCurrentY);
        }
    }

    public void ApplyGunRecoil()
    {
        if (recoilPivot == null || isNPC) return;

        float adsScale = isAiming ? gunRotation_adsMagnification : 1f;
        float side = Random.Range(-kickSide, kickSide) * adsScale;

        gunTargetPos += new Vector3(0f, 0f, -kickBack * adsScale);

        gunTargetRot += new Vector3(kickUp * adsScale, side, 0f);

        gunTargetRot.x = Mathf.Clamp(gunTargetRot.x, -maxRotX, maxRotX);
        gunTargetRot.y = Mathf.Clamp(gunTargetRot.y, -maxRotY, maxRotY);

        ApplyCameraRecoil();
    }

    public void ApplyCameraRecoil()
    {
        if (isNPC) return;

        float fX = recoilForce;
        float fY = horizontalRecoilForce;
        if (isAiming)
        {
            fX *= ADSMagnification;
            fY *= ADSMagnification;
        }
        camTargetX -= fX;
        camTargetY += Random.Range(-fY, fY);
    }
    
    private int currentIndex = 0;
    public void PlayShotSound()
    {
        if (ShotSounds == null || ShotSounds.Length == 0) return;

        AudioSource source = null;
        if (shotAudioSources != null && shotAudioSources.Count > 0)
        {
            source = shotAudioSources[currentIndex];
            currentIndex = (currentIndex + 1) % shotAudioSources.Count;
        }
        else
        {
            source = GetComponent<AudioSource>();
        }

        if (source == null) return;

        AudioClip clipToPlay = ShotSounds[Random.Range(0, ShotSounds.Length)];

        float ammoRatio = (float)currentAmmo / maxAmmo;
        float dynamicPitch = 1.0f;
        float kinKinVolume = 0f;

        if (ammoRatio < changeStartThreshold)
        {
            float effectProgress = 1.0f - (ammoRatio / changeStartThreshold);
            dynamicPitch = Mathf.Lerp(1.0f, maxPitchShift, effectProgress);
            kinKinVolume = effectProgress * maxKinKinVolume;
        }

        source.clip = clipToPlay;
        
        source.pitch = (1.0f + Random.Range(-pitchRandomness, pitchRandomness)) * dynamicPitch;
        
        source.Play();

        if (kinKinVolume > 0 && kinKinSound != null)
        {
            StartCoroutine(DelayedKinKin(kinKinVolume));
        }
    }

    private IEnumerator DelayedKinKin(float volume)
    {
        yield return new WaitForSeconds(kinKinDelay);
        
        AudioSource source = kinKinAudioSource != null ? kinKinAudioSource : (shotAudioSources != null && shotAudioSources.Count > 0 ? shotAudioSources[0] : GetComponent<AudioSource>());
        if (source != null)
        {
            source.PlayOneShot(kinKinSound, volume);
        }
    }

    public void PlayReloadSound(AudioClip clip)
    {
        AudioSource source = reloadAudioSource != null ? reloadAudioSource : GetComponent<AudioSource>();
        if (clip == null || source == null) return;
        
        source.pitch = 1.0f;
        source.PlayOneShot(clip, 1.0f);
    }

    protected void EjectShell()
    {
        if(ejectPoint == null)
        {
            Debug.Log("薬莢の排出がままならないね...");
            return;
        }

       GameObject shell;
        if(currentEject == EjectKind.ShotGun) {shell = pManager.ShotGunShellPool.Get();}
        else {shell = pManager.NormalShellPool.Get();}
        
        shell.transform.position = ejectPoint.position;
        shell.transform.rotation = ejectPoint.rotation;
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if(shell == null){Debug.Log("シェルわかんないね...");} 
       
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Vector3 force = ejectPoint.right * Random.Range(ejectForceRange.x, ejectForceRange.y) 
                        + ejectPoint.up * Random.Range(1f, 2f);
            
            rb.AddForce(force, ForceMode.Impulse);
       }

        StartCoroutine(ReturnShellAfterTime(shell, 3.0f));
    }

    private IEnumerator ReturnShellAfterTime(GameObject shell, float delay)
    {
        yield return new WaitForSeconds(delay);

        if(currentEject == EjectKind.ShotGun){pManager.ShotGunShellPool.ReturnToPool(shell);}
        else{pManager.NormalShellPool.ReturnToPool(shell);}
    }

    public void PlayMuzzleFlash()
    {
        if (muzzleFlashLight == null) return;
        if (useMuzzleFlashLight == false) return;

        muzzleFlashLight.enabled = true;
        muzzleFlashLight.intensity = maxIntensity;

        StartCoroutine(ExtinguishFlash());
    }

    private IEnumerator ExtinguishFlash()
    {
        yield return new WaitForSeconds(flashDuration);
    
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false;
        }
   }

   void HandleADS()
   {
        if(isNPC){return;}

        if(isReloading)
        {
            if (isAiming)
            {
                isAiming = false;
                crosshair.SetVisible(true);
                PlayADSTween(defaultGunPosition, defaultFOV, defaultGunRotation);

                foreach (var p in pManager.activePassives.ToArray())
                {
                    p.OnADSEnd(pManager);
                }
            }
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            crosshair.SetVisible(false);
            PlayADSTween(adsPosition, adsFieldOfView,adsRotation);
            
            foreach (var p in pManager.activePassives.ToArray())
            {
                p.OnADSStart(pManager);
            }
        }
    
        if (Input.GetMouseButtonUp(1))
        {
            crosshair.SetVisible(true);
            isAiming = false;
            PlayADSTween(defaultGunPosition, defaultFOV,defaultGunRotation);

            foreach (var p in pManager.activePassives.ToArray())
            {
                p.OnADSEnd(pManager);
            }
        }
    }

    void PlayADSTween(Vector3 targetPos, float targetFOV,Vector3 targetLotate)
    {
        adsPivot.DOLocalMove(targetPos, adsSpeed).SetEase(Ease.OutQuad);
        adsPivot.DOLocalRotate(targetLotate, adsSpeed).SetEase(Ease.OutQuad);
    
        playerCamera.DOFieldOfView(targetFOV, adsSpeed).SetEase(Ease.OutQuad);
    }

    IEnumerator tryReload()
    {
        if(isReloading) yield break;
        
        isReloading = true;
        
        if(!isNPC && reloadAnimation)
        {
           foreach (var p in pManager.activePassives.ToArray())
           {
               p.OnBeforeReload(pManager);
           }
            reloadAnimation.Play(this);
        }
        else
        {
            yield return new WaitForSeconds(PlugInSetting.DefaltReloadTime);
            OnReloadComplete();
        }
    }

    public void OnMagazineEjected()
    {
        _currentAmmo = 0;
        UpdateAmmoDisplay();
    }

    public void OnReloadComplete()
    {
        
        if (!isNPC)
        {
            _currentAmmo = maxAmmo;
            UpdateAmmoDisplay();
        }
        else
        {
            _currentAmmo = baseMaxAmmo;
        }
        isReloading = false;
        
        if (pManager != null && pManager.activePassives!= null && !isNPC)
        {
            foreach (var p in pManager.activePassives.ToArray())
            {
                p.OnReloadComplete(pManager);
            }
        }
    }

   private Coroutine ammoChangeCoroutine;
   private int targetAnimatedAmmo;

public void StartReloadCountAnimation(float restTime)
{
    if (ammoChangeCoroutine != null)
    {
        StopCoroutine(ammoChangeCoroutine);
    }

    ammoChangeCoroutine = StartCoroutine(AmmoChangeAnimation(currentAmmo, maxAmmo, restTime));
}

public void StopReloadCountAnimation()
{
    if (ammoChangeCoroutine != null)
    {
        StopCoroutine(ammoChangeCoroutine);
        ammoChangeCoroutine = null;
    }
}

IEnumerator AmmoChangeAnimation(int startAmmo, int targetAmmo, float duration)
{
    int currentDisplay = startAmmo;
    int diff = targetAmmo - startAmmo;
    
    if (diff <= 0) 
    {
        UpdateAmmoDisplay();
        yield break;
    }

    float interval = duration / diff;

    while (currentDisplay < targetAmmo)
    {
        currentDisplay++;
        _currentAmmo = currentDisplay; 
        
        UpdateAmmoDisplay();

        yield return new WaitForSeconds(interval);
    }

    ammoChangeCoroutine = null;
}

public void AddAmmoAnimated(int amountToAdd, float duration)
{
    if (ammoChangeCoroutine != null)
    {
        StopCoroutine(ammoChangeCoroutine);
    }
    else
    {
        targetAnimatedAmmo = currentAmmo;
    }
    
    targetAnimatedAmmo += amountToAdd;
    
    ammoChangeCoroutine = StartCoroutine(AmmoChangeAnimation(currentAmmo, targetAnimatedAmmo, duration));
}

public void UpdateAmmoDisplay()
{
if(isNPC){return;}

if (ammoText != null)
{
ammoText.text = $"{currentAmmo} / {maxAmmo}";

if (currentAmmo <= maxAmmo * 0.1)
{
ammoText.color = pManager.LowTextColor;
}
else if (currentAmmo <= maxAmmo * 0.5f)
{
ammoText.color = pManager.HarfTextColor;
}
else
{
ammoText.color = pManager.DefaultTextColor;
}

if (ammoBeltUI != null)
{
ammoBeltUI.SynchronizeAmmoUI(currentAmmo, maxAmmo, isReloading, ammoText.color);
}
}
}
}