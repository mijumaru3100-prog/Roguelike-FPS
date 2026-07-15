using UnityEngine;
using DG.Tweening; // DOColorなどを使わない場合も、一応入れておくと安心だ、ね

[CreateAssetMenu(menuName = "Gun/Action/Prefabshot")]
public class PrefabshotAction : shotAction
{ 
    // gunbase_saigenのfireから呼び出される、よ
    public override void shot(GunBase baseGun)
    {
        GameObject b =baseGun.pManager.bulletPool.Get();
            b.transform.position = baseGun.muzzlePoint.position;
            b.transform.rotation = baseGun.muzzlePoint.rotation;
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
}