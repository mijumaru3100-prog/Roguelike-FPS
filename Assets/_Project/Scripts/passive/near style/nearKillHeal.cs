using UnityEngine;
[CreateAssetMenu(menuName = "Passives/NearStyle/nearKillHeal")]
public class nearKillHeal : PassiveEffect
{
    public float checkDistance = 5f;
    public override void OnKillEnemy(PlayerManager manager)
    {
        if (manager.minEnemyDistance <= checkDistance)
        {
            manager.playerHP.Heal(1);
        }
    }
}
