using UnityEngine;

[CreateAssetMenu(menuName = "Passives/ManyShotStyle/Hit_to_Rate")]
public class Hit_to_Rate: PassiveEffect
{
    //親で宣言済み//  
    //public int remainCount;
    //public bool running = true;   

    public float BuffRate = 0.01f;

    
    public override float GetFireRateMultiplier(PlayerManager manager)
    {
        return remainCount * BuffRate;   
    }  

    public override void OnGetThisPassive(PlayerManager manager) 
    {
        remainCount =0;
        running = false;
    }

    public override void OnHitBullet(PlayerManager manager, float damage, EnemyHP enemyHP)
    {
        remainCount++;
        running = true;
    }

    public override void OnBeforeReload(PlayerManager manager)
    {
        remainCount = 0;
        running = false;
    }
}
