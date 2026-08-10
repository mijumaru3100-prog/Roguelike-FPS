using UnityEngine;

[CreateAssetMenu(menuName = "Passives/Common/OverHealCharge")]
public class OverHealCharge: PassiveEffect
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

    //親で宣言済み//  
    //public int remainCount;
    //public bool running = true;   
    //manager.StartCoroutine(wait());
    //{
        //yield return new WaitForSeconds(buffDuration);
    //}

    public int maxCount = 1;
     
    public override void OnGetThisPassive(PlayerManager manager) 
    {
        running = false;
        remainCount =0;
    }
    public override void OnHeal(PlayerManager manager,int amount,PlayerHP HP)
    {
        if(HP.currentHP + amount > HP.maxHP)
        {
            remainCount = HP.currentHP + amount - HP.maxHP;
            if(remainCount > maxCount) remainCount = maxCount;
            running = true;
        }
    }

    public override void OnGetDamage(PlayerManager manager)
    {
        if(remainCount > 0)
        {
            remainCount --;
            manager.playerHP.Heal(1);
            if(remainCount <= 0)
            {
                running = false;
            }
        }
    }
}
