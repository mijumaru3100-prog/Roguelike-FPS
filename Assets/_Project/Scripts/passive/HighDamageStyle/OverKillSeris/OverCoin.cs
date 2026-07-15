using UnityEngine;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/OverCoin")]
public class OverCoin: PassiveEffect
{
    private float overPercentage;   
    public override void OnTakeDamage(PlayerManager manager, float damage, EnemyHP enemyHP)
    {
        if(damage > enemyHP.CurrentHP)
        {
            overPercentage = (damage-enemyHP.CurrentHP)/enemyHP.maxHP;
            float randomValue = Random.Range(0f, 100f);
            if(overPercentage/2 > randomValue)
            {
                manager.money +=100;
            }
        }
    }

}
