using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/LeaveShot")]
public class LeaveShot : PassiveEffect
{
    public float LeaveTime;
    public PlayerStats Stats;

    private Coroutine currentBuffCoroutine;
    public override void OnBeforeShot(PlayerManager manager)
    {
        if (currentBuffCoroutine != null)
        {
            manager.StopCoroutine(currentBuffCoroutine);
            currentBuffCoroutine = null;

            if (running)
            {
                manager.sharedStats.ApplyModifier(Stats, false);
                running = false;
            }
        }

        currentBuffCoroutine = manager.StartCoroutine(HandleLeaveTime(manager));
    }

    private IEnumerator HandleLeaveTime(PlayerManager manager)
    {
        // manager.sharedStats.ApplyModifier(Stats, true);
        // running = true;

        yield return new WaitForSeconds(LeaveTime);

        manager.sharedStats.ApplyModifier(Stats, true);
        running = true;

        currentBuffCoroutine = null;
    }

    public override void OnGetThisPassive(PlayerManager manager)
    {
        currentBuffCoroutine = null;
        running = false;
    }
}