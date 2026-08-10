using UnityEngine;
using System.Collections;
[CreateAssetMenu(menuName = "Passives/AmmoStyle/ ReloadInfinityAmmo")]

public class ReloadInfinityAmmo : PassiveEffect
{
    public float InfinitTime = 1f;
    private float LastReloadTime;
    public override void OnReloadComplete(PlayerManager manager)
    {
        LastReloadTime = Time.time;
        manager.StartCoroutine(runningSwitch());
    }
    public override void OnShotComplete(PlayerManager manager)
    {
        if(running)
        {
            manager.currentWeapon.AddAmmoAnimated(1, 0.1f);
        }
    }

    private IEnumerator runningSwitch()
    {
        running = true;
        yield return new WaitForSeconds(InfinitTime);
        running = false;
    }

    public override void OnGetThisPassive(PlayerManager manager)
    {
        running = false;
    }
}
