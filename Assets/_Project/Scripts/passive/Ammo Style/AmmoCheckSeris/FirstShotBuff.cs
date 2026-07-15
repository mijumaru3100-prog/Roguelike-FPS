using UnityEngine;

[CreateAssetMenu(menuName = "Passives/AmmoStyle/FirstShotBuff")]
public class FirstShotBuff: PassiveEffect
{
    public float DamageMultipleBuff;
    public float WeakBonusBuff;

    public override void OnGetThisPassive(PlayerManager manager)
    {
        running = false;
    }

    public override void OnReloadComplete(PlayerManager manager)
    {
        running = true; 
    }

    public override void OnShotComplete(PlayerManager manager)
    {
        running = false;
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
}
