using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/ADS_CanTMove")]
public class ADS_CanTMove: PassiveEffect
{
    [SerializeField] private float buffCoolTime = 1f;
    [SerializeField] private PlayerStats buffStats;
    public int maxCount;
    [SerializeField] private bool isAdditive;
    private Coroutine activeCoroutine;
    public override void OnADSStart(PlayerManager manager)
    {
        if (manager == null || buffStats == null ||running) return;
        running = true;
        manager.playerMove.canNotWark = true;
        manager.playerMove.canNotJump = false;
        activeCoroutine = manager.StartCoroutine(WaitTime(manager.sharedStats));
    }
    private IEnumerator WaitTime(PlayerStats stats)
    {
        for(int i = 0; i < maxCount; i++)
        {
            yield return new WaitForSeconds(buffCoolTime);
            stats.ApplyModifier(buffStats, true);
            remainCount++;
        }
    }
    public override void OnADSEnd(PlayerManager manager)
    {
        if (manager == null || buffStats == null) return;
        running = false;
        manager.playerMove.canNotWark = false;
        manager.playerMove.canNotJump = false;
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
        
    }
}
