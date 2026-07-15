using UnityEngine;

[CreateAssetMenu(menuName = "Passives/OOOOStyle/NoMiss_HeadShot")]
public class NoMiss_HeadShot: PassiveEffect
{
    public int MaxBuffCount;
    [SerializeField] private PlayerStats buffStats;
    private PlayerStats Stats;

    public override void OnHitBullet(PlayerManager manager, float damage, EnemyHP enemyHP)
    {
        if(MaxBuffCount > remainCount && manager.BuffFlug.IsHeadShot)
        {
            remainCount +=1;
            Stats = manager.sharedStats; 
            Stats.ApplyModifier(buffStats, true);
        }
        else
        {
            for(int i=0; i<remainCount; i++)
            {
                Stats = manager.sharedStats; 
                Stats.ApplyModifier(buffStats, false);
            }
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
