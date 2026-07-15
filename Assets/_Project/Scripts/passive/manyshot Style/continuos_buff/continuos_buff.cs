using UnityEngine;
[CreateAssetMenu(menuName = "Passives/ManyShotStyle/ContinuosBuff")]
public class continuos_buff : PassiveEffect
{
    [SerializeField] private PlayerStats buffStats;
    [SerializeField] private float damagebuffMultiple;
    [SerializeField] private float fireratebuffMultiple;
    [SerializeField] private int buffCount;
    [SerializeField] private int maxbuffCount = 5;
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        return damagebuffMultiple * buffCount;
    }
    public override float GetFireRateMultiplier(PlayerManager manager)
    {
        return fireratebuffMultiple * buffCount;
    }
    public override void OnHitBullet(PlayerManager manager ,float damage, EnemyHP hp)
    {
        if (buffCount < maxbuffCount)
        {
            buffCount ++;
        }
    }
    public override void OnMiss(PlayerManager manager)
    {
            buffCount = 0;
    }
}
