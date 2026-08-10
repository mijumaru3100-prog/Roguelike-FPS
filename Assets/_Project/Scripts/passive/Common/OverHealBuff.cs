using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/Common/OverHealBuff")]
public class OverHealBuff: PassiveEffect
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
    [SerializeField] private float buffDuration = 1.5f;

    //親で宣言済み//  
    //public int remainCount;
    //public bool running = true;   
    //manager.StartCoroutine(wait());
    //{
        //yield return new WaitForSeconds(buffDuration);
    //}
     
    public override void OnGetThisPassive(PlayerManager manager) 
    {
        running = false;
    }
    public override void OnHeal(PlayerManager manager,int amount,PlayerHP HP)
    {
        if(HP.currentHP + amount > HP.maxHP)
        {
            manager.sharedStats.ApplyModifier(buffStats, true);
            manager.StartCoroutine(wait(manager));
        }
    }

    IEnumerator wait(PlayerManager manager)
    {
        yield return new WaitForSeconds(buffDuration);
        manager.sharedStats.ApplyModifier(buffStats, false);
    }
}
