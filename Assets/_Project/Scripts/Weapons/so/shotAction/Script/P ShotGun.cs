using UnityEngine;
using DG.Tweening;
[CreateAssetMenu(menuName = "Gun/Action/P_shotGun")]
public class P_shotGunAction : shotAction
{ 
    public int palletCount = 8; 
    public float spreadAngle = 10.0f; 
    public override void shot(GunBase baseGun)
    {
        for(int i=0;i<palletCount ;i++)
        {
        GameObject b =baseGun.pManager.bulletPool.Get();
        b.transform.position = baseGun.muzzlePoint.position;
        
        if(i != 1)
        {
        float randomPitch = Random.Range(-spreadAngle, spreadAngle);
        float randomYaw = Random.Range(-spreadAngle, spreadAngle);
            
        Quaternion spreadRotation = Quaternion.Euler(randomPitch, randomYaw, 0);
        b.transform.rotation = baseGun.muzzlePoint.rotation * spreadRotation;
        }
        else
        {
            b.transform.rotation = baseGun.muzzlePoint.rotation;
        }
        bullet bulletScript = b.GetComponent<bullet>();
        if(bulletScript != null)
        {
            if(!baseGun.isNPC)
            {
                bulletScript.damage = baseGun.damage;
            }
            bulletScript.pManager = baseGun.pManager;
        }
        }

        if (baseGun != null)
        {
             baseGun.ApplyCameraRecoil();
        }
    }
}