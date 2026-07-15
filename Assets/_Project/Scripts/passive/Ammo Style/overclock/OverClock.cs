using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewOverClock", menuName = "Passives/AmmoStyle/OverClock")]
public class OverClock : PassiveEffect
{
    [SerializeField] private float buffDuration = 1.5f;
    public PlayerStats buffStats;
    
    // 実行中のコルーチンを記憶しておく変数
    private Coroutine currentBuffCoroutine;
    
    
    public override void OnReloadComplete(PlayerManager manager)
    {
        if (manager != null && buffStats != null)
        {
            PlayerStats activeStats = manager.sharedStats;
            // すでに実行中なら、前回のコルーチンを停止して時間をリセットする
            if (currentBuffCoroutine != null)
            {
                manager.StopCoroutine(currentBuffCoroutine);
                // 注意: ここで一度バフを解除しないと、Statsが重複したままになります
                activeStats.ApplyModifier(buffStats, false); 
            }

            
            currentBuffCoroutine = manager.StartCoroutine(ApplyBuff(activeStats));
        }
    }

    private IEnumerator ApplyBuff(PlayerStats stats)
    {
        stats.ApplyModifier(buffStats, true);
        running = true;
        
        yield return new WaitForSeconds(buffDuration);
        
        stats.ApplyModifier(buffStats, false);
        currentBuffCoroutine = null; // 終了したら初期化
        running = false;
    }

    public override void OnGetThisPassive(PlayerManager manager)
    {
        currentBuffCoroutine = null;
    }
}