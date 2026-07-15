using UnityEngine;
using System.Collections;
public class bullet : MonoBehaviour
{
    public float baseBulletSpeed = 5;
    public float bulletSpeed;
    public float baseLifeTime = 5f;
   public float lifeTime;
    public int baseThrouthCount = 1;
    public int currentThrouthCount;
    public float damage = 1;
    public PlayerManager pManager;
    void OnEnable()
    {
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
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        
        EnemyHP EnemyHealth = other.GetComponentInParent<EnemyHP>();
        PlayerHP PlayerHealth = other.GetComponentInParent<PlayerHP>();
        int Hphit = 0;
        //if (EnemyHealth != null)
        //{
        //    EnemyHealth.OnHitBullet(damage, other); 
        //    Hphit++;
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
    private void DeactivateBullet()
    {
        if (pManager != null && pManager.bulletPool != null)
        {
            pManager.bulletPool.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }
}