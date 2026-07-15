using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/OneKill_DamageBuff")]
public class OneKill_DamageBuff : PassiveEffect
{
    [SerializeField] private float buffDuration = 1.5f;
    public PlayerStats buffStats;

    // 実行中のコルーチンと、バフを掛けたStatsを記憶する変数
    private Coroutine currentBuffCoroutine;
    private PlayerStats activeStats;

    public override void OnGetThisPassive(PlayerManager manager)
    {
        currentBuffCoroutine = null;
        activeStats = null;
    }  
    
    public override void OnKillEnemy(PlayerManager manager)
    { 
        // 連続キルで時間を更新したいため、「!running」の条件を外します
        if (manager != null && manager.BuffFlug.IsOneShotKill && buffStats != null)
        {
            // すでにバフが実行中（タイマーが動いている）なら、前のコルーチンを止めてバフを一瞬解除
            if (currentBuffCoroutine != null)
            {
                manager.StopCoroutine(currentBuffCoroutine);
                
                if (activeStats != null)
                {
                    activeStats.ApplyModifier(buffStats, false);
                }
            }

            // 新しくバフを掛けて、1.5秒の計測を開始
            activeStats = manager.sharedStats;
            currentBuffCoroutine = manager.StartCoroutine(ApplyBuff(activeStats));
        }
    }

    private IEnumerator ApplyBuff(PlayerStats stats)
    {
        stats.ApplyModifier(buffStats, true);
        
        yield return new WaitForSeconds(buffDuration);
        
        stats.ApplyModifier(buffStats, false);
        
        // 正常に終了したため参照をクリア
        currentBuffCoroutine = null; 
        activeStats = null;
    }
}