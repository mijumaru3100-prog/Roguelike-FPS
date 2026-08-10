using UnityEngine;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/OverCoin")]
public class OverCoin: PassiveEffect
{
        public int Amount = 10;
    private float overPercentage;   
    public override void OnTakeDamage(PlayerManager manager, float damage, EnemyHP enemyHP)
    {
        if(damage > enemyHP.CurrentHP)
        {
            overPercentage = (damage-enemyHP.CurrentHP)/enemyHP.maxHP;
            float randomValue = Random.Range(0f, 2f);
            if(overPercentage > randomValue)
            {
                manager.AddMoney(Amount);
            }
        }
    }

}
