using UnityEngine;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/DelicateBuff")]
public class DelicateBuff: PassiveEffect
{
    public PlayerStats buffStats;
    public int defaultCount = 1;

    //親で宣言済み//  
    //public int remainCount;
    //public bool running = true;   

    // パッシブ取得 //(Pmanager)
    public override void OnGetThisPassive(PlayerManager manager) 
    {
        manager.sharedStats.ApplyModifier(buffStats, true);
        remainCount = defaultCount;
    }
    public override void OnGetDamage(PlayerManager manager)
    {
        remainCount --;
        if(remainCount <= 0)
        {
            manager.sharedStats.ApplyModifier(buffStats, false);
            manager.RemovePassive(this);
        }
    }
}
