using UnityEngine;
using UnityEngine.Serialization;
[CreateAssetMenu(menuName = "stats")]
public class PlayerStats : ScriptableObject
{
    public PlayerManager pManager;

    public int bonusMaxAmmo =0;

    public int bonusMaxHP = 0;

     public int bonusDamage = 0;
    public float WeakPointBonus  =0f;
    [Header("Multiple")]
    public float MoveSpeedMultiple = 0;
    public float JumpHeightMultiple = 0;
   public float reloadSpeedMultiple = 0;
    public float fireRateMultiple= 0;
    public float maxAmmoMultiple = 0;
    public float recoilForceMultiple= 0;

    public float maxHPMultiple = 0;

    public float damageMultiple = 0;
    [Header("prefabShot用")]
    public float bonusBulletSpeed = 0;
    public float bulletSpeedMultiple = 0;
    public float bonusLifeTime = 0;
    public float lifeTimeMultiple = 0;
    public int bonusThrouthCount = 0;
    public float throuthCountMultiple = 0;
    public void ResetToDefault()
    {
        bonusMaxAmmo = 0;
        maxAmmoMultiple = 0;
        fireRateMultiple = 0;
        recoilForceMultiple =0;
        reloadSpeedMultiple = 0;
        WeakPointBonus = 0;
        bonusMaxHP = 0;
        maxHPMultiple = 0;
        bonusDamage = 0;
        damageMultiple = 0;

        bonusBulletSpeed = 0;
        bulletSpeedMultiple = 0;
        bonusLifeTime = 0;
        lifeTimeMultiple = 0;
        bonusThrouthCount = 0;
        throuthCountMultiple = 0;

        JumpHeightMultiple  = 0;
        MoveSpeedMultiple = 0;
    }

    public void ApplyModifier(PlayerStats modifier, bool isApplying)
    {
        if (modifier == null) return;
        int sign = isApplying ? 1 : -1;
        bonusMaxAmmo += modifier.bonusMaxAmmo * sign;
        bonusMaxHP += modifier.bonusMaxHP * sign;
        bonusDamage += modifier.bonusDamage * sign;
        bonusBulletSpeed += modifier.bonusBulletSpeed * sign;
        bonusLifeTime += modifier.bonusLifeTime * sign;
        bonusThrouthCount += modifier.bonusThrouthCount * sign;
        WeakPointBonus += modifier.WeakPointBonus * sign;
        maxAmmoMultiple += modifier.maxAmmoMultiple * sign;
        fireRateMultiple += modifier.fireRateMultiple * sign;
        recoilForceMultiple += modifier.recoilForceMultiple * sign;
        reloadSpeedMultiple += modifier.reloadSpeedMultiple * sign;
        maxHPMultiple += modifier.maxHPMultiple * sign;
        damageMultiple += modifier.damageMultiple * sign;
        bulletSpeedMultiple += modifier.bulletSpeedMultiple * sign;
        lifeTimeMultiple += modifier.lifeTimeMultiple * sign;
        throuthCountMultiple += modifier.throuthCountMultiple * sign;

        JumpHeightMultiple +=  modifier.JumpHeightMultiple * sign;
        MoveSpeedMultiple += modifier.MoveSpeedMultiple * sign;

        if (pManager != null && modifier.bonusMaxAmmo != 0 || modifier.maxAmmoMultiple != 0)
    {
        if (pManager.currentWeapon != null) 
        {
            pManager.currentWeapon.RefreshMaxAmmoUI();
        }
    }
    }
}
