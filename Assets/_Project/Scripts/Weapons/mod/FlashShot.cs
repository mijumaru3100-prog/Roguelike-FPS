using UnityEngine;

public class FlashShot : WeaponAbility
{
    private int count = 1;

    public override void SpecialShot(GunBase baseGun)
    {
        if (count <= 0)
        {
            baseGun.shotAction.shot(baseGun);
            return;
        }

        count--;
        baseGun.AddAmmoAnimated(1,0.1f);

        GameObject b = baseGun.pManager.flashbulletPool.Get();

        // カメラ中央からRayを飛ばす
        Ray ray = baseGun.playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Rayの到達地点
        Vector3 targetPoint = ray.origin + ray.direction * 100f;

        // 何かに当たったら、その地点を到達地点にする
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }

        // 弾は今まで通り銃口から出す
        b.transform.position = baseGun.muzzlePoint.position;

        // 銃口からRayの到達地点へ向ける
        b.transform.rotation = Quaternion.LookRotation(
            targetPoint - b.transform.position
        );

        bullet bulletScript = b.GetComponent<bullet>();

        if (bulletScript != null)
        {
            bulletScript.damage = 0;
            bulletScript.pManager = baseGun.pManager;
        }
    }

    public override void OnCompleteReload()
    {
        count = 1;
    }
}