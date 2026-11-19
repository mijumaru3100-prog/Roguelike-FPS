using UnityEngine;
using System.Collections;
public class bullet : MonoBehaviour
{
    public float baseBulletSpeed = 5;
    private float bulletSpeed;
    public float baseLifeTime = 5f;
   private float lifeTime;
    public int baseThrouthCount = 1;
    private int currentThrouthCount;
    public int damage = 1;
    public PlayerManager pManager;
    public ParticleSystem effect; 
    public Light light;
    public int FlashIntensity;
    [SerializeField] private Collider bulletCollider;
    
    private bool isHit = false;
        protected virtual void OnEnable()
    {
        if(bulletCollider != null) bulletCollider.enabled = true;
        if(effect != null)effect.Play();
        isHit = false;
        
        //if (pManager != null && pManager.sharedStats != null)
        //{
        //    bulletSpeed = (baseBulletSpeed + pManager.sharedStats.bonusBulletSpeed) * pManager.sharedStats.bulletSpeedMultiple;
        //    lifeTime = (baseLifeTime + pManager.sharedStats.bonusLifeTime) * pManager.sharedStats.lifeTimeMultiple;
        //   currentThrouthCount = (int)((baseThrouthCount + pManager.sharedStats.bonusThrouthCount) * pManager.sharedStats.throuthCountMultiple);
        //}
        //else
        //{
            bulletSpeed = baseBulletSpeed;
            lifeTime = baseLifeTime;
            currentThrouthCount = baseThrouthCount;
        //}
        StopAllCoroutines();
        StartCoroutine(LifeTimeRoutine(lifeTime));
    }
    private IEnumerator LifeTimeRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        DeactivateBullet();
    }
    void Update()
    {
        if(!isHit)transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        isHit = true;
        
        EnemyHP EnemyHealth = other.GetComponentInParent<EnemyHP>();
        PlayerHP PlayerHealth = other.GetComponentInParent<PlayerHP>();
        int Hphit = 0;
        //if (EnemyHealth != null)
        //{
        //    EnemyHealth.OnHitBullet(damage, other); 
        //    if(pManager != null && pManager.crosshair != null) pManager.crosshair.OnHit();
        //}
        if (PlayerHealth != null)
        {
            PlayerHealth.TakeDamage(damage); 
            Hphit++;
        }
        currentThrouthCount--;
        if(currentThrouthCount <= 0)
        {
        //    if(Hphit == 0 && pManager != null)
        //    {
        //        foreach(var p in pManager.activePassives)
        //        {
        //            p.OnMiss(pManager);
        //        }
        //    }
            DeactivateBullet();
        }
    }

protected void DeactivateBullet()
{
    StartCoroutine(DeactivateRoutine());
}

private IEnumerator DeactivateRoutine()
{
    isHit = true;
    bulletCollider.enabled = false;

    if(light != null)yield return StartCoroutine(FlashLight());

    if (pManager != null && pManager.bulletPool != null)
        pManager.bulletPool.ReturnToPool(gameObject);
    else
        Destroy(gameObject);
}
    void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator FlashLight()
{
    light.enabled = true;
    light.intensity = FlashIntensity;

    yield return new WaitForSeconds(0.03f);

    float elapsed = 0f;

    while (elapsed < 0.1f)
    {
        elapsed += Time.deltaTime;
        light.intensity = Mathf.Lerp(FlashIntensity, 0f, elapsed / 0.1f);
        yield return null;
    }

    light.intensity = 0f;
    light.enabled = false;
}
}