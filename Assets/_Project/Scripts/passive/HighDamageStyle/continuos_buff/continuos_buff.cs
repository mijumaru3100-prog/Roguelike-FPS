using UnityEngine;
[CreateAssetMenu(menuName = "Passives/HighDamageStyle/ContinuosBuff")]
public class continuos_buff : PassiveEffect
{
    [SerializeField] private PlayerStats buffStats;
    [SerializeField] private float damagebuffMultiple;
    [SerializeField] private float fireratebuffMultiple;
    [SerializeField] private int maxbuffCount = 5;
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        return damagebuffMultiple * remainCount;
    }
    public override float GetFireRateMultiplier(PlayerManager manager)
    {
        return fireratebuffMultiple * remainCount;
    }
    public override void OnHitBullet(PlayerManager manager ,float damage, EnemyHP hp)
    {
        if (remainCount < maxbuffCount)
        {
            remainCount++;
        }
    }
    public override void OnMiss(PlayerManager manager)
    {
        remainCount = 0;
    }
    public override void OnGetThisPassive(PlayerManager manager)
    {
        remainCount = 0;
    }
}
