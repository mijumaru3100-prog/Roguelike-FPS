using UnityEngine;

[CreateAssetMenu(menuName = "Passives/ManyShotStyle/xTime_Hit_to_Damage")]
public class xTime_Hit_to_Damage : PassiveEffect
{
    public int count = 10;
    private int HitCount = 0;

    public int ExtraDamage = 70;
    private float calculateDamageMultiple = 1.0f;


    public override void OnHitBullet(PlayerManager manager, float damage, EnemyHP enemyHP) 
    { 
        HitCount++;
        remainCount =0;
        if(HitCount >= count)
        {
            PlayerStats stats = manager.sharedStats;
            GunBase gun = manager.currentWeapon.GetComponent<GunBase>();
            
            float totalMult = stats.damageMultiple;
            if (manager != null)
            {
                foreach (var p in manager.activePassives) 
                {
                    totalMult += p.GetDamageMultiplier(manager);
                }
            }
            int scalableDamage = Mathf.RoundToInt((gun.baseDamage + stats.bonusDamage) * totalMult * calculateDamageMultiple);
            
            int finalDamage = Mathf.Max(scalableDamage, (int)ExtraDamage);
            enemyHP.TakeDamage(finalDamage, null);
            HitCount = 0;
            remainCount =0;
        } 
    } 

    public virtual void OnGetThisPassive(PlayerManager manager)
    {
        HitCount = 0;
        remainCount =0;
    }
}
