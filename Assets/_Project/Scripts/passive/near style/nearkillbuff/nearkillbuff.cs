using UnityEngine;
using System.Collections;
[CreateAssetMenu(menuName = "Passives/NearStyle/NearKillBuff")]
public class nearkillbuff : PassiveEffect
{
    [SerializeField] private PlayerStats buffStats;
    [SerializeField] private float range = 5f;
    [SerializeField] private float buffTime = 5f;
    public override void OnKillEnemy(PlayerManager manager)
    {
        if(manager.minEnemyDistance < range)
        {
            running = true;
            manager.sharedStats.ApplyModifier(buffStats, true);
            manager.StartCoroutine(RemoveBuff(manager));
        }   
    }
    private IEnumerator RemoveBuff(PlayerManager manager)
    {
        yield return new WaitForSeconds(buffTime);
        manager.sharedStats.ApplyModifier(buffStats, false);
        running = false;
    }

    public override void OnGetThisPassive(PlayerManager manager)
    {
        running = false;   
    }
}
