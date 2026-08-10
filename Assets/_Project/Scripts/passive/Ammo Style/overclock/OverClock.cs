using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewOverClock", menuName = "Passives/AmmoStyle/OverClock")]
public class OverClock : PassiveEffect
{
    [SerializeField] private float buffDuration = 1.5f;
    public PlayerStats buffStats;
    
    private Coroutine currentBuffCoroutine;
    
    public override void OnReloadComplete(PlayerManager manager)
    {
        if (manager != null && buffStats != null)
        {
            PlayerStats activeStats = manager.sharedStats;
            if (currentBuffCoroutine != null)
            {
                manager.StopCoroutine(currentBuffCoroutine);
                activeStats.ApplyModifier(buffStats, false); 
            }

            currentBuffCoroutine = manager.StartCoroutine(ApplyBuff(activeStats));
        }
    }

    private IEnumerator ApplyBuff(PlayerStats stats)
    {
        stats.ApplyModifier(buffStats, true);
        running = true;
        
        yield return new WaitForSeconds(buffDuration);
        
        stats.ApplyModifier(buffStats, false);
        currentBuffCoroutine = null;
        running = false;
    }

    public override void OnGetThisPassive(PlayerManager manager)
    {
        currentBuffCoroutine = null;
    }
}