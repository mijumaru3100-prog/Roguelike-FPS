using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/ManyShotStyle/MissAmmo")]
public class MissAmmo: PassiveEffect
{
    [Range(0, 1)] public float chance = 0.6f;
    public override void OnMiss(PlayerManager manager)
    {
        if (Random.value < chance)
        {
            manager.currentWeapon.AddAmmoAnimated(1, 0.1f);
        }
    }
}
