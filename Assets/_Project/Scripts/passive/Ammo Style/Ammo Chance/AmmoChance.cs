using UnityEngine;
[CreateAssetMenu(menuName = "Passives/AmmoStyle/AmmoChance")]
public class AmmoChancePassive : PassiveEffect
{
    [Range(0, 1)] public float chance = 0.3f;
    public override void OnShotComplete(PlayerManager manager)
    {
        if (Random.value < chance)
        {
            GunBase gun = manager.currentWeapon;
            gun.currentAmmo++; 
            gun.UpdateAmmoDisplay();
            Debug.Log("弾薬節約発動！");
        }
    }
}