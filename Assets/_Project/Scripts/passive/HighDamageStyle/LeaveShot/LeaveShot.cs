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
    }

    

    currentBuffCoroutine = manager.StartCoroutine(HandleLeaveTime(manager));
}

public override void OnShotComplete(PlayerManager manager)
{
    if (currentBuffCoroutine != null)
    {
        manager.StopCoroutine(currentBuffCoroutine);
        currentBuffCoroutine = null;
    }

    SetBuff(manager, false);
}

private IEnumerator HandleLeaveTime(PlayerManager manager)
{
    yield return new WaitForSeconds(LeaveTime);

    currentBuffCoroutine = null;
    SetBuff(manager, true);
}

public override void OnGetThisPassive(PlayerManager manager)
{
    if (currentBuffCoroutine != null)
    {
        manager.StopCoroutine(currentBuffCoroutine);
        currentBuffCoroutine = null;
    }

    manager.sharedStats.ApplyModifier(Stats, true);
    running = true;
}

private void SetBuff(PlayerManager manager, bool active)
{
    if (running == active) return;

    manager.sharedStats.ApplyModifier(Stats, active);
    running = active;
}
}