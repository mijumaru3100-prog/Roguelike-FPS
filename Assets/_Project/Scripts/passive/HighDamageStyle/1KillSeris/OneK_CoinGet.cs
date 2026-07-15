using UnityEngine;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/OneK_CoinGet")]
public  class OneK_CoinGet: PassiveEffect
{
    public int increaseAmount;


    //親で宣言済み//  
    //public int remainCounts;
    //public bool running = true;     

    // 敵関連 //(Pmanager)
    public override void OnKillEnemy(PlayerManager manager)
    {
        if(!manager.BuffFlug.IsOneShotKill)
        {
            
            manager.AddMoney(increaseAmount);
        }
    }
}
