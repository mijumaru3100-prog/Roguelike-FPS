using UnityEngine;

[CreateAssetMenu(menuName = "Passives/Common/Guillotine")]
public class Guillotine: PassiveEffect
{
    public float HPRate = 0.2f;
    public override void OnHitBullet(PlayerManager manager, float damage, EnemyHP enemyHP)
    {
        if(enemyHP.CurrentHP-damage < enemyHP.maxHP * HPRate)
        {
            enemyHP.TakeDamage(enemyHP.CurrentHP,null);
        }
    }
}
