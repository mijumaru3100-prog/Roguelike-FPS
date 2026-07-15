using UnityEngine;
using DG.Tweening; 
using System.Collections;

[CreateAssetMenu(menuName = "Gun/Action/reserveShot")]
public class reserveShot : shotAction
{ 
    // gunbase_saigenのfireから呼び出される、よ
    public override void shot(GunBase baseGun)
    {
        Camera cam = baseGun.playerCamera;
        
        // 1. スタート地点を「銃口」にする。もし未設定ならカメラから。
        Vector3 startPos = baseGun.muzzlePoint != null 
                           ? baseGun.muzzlePoint.position 
                           : cam.transform.position;

        // 2. 狙い（レティクル）の先を計算
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
                baseGun.currentAmmo++;
                baseGun.StartCoroutine(ReserveAmmoRoutine(baseGun));
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
            // 3. 生成して、向きを着弾点へ向ける
            GameObject t = baseGun.pManager.tracerPool.Get(); // 借りる
            t.transform.position = Vector3.zero; // 初期化が必要な場合はここで
            LineRenderer lr = t.GetComponent<LineRenderer>();
            

            if (lr != null)
            {
                // カメラの方を向くように設定（これで回転の違和感が消えるはず）
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
        // baseGun.fireRate秒 待つ
        yield return new WaitForSeconds(baseGun.fireRate);

        // 待機後に表示を更新
        if (baseGun != null) // 待っている間に武器が破棄された場合の安全対策
        {
            baseGun.UpdateAmmoDisplay();
        }
    }
    
}