using UnityEngine;

[CreateAssetMenu(menuName = "Passives/Common/EmerGencyHeal")]
public class EmerGencyHeal: PassiveEffect
{
    //manager.playerHP.currentHP
    public int Amount = 1;

    public override void OnBattleStart(PlayerManager manager)
    {
        if(manager.playerHP.currentHP == 1);
        {
            manager.playerHP.Heal(Amount);
        }
    }
   
    // ボス //(未実装)
    // public override void OnBossStart(PlayerManager manager) { }
}
