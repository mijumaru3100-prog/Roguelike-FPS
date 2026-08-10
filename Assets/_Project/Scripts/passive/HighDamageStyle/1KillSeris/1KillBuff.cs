using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/1K_Buff")]
public  class OneKillBuff: PassiveEffect
{
    [SerializeField] private float buffDuration = 1.5f;
    public PlayerStats buffStats;

    //親で宣言済み//  
    //public int remainCounts;
    //public bool running = true;     

    private Coroutine currentBuffCoroutine;
    private PlayerStats activeStats;

    public override void OnGetThisPassive(PlayerManager manager)
    {
        currentBuffCoroutine = null;
        activeStats = null;
    }  

    // 敵関連 //(Pmanager)
    public override void OnKillEnemy(PlayerManager manager)
    {
        if(!running && manager.BuffFlug.IsOneShotKill)
        {
            running = true;
        
            activeStats = manager.sharedStats;
            currentBuffCoroutine = manager.StartCoroutine(ApplyBuff(activeStats));
        }
    }

    private IEnumerator ApplyBuff(PlayerStats stats)
    {
        stats.ApplyModifier(buffStats, true);
        
        yield return new WaitForSeconds(buffDuration);
        
        stats.ApplyModifier(buffStats, false);
        
        running = false;
        currentBuffCoroutine = null; 
    }
}
