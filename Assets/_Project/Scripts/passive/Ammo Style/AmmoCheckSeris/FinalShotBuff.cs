using UnityEngine;

[CreateAssetMenu(menuName = "Passives/AmmoStyle/FinalShotBuff")]
public class FinalShotBuff: PassiveEffect
{
    public float DamageMultipleBuff;
    public float WeakBonusBuff;

    public override void OnGetThisPassive(PlayerManager manager)
    {
        running = false;
    }

    public override void OnReloadComplete(PlayerManager manager)
    {
        running = false; 
    }

    private void CheckAmmo(PlayerManager manager)
    {
        running = (manager.currentWeapon.currentAmmo == 1);
    }

    public override void OnBeforeShot(PlayerManager manager) => CheckAmmo(manager);
    public override void OnShotComplete(PlayerManager manager) => CheckAmmo(manager);

    public override float GetDamageMultiplier(PlayerManager manager)
    {
        return running ? DamageMultipleBuff : 0;
    }
    
    public override float GetWeakPointBonus(PlayerManager manager)
    {
        return running ? WeakBonusBuff : 0;
    }
}