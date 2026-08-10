using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/Common/MultiKiller")]
public class MultiKiller : PassiveEffect
{
    [Header("Settings")]
    [SerializeField] private int requiredKills = 3;
    [SerializeField] private float timeWindow = 2.0f;
    [SerializeField] private float buffDuration = 5.0f;
    [SerializeField] private int healAmount = 10;
    
    public PlayerStats buffStats; 

    private float lastKillTime = 0f;
    private Coroutine resetCoroutine;

    public override void OnKillEnemy(PlayerManager manager)
    {
        if (running) return;

        if (resetCoroutine != null)
        {
            manager.StopCoroutine(resetCoroutine);
        }

        remainCount++;
        lastKillTime = Time.time;

        if (remainCount >= requiredKills)
        {
            manager.StartCoroutine(ActivateBuff(manager));
        }
        else
        {
            resetCoroutine = manager.StartCoroutine(TimeOutMonitor(manager));
        }
    }

    private IEnumerator TimeOutMonitor(PlayerManager manager)
    {
        yield return new WaitForSeconds(timeWindow);
        
        remainCount = 0;
        resetCoroutine = null;
    }

    private IEnumerator ActivateBuff(PlayerManager manager)
    {
        running = true;
        
        manager.sharedStats.ApplyModifier(buffStats, true);
        manager.playerHP.Heal(healAmount);

        yield return new WaitForSeconds(buffDuration);

        manager.sharedStats.ApplyModifier(buffStats, false);
        
        remainCount = 0;
        running = false;
    }

    public override void OnGetThisPassive(PlayerManager manager)
    {
        running = false;
        remainCount = 0;
    }
}