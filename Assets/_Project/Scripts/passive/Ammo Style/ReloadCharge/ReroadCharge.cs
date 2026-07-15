using UnityEngine;
[CreateAssetMenu(fileName = "ReloadCharge", menuName = "Passives/AmmoStyle/Reloadcharges")]
public class ReloadCharge : PassiveEffect
{
    [SerializeField]private int CurrentCount;       
    [SerializeField]private int MaxCount = 5;
    [SerializeField]private int IncreaseCount = 5;

    [SerializeField] private PlayerStats buffStats;
    private bool isBuff = false;
    public override void OnGetThisPassive(PlayerManager manager)
    {
        remainCount = 0;
        isBuff = false;
        running = false;
    }
    public override void OnReloadComplete(PlayerManager manager)
    {
       
        remainCount += IncreaseCount;
       remainCount =Mathf.Clamp(remainCount,0,MaxCount);
       
        if(!isBuff)
        {
            manager.sharedStats.ApplyModifier(buffStats, true); 
            isBuff = true;
            running = true;
        }
    }

    public override void OnShotComplete(PlayerManager manager) 
    {
        if(remainCount>0)
        {
            remainCount --;
            
            if(remainCount==0)
            {
                manager.sharedStats.ApplyModifier(buffStats, false); 
                isBuff = false;
                running = false;
            }
        }
    }


 

}
