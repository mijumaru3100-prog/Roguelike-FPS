using UnityEngine;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/Nodamage_DamageBuff")]
public class BuffDamage: PassiveEffect
{
    public float DamageMultipleBuff;
    public float WeakBonusBuff;
    // ステータス倍率 //(GunBase)   
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        if(manager.BuffFlug.MaxHP) return DamageMultipleBuff;
        else return 0;
    }
    
    public override float GetWeakPointBonus(PlayerManager manager)
    {
        if(manager.BuffFlug.MaxHP) return WeakBonusBuff;
        else return 0;
    }
}