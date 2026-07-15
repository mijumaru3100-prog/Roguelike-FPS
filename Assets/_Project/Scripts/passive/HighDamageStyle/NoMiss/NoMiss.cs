using UnityEngine;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/NoMiss")]
public class NoMiss: PassiveEffect
{
    public int MaxBuffCount;
    [SerializeField] private PlayerStats buffStats;
    private PlayerStats Stats;

    public override void OnHitBullet(PlayerManager manager, float damage, EnemyHP enemyHP)
    {
        if(MaxBuffCount > remainCount)
        {
            remainCount +=1;
            Stats = manager.sharedStats; 
            Stats.ApplyModifier(buffStats, true);
        }
    }

    public override void OnMiss(PlayerManager manager)
    {
        for(int i=0; i<remainCount; i++)
        {
            Stats = manager.sharedStats; 
            Stats.ApplyModifier(buffStats, false);
        }
        remainCount = 0;
    }


     public override void OnGetThisPassive(PlayerManager manager) 
    {
        remainCount = 0;
    }
}
