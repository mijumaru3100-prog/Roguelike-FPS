using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/ManyShotStyle/MissCover")]
public class MissCover: PassiveEffect
{
    public PlayerStats buffStats;

    public override void OnGetThisPassive(PlayerManager manager) 
    {
        running = false;
    }

    public override void OnMiss(PlayerManager manager)
    {
        running = true;
        remainCount++;
        manager.sharedStats.ApplyModifier(buffStats, true);
    }

    public override void OnReloadComplete(PlayerManager manager)
    {
        for (int i = 0; i < remainCount; i++)
        {
            manager.sharedStats.ApplyModifier(buffStats, false);
        }
        remainCount = 0;
    }

    public override void OnChangeWeapon(PlayerManager manager)
    {
        for (int i = 0; i < remainCount; i++)
        {
            manager.sharedStats.ApplyModifier(buffStats, false);
        }
        remainCount = 0;
    } 
}
