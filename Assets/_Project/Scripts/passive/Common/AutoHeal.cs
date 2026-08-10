using UnityEngine;

[CreateAssetMenu(menuName = "Passives/Common/AutoHeal")]
public class AutoHeal: PassiveEffect
{
    public float HealRate = 0.4f;
    public int HealAmount = 3;

    //親で宣言済み//  
    //public int remainCount;
    //public bool running = true;   
    
    public override void OnGetDamage(PlayerManager manager)
    {
        if(manager.playerHP.currentHP < manager.playerHP.maxHP * HealRate)
        {
            manager.playerHP.Heal(HealAmount);
            manager.RemovePassive(this);
        }
    }
}
