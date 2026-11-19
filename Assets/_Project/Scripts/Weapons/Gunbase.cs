using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

public class GunBase : MonoBehaviour
{
    [Header("基本性能")]
    public int baseMaxAmmo = 30;
    public virtual int maxAmmo
{
    get
    {
        if(AI!=null) return baseMaxAmmo;

        float ammoBonus = stats.bonusMaxAmmo;
        float ammoMultiple = stats.maxAmmoMultiple;


        return Mathf.RoundToInt(
            Mathf.Max(
                1,
                (baseMaxAmmo + ammoBonus) * (1 + ammoMultiple)
            )
        );
    }
}
    public void RefreshMaxAmmoUI()
    {
        if (AI != null) return;

        if (ammoBeltUI != null)
        {
            ammoBeltUI.InitializeAmmoBelt(currentAmmo, maxAmmo,uiBulletSprite,uiWeaponSprite);
        }
    }
    [SerializeField]protected int _currentAmmo;
    public int currentAmmo => _currentAmmo;

    public float defaultRPM = 450;
    public virtual float fireRate
{
    get
    {
        if(AI!=null) return (60f / defaultRPM);

        float totalMult = stats.fireRateMultiple;

        if (pManager != null)
        {
            foreach (var p in pManager.activePassives.ToArray())
            {
                totalMult += p.GetFireRateMultiplier(pManager);
            }
        }

        totalMult = Mathf.Max(0.01f, totalMult);

        return (60f / defaultRPM) / (1 + totalMult);
    }
}
    
    public int baseDamage = 1;
    public int damage
{
    get
    {
        if(AI!=null) return baseDamage;

        float bonusDamage = stats.bonusDamage;
        float totalMult = stats.damageMultiple;

        if (pManager != null)
        {
            foreach (var p in pManager.activePassives.ToArray())
            {
                totalMult += p.GetDamageMultiplier(pManager);
            }
        }

        return Mathf.Max(
            1,
            Mathf.RoundToInt(
                (baseDamage + bonusDamage) * (1 + totalMult)
            )
        );
    }
}

    public float knockbackForce = 5f;

    [Header(" カメラ反動 ")]
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
        {
            totalMult += p.GetReloadSpeedMultiplier(pManager);
        }
    }

    totalMult = 1 + Mathf.Max(0.01f, totalMult);

    return totalMult;
}

    [Header("射撃FOV演出")]
public bool useShotFOV = true;
public float shotFOVKick = 2.5f;
public float shotFOVInDuration = 0.04f;
public float shotFOVOutDuration = 0.15f;

