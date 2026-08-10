using UnityEngine;
using DG.Tweening; 
using System.Collections;

[CreateAssetMenu(menuName = "Gun/Action/reserveShot")]
public class reserveShot : shotAction
{ 
    public override void shot(GunBase baseGun)
    {
        Camera cam = baseGun.playerCamera;
        
        Vector3 startPos = baseGun.muzzlePoint != null 
                           ? baseGun.muzzlePoint.position 
                           : cam.transform.position;

        Ray ray = cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        Vector3 endPos;

        int mask = ~LayerMask.GetMask("Ignore Raycast", "invisibleWall");

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask, QueryTriggerInteraction.Collide))
        {
            endPos = hit.point;
            MakeHitEffect(baseGun.pManager, hit, ray.direction);
            EnemyHP targetHealth = hit.collider.GetComponentInParent<EnemyHP>();

            if (targetHealth != null && !baseGun.isNPC)
            {
                targetHealth.OnHitBullet(baseGun.damage, hit.collider); 
                baseGun.pManager.crosshair.OnHit();

                baseGun.AddAmmoAnimated(1, 0.1f);
                
                baseGun.StartCoroutine(ReserveAmmoRoutine(baseGun));
            }
            else
            {
                MakeHitMark(baseGun.pManager, hit, ray.direction);
            }
        }
        else
        {
            endPos = ray.origin + (ray.direction * 100f);
            foreach(var p in baseGun.pManager.activePassives)
            {
                p.OnMiss(baseGun.pManager);
            }
        }

        if (baseGun.pManager.tracerPool != null)
        {
            GameObject t = baseGun.pManager.tracerPool.Get();
            t.transform.position = Vector3.zero;
            LineRenderer lr = t.GetComponent<LineRenderer>();
            
            if (lr != null)
            {
                lr.alignment = LineAlignment.View;

                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f; 
                
                lr.SetPosition(0, startPos);
                lr.SetPosition(1, endPos);

                DOTween.To(() => lr.startWidth, x => lr.startWidth = x, 0f, 0.05f);
                DOTween.To(() => lr.endWidth, x => lr.endWidth = x, 0f, 0.08f)
                       .OnComplete(() => baseGun.pManager.tracerPool.ReturnToPool(t));
            }
        }

        if (baseGun != null)
        {
             baseGun.ApplyCameraRecoil();
        }
    }
    private IEnumerator ReserveAmmoRoutine(GunBase baseGun)
    {
        yield return new WaitForSeconds(baseGun.fireRate);

        if (baseGun != null)
        {
            baseGun.UpdateAmmoDisplay();
        }
    }
    
}