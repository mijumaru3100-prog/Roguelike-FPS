using UnityEngine;

[CreateAssetMenu(menuName = "Passives/AmmoStyle/EvenAmmoBuff")]
public class EvenAmmoBuff: PassiveEffect
{
    public float DamageMultipleBuff;
    public float WeakBonusBuff;
    public float ReloadSpeedBuff;
    

    private bool Isrunning = false;

    public override void OnGetThisPassive(PlayerManager manager)
    {
        Isrunning = false;
    }

    public override void OnReloadComplete(PlayerManager manager)
    {
        Isrunning = false; 
    }

    public override void OnShotComplete(PlayerManager manager)
    {
        if(manager.currentWeapon.currentAmmo % 2  == 0)
        {
            Isrunning = true;
        }
        else
        {
            Isrunning = false;
        }
    }

    public override void OnBeforeShot(PlayerManager manager)
    {
        if(manager.currentWeapon.currentAmmo % 2  == 0)
        {
            Isrunning = true;
        }
        else
        {
            Isrunning = false;
        }
    }
    // ステータス倍率 //(GunBase)   
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        if(Isrunning) return DamageMultipleBuff;
        else return 0;
    }
    
    public override float GetWeakPointBonus(PlayerManager manager)
    {
        if(Isrunning) return WeakBonusBuff;
        else return 0;
    }

    public override float GetReloadSpeedMultiplier(PlayerManager manager)
    {
        if(Isrunning) return ReloadSpeedBuff;
        else return 0;
    } 
}