private Tween shotFOVTween;

    [Header("銃の見た目反動 ")]
    public float kickBack = 0.05f;
    public float kickUp = 0.05f;
    public int maxRotX = 10;
    public float kickSide = 2.0f;
    public int maxRotY = 10;
     public float gunRotation_adsMagnification = 0.8f;
    public float returnSpeed = 8f;
     public float snapSpeed = 16f;

    private Vector3 gunTargetPos;
    private Vector3 gunCurrentPos;
    private Vector3 gunTargetRot;
    private Vector3 gunCurrentRot;

    protected float lastFireTime;

    [Header("ADS設定 ")]
    public Vector3 adsPosition;
    public Vector3 adsRotation;
    public float adsSpeed = 0.1f;
    public float adsFieldOfView = 40f;

    [Header("配置・武器揺れ設定 ")]
    public Vector3 offsetPosition;
     public Vector3 offsetRotation;
    public float bobbingAmount = 0.05f;
    public float sideBobbingAmount = 0.03f;
    public float BackwardOffset = 0.1f;
    public float swayAmount = 1.5f;
    

    [Header("薬莢排出設定")]
    public bool useEject = false; 
    public enum EjectKind { Normal, ShotGun}
     public EjectKind currentEject = EjectKind.Normal;
     public Transform ejectPoint;
    public Vector2 ejectForceRange = new Vector2(3f, 5f);

    [Header("ヒート設定 ")]
    public float heatPerShot = 1f;
    public float coolDownRate = 4f;

    [Header("マズルフラッシュ設定 ")]
    [SerializeField] private bool useMuzzleFlashLight = true;
    public Light muzzleFlashLight;
    public float flashDuration = 0.05f;
    public float maxIntensity = 15f;
    public List<ParticleSystem> FlashParticles = new List<ParticleSystem>();

    [Header("オーディオ設定")]
    public SoundsPlayer ShotSound;
    public List<AudioSource> shotAudioSources = new List<AudioSource>();
    public AudioSource reloadAudioSource;
    public AudioSource dryFireAudioSource;
    public AudioSource kinKinAudioSource;
    public AudioClip dryFireSound;

    [Header("残弾演出設定")]
    [SerializeField] private AudioClip kinKinSound;
    public float changeStartThreshold = 0.3f;
    public float maxPitchShift = 1.1f;
    public float maxKinKinVolume = 0.8f;
    public float kinKinDelay = 0.02f;
    public float kinKinVolume = 1.0f;

    [Header("武器パーツ参照")]
    public Transform gunTiltModel;
     public Transform muzzlePoint;
    public Transform magazinePoint;
    public GameObject magazineObject;
    public GameObject MoveArm;
    public GameObject MoveTekubi;
    public List<GameObject> SpecialParts = new List<GameObject>();
    public List<GunPartSet> gunPartSets = new List<GunPartSet>();

    [Header("UI設定")]
    public Sprite uiWeaponSprite; 
    public Sprite uiBulletSprite;

    [Header("プラグイン")]
    public shotMode shotMode;
    public shotAction shotAction; 
    public reloadAnimation reloadAnimation; 
    [SerializeField] private ExtraSetting PlugInSetting;

    [Header("モディファイア")]
    [SerializeField] private WeaponAbility _localAbility;
    public WeaponAbility ability
    {
        get
        {
            if (pManager != null && pManager.currentAbility != null)
                return pManager.currentAbility;
            return _localAbility;
        }
        set
        {
            _localAbility = value;
            if (pManager != null && value != null)
            {
                pManager.currentAbility = value;
            }
        }
    }

    [Serializable]
    public class ExtraSetting
    {
        [Tooltip("デフォルトリロード時間")]
        public float DefaltReloadTime = 3f;
    }

    [Header("=== その他設定 ===")]
    [Tooltip("NPCAI")] public NormalEnemyAI AI;

    // --- 実行時・外部注入変数（インスペクタ非表示） ---
     public PlayerManager pManager;
    [HideInInspector] public Camera playerCamera;
    [HideInInspector] public Transform adsPivot;
    [HideInInspector] public Transform recoilPivot;
    [HideInInspector] public CrosshairController crosshair;
    [HideInInspector] public TextMeshProUGUI ammoText; 
    [HideInInspector] public AmmoBeltUI ammoBeltUI; 
    [HideInInspector] public PlayerStats stats;
    [HideInInspector] public bool isAiming = false;
    [HideInInspector] public bool isReloading = false;
    [HideInInspector] public float currentHeat = 0f;

    private Vector3 defaultGunPosition;
    private Vector3 defaultGunRotation;
    private float defaultFOV;
    
    protected virtual void Start()
    {
        if(AI != null)
        {
            if (pManager == null)
            {
                pManager = AI.pManager;
            }
            return;
        }

        isReloading = false;
        _currentAmmo = maxAmmo;
        if(AI == null)
        {

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

        if (ammoBeltUI != null && AI != null == false)
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
    if (AI != null) return;

    if (pManager == null) return;
    if (pManager.IsTitle) return;
    if (Time.timeScale == 0) return;

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
        currentHeat = Mathf.Max(
            0,
            currentHeat - coolDownRate * Time.deltaTime
        );
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

    public IEnumerator AllFire()
{
    while (!isReloading)
    {
        tryFire();

        yield return new WaitForSeconds(fireRate);
    }
}

    protected virtual void fire()
    {
       if(AI == null)
        {
            foreach (var p in pManager.activePassives.ToArray())
            {
                p.OnBeforeShot(pManager);
            }
        }
        
        if (shotAction == null) 
        {
            Debug.Log("shotactionが未設定...だ、よ。ままならないね...");
        }

        _currentAmmo --;
        lastFireTime = Time.time;

        if(AI == null)
        {
            UpdateAmmoDisplay();
            ApplyGunRecoil();
            ApplyShotFOV();
        }
        

        //多くのmodifierではshotAction.shot(this);となる
        if(ability != null && AI ==null) ability.SpecialShot(this);
        else shotAction.shot(this);

        currentHeat += heatPerShot;
        PlayShotSound();
        PlayMuzzleFlash();
        
        if(AI == null)
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
        if (AI != null) return;

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
        if (recoilPivot == null || AI != null) return;

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
        if (AI != null) return;

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

    private void ApplyShotFOV()
{
    if (AI != null || !useShotFOV || playerCamera == null)
        return;

    shotFOVTween?.Kill();

    float baseFOV = isAiming ? adsFieldOfView : defaultFOV;
    float targetFOV = baseFOV + shotFOVKick;

    playerCamera.fieldOfView = targetFOV;

    shotFOVTween = playerCamera
        .DOFieldOfView(baseFOV, shotFOVOutDuration)
        .SetEase(Ease.OutQuad);
}
    
    private int currentIndex = 0;
    public void PlayShotSound()
    {
        ShotSound.PlaySound(this);

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

        foreach(var p in FlashParticles)
        {
            p.Play();
        }

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

        if(AI != null){return;}

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
        
        if(AI == null && reloadAnimation)
        {
           foreach (var p in pManager.activePassives.ToArray())
           {
               p.OnBeforeReload(pManager);
           }
            reloadAnimation.Play(this);
        }
        else
        {
            AI.attackCoroutine = null;
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
        
        if (AI == null)
        {
            _currentAmmo = maxAmmo;
            UpdateAmmoDisplay();
        }
        else
        {
            _currentAmmo = baseMaxAmmo;
        }
        isReloading = false;
        
        if (pManager != null && pManager.activePassives!= null && AI == null)
        {
            foreach (var p in pManager.activePassives.ToArray())
            {
                p.OnReloadComplete(pManager);
            }
        }

        if(ability != null && AI == null) ability.OnCompleteReload();
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
if(AI != null){return;}

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