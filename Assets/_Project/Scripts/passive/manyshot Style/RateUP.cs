using UnityEngine;

[CreateAssetMenu(menuName = "Passives/Common/Buff")]
public class RateUP: PassiveEffect
{
    public PlayerStats buffStats;

    // パッシブ取得 //(Pmanager)
    public override void OnGetThisPassive(PlayerManager manager) 
    {
        manager.sharedStats.ApplyModifier(buffStats, true);
    }
}
