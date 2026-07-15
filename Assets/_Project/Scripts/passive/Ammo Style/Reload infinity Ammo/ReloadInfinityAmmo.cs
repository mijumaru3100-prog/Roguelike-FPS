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
        runningSwitch();
    }
    public override void OnShotComplete(PlayerManager manager)
    {
        if(Time.time < LastReloadTime+InfinitTime)
        {
            manager.currentWeapon.currentAmmo++;
            manager.currentWeapon.UpdateAmmoDisplay(); 
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
