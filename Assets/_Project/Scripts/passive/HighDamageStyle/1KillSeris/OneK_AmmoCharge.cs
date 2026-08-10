using UnityEngine;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/1K_AmmoCharge")]
public  class OneK_AmmoCharge: PassiveEffect
{
    public int increaseAmount = 3;


    //親で宣言済み//  
    //public int remainCounts;
    //public bool running = true;     

    // 敵関連 //(Pmanager)
    public override void OnKillEnemy(PlayerManager manager)
    {
        if(manager.BuffFlug.IsOneShotKill)
        {
             manager.currentWeapon.AddAmmoAnimated(increaseAmount, 0.1f);
        }
    }
}
