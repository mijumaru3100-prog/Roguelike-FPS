using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/NearStyle/CounterHeal")]
public class CounterHeal: PassiveEffect
{
    [SerializeField] private float healDuration = 0.5f;
    private float lastHitTime;
    public override void OnKillEnemy(PlayerManager manager)
    {
        running = false;
        if(Time.time >= lastHitTime+healDuration)
        {
            manager.playerHP.Heal(1);
        }
    }

    public override void OnGetThisPassive(PlayerManager manager) 
    {
        running = false;
    }

    public override void OnGetDamage(PlayerManager manager)
    {
        running = true;
        lastHitTime = Time.time;
        manager.StartCoroutine(wait());
    }

   IEnumerator wait()
    {
        yield return new WaitForSeconds(healDuration);
        running = false;
    }
}
