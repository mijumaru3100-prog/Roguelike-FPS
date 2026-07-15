using UnityEngine;
using System.Collections;
[CreateAssetMenu(menuName = "Passives/HighDamageStyle/TaletMode")]
public class TaletMode : PassiveEffect
{
    [SerializeField] private float buffCoolTime = 1f;
    [SerializeField] private PlayerStats buffStats;
    public int maxCount;
    [SerializeField] private bool isAdditive;
    private Coroutine activeCoroutine;
    public override void OnStopping(PlayerManager manager)
    {
        if (manager == null || buffStats == null ||running) return;
        running = true;
        activeCoroutine = manager.StartCoroutine(WaitTime(manager.sharedStats));
    }
    private IEnumerator WaitTime(PlayerStats stats)
    {
        for(int i = 0; i < maxCount+1; i++)
        {
            yield return new WaitForSeconds(buffCoolTime);
            stats.ApplyModifier(buffStats, true);
            remainCount++;
        }
        running = false;
    }
    public override void OnMoving(PlayerManager manager)
    {
        if (manager == null || buffStats == null) return;
        if (activeCoroutine != null)
        {
            manager.StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
        for(int i = 0; i < remainCount; i++)
        {
            manager.sharedStats.ApplyModifier(buffStats, false);
        }
        remainCount = 0;
        running = false;
    }
}