using UnityEngine;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/1K_AmmoCharge")]
public  class OneK_AmmoBuff: PassiveEffect
{
    public PlayerStats buffStats;

    //親で宣言済み//  
    //public int remainCounts;
    //public bool running = true;     

    // パッシブ取得 //(Pmanager)
    public override void OnGetThisPassive(PlayerManager manager)
    {
        running = false;
    }

    // 敵関連 //(Pmanager)
    public override void OnKillEnemy(PlayerManager manager)
    {
        if(!running && manager.BuffFlug.IsOneShotKill)
        {
            running = true;
            manager.sharedStats.ApplyModifier(buffStats, true);
        }
    }
    // 射撃 //(GunBase)

    public override void OnShotComplete(PlayerManager manager)
    {
        if(running)
        {
            manager.sharedStats.ApplyModifier(buffStats, false);
            running = false;
        }
    }

}
