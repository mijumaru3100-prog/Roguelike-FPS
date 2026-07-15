using UnityEngine;
using System.Collections;
[CreateAssetMenu(menuName = "Passives/NearStyle/damageToBuff")]
public class damageToBuff : PassiveEffect
{
    [SerializeField] private PlayerStats buffStats;
    [SerializeField] private float buffTime;
    public override void OnTakeDamage(PlayerManager manager, float damage,EnemyHP enemyHP)
    {
        running = true;
        manager.sharedStats.ApplyModifier(buffStats, true); 
        manager.StartCoroutine(RemoveBuffAfterDelay(manager, buffTime));
    }
    private IEnumerator RemoveBuffAfterDelay(PlayerManager manager, float delay)
    {
        yield return new WaitForSeconds(delay);
        manager.sharedStats.ApplyModifier(buffStats, false); 
        running = false;
    }

    public override void OnGetThisPassive(PlayerManager manager)
    {
        running = false;   
    }
}
