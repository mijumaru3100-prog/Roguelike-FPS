using UnityEngine;
using System.Collections;

public class ReloadInfinityAmmoAbility : WeaponAbility
{
    [SerializeField] private float infinitTime = 1f;

    private bool running = false;
    private Coroutine runningCoroutine;

    public override void OnCompleteReload()
    {
       runningCoroutine = StartCoroutine(RunningSwitch());
    }

    public override void SpecialShot(GunBase baseGun)
    {
        baseGun.shotAction.shot(baseGun);

        if (!running)
            return;

        baseGun.AddAmmoAnimated(1, 0.1f);
    }

    private IEnumerator RunningSwitch()
    {
        running = true;

        yield return new WaitForSeconds(infinitTime);

        running = false;
        runningCoroutine = null;
    }
}