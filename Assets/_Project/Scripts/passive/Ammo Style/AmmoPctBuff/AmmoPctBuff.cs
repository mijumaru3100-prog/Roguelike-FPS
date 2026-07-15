using UnityEngine;
[CreateAssetMenu(menuName = "Passives/AmmoStyle/AmmoPctBuff")]
public class AmmoPctBuff : PassiveEffect
{
    [SerializeField] private float damagebuff;
    [SerializeField] private float fireratebuff;
    [SerializeField][Range(0, 1)] private float buffAmmoThreshold = 0.3f;
    [SerializeField] private bool triggerWhenHigher = false; 
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        if (IsConditionMet(manager))
        {
            return damagebuff;
        }
        return 0f;
    }
    public override float GetFireRateMultiplier(PlayerManager manager)
    {
        if (IsConditionMet(manager))
        {
            return fireratebuff;
        }
        return 0f;
    }
    private bool IsConditionMet(PlayerManager manager)
    {
        if (manager.currentWeapon == null) return false;
        float ammoPercent = (float)manager.currentWeapon.currentAmmo / manager.currentWeapon.maxAmmo;
        if (triggerWhenHigher)
        {
            return ammoPercent >= buffAmmoThreshold;
            running =  ammoPercent >= buffAmmoThreshold;
        }
        else
        {
            return ammoPercent <= buffAmmoThreshold;
            running =  ammoPercent <= buffAmmoThreshold;
        }
    }
    public override void OnGetThisPassive(PlayerManager manager)
    {
        running = IsConditionMet(manager);
    }

    public override void OnReloadComplete(PlayerManager manager)
    {
        running = IsConditionMet(manager);
    }
    public override void OnShotComplete(PlayerManager manager)
    {
        running = IsConditionMet(manager);
    }

}
