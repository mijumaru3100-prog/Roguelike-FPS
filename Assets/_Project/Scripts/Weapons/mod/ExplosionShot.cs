using UnityEngine;

public class ExplosionShot : WeaponAbility
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

        baseGun.AddAmmoAnimated(1, 0.1f);

        GameObject b =
            baseGun.pManager.explosionBulletPool.Get();

        Ray ray =
            baseGun.playerCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f)
            );

        Vector3 targetPoint =
            ray.origin + ray.direction * 100f;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }

        b.transform.position =
            baseGun.muzzlePoint.position;

        b.transform.rotation =
            Quaternion.LookRotation(
                targetPoint - b.transform.position
            );

        bullet bulletScript =
            b.GetComponent<bullet>();

        if (bulletScript != null)
        {
            bulletScript.damage = 0;
            bulletScript.pManager = baseGun.pManager;
        }
    }

    public override void OnDamaged()
    {
        count = 1;
    }
}