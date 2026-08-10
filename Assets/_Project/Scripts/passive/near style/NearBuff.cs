using UnityEngine;

[CreateAssetMenu(menuName = "Passives/NearStyle/NearSpeedUP")]
public class NearBuff: PassiveEffect
{
    //manager.playerHP.currentHP
    //manager.playerHP.maxHP
    //manager.playerHP.Heal(HealAmount);
    //enemyHP.maxHP
    //enemyHP.CurrentHP
    //manager.currentWeapon
    //manager.money
    //manager.AddMoney(amount)
    //manager.BuffFlug.
    //manager.RemovePassive(this);

    public PlayerStats buffStats;
    //manager.sharedStats.ApplyModifier(buffStats, true)
    //[SerializeField] private float buffDuration = 1.5f;

    //親で宣言済み//  
    //public int remainCount;
    //public bool running = true;   

    //using System.Collections;
    //manager.StartCoroutine(wait());
    //{
        //yield return new WaitForSeconds(buffDuration);
    //}

    public override void OnEnemyNear(PlayerManager manager)
    {
        manager.sharedStats.ApplyModifier(buffStats, true);
    }
    
    public override void OnEnemyAway(PlayerManager manager)
    {
        manager.sharedStats.ApplyModifier(buffStats, false);
    }
    
}
