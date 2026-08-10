using UnityEngine;
[CreateAssetMenu(menuName = "Passives/AmmoStyle/AmmoChance")]
public class AmmoChancePassive : PassiveEffect
{
    [Range(0, 1)] public float chance = 0.3f;
    public override void OnHitBullet(PlayerManager manager,float damage, EnemyHP enemyHP)
    {
        if (Random.value < chance)
        {
            manager.currentWeapon.AddAmmoAnimated(1, 0.1f);
            Debug.Log("弾薬節約発動！");
        }
    }
}