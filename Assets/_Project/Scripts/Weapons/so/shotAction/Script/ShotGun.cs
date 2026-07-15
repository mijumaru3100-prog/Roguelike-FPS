using UnityEngine;
using DG.Tweening;
[CreateAssetMenu(menuName = "Gun/Action/shotGun")]
public class shotGunAction : shotAction
{ 
    public int palletCount = 8; 
    public float spreadAngle = 10.0f; 
    public override void shot(GunBase baseGun)
    {
        Camera cam = baseGun.playerCamera;
        Vector3 startPos = baseGun.muzzlePoint != null 
                           ? baseGun.muzzlePoint.position 
                           : cam.transform.position;
        int mask = ~LayerMask.GetMask("Ignore Raycast", "invisibleWall");
        for (int i = 0; i < palletCount; i++)
        {
            Ray ray = cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            Quaternion spread = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0
            );
            Vector3 fireDirection = spread * ray.direction;
            Vector3 endPos;
            if (Physics.Raycast(startPos, fireDirection, out RaycastHit hit, 100f, mask, QueryTriggerInteraction.Collide))
            {
                endPos = hit.point;
                MakeHitEffect(baseGun.pManager, hit, ray.direction);
                EnemyHP targetHealth = hit.collider.GetComponentInParent<EnemyHP>();
                if (targetHealth != null && !baseGun.isNPC)
                {
                    targetHealth.OnHitBullet(baseGun.damage, hit.collider); 
                    baseGun.pManager.crosshair.OnHit();
                }
            }
            else
            {
                endPos = startPos + (fireDirection * 100f);
                foreach(var p in baseGun.pManager.activePassives)
                {
                    p.OnMiss(baseGun.pManager);
                }
            }
            if (baseGun.pManager.tracerPool != null)
            {
                GameObject t = baseGun.pManager.tracerPool.Get();
                LineRenderer lr = t.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.alignment = LineAlignment.View;
                    lr.startWidth = 0.08f; 
                    lr.endWidth = 0.08f; 
                    lr.SetPosition(0, startPos);
                    lr.SetPosition(1, endPos);
                    DOTween.To(() => lr.startWidth, x => lr.startWidth = x, 0f, 0.05f);
                    DOTween.To(() => lr.endWidth, x => lr.endWidth = x, 0f, 0.08f)
                           .OnComplete(() => baseGun.pManager.tracerPool.ReturnToPool(t));
                }
            }
        }
        if (baseGun != null)
        {
             baseGun.ApplyCameraRecoil();
        }
    }
}