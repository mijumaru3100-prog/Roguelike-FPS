using UnityEngine;

[CreateAssetMenu(menuName = "Passives/AmmoStyle/EvenAmmoBuff")]
public class EvenAmmoBuff: PassiveEffect
{
    public float DamageMultipleBuff;
    public float WeakBonusBuff;
    public float ReloadSpeedBuff;

    public override void OnGetThisPassive(PlayerManager manager)
    {
        if(manager.currentWeapon.currentAmmo % 2  == 0)
        {
            running = true;
        }
        else
        {
            running = false;
        }
    }

    public override void OnReloadComplete(PlayerManager manager)
    {
        if(manager.currentWeapon.currentAmmo % 2  == 0)
        {
            running = true;
        }
        else
        {
            running = false;
        }
    }

    public override void OnShotComplete(PlayerManager manager)
    {
        if(manager.currentWeapon.currentAmmo % 2  == 0)
        {
            running = true;
        }
        else
        {
            running = false;
        }
    }

    public override void OnBeforeShot(PlayerManager manager)
    {
        if(manager.currentWeapon.currentAmmo % 2  == 0)
        {
            running = true;
        }
        else
        {
            running = false;
        }
    }
    // ステータス倍率 //(GunBase)   
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        if(running) return DamageMultipleBuff;
        else return 0;
    }
    
    public override float GetWeakPointBonus(PlayerManager manager)
    {
        if(running) return WeakBonusBuff;
        else return 0;
    }

    public override float GetReloadSpeedMultiplier(PlayerManager manager)
    {
        if(running) return ReloadSpeedBuff;
        else return 0;
    } 
}
