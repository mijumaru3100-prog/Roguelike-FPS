using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/HighDamageStyle/LeaveShot")]
public  class LeaveShot: PassiveEffect
{
    public float LeaveTime;
    public PlayerStats Stats;

    private float lastShotTime;
    private bool IsBuff = false;
    private Coroutine currentBuffCoroutine;
    private PlayerStats activeStats;


    public override void OnBeforeShot(PlayerManager manager)
   {
        // すでに実行中なら、前回のコルーチンを停止して時間をリセットする
        if (currentBuffCoroutine != null)
        {
            manager.StopCoroutine(currentBuffCoroutine);
            // 注意: ここで一度バフを解除しないと、Statsが重複したままになります
            activeStats.ApplyModifier(Stats, false); 
        }

        activeStats = manager.sharedStats;
        currentBuffCoroutine = manager.StartCoroutine(ApplyBuff(activeStats));
    }
    

    private IEnumerator ApplyBuff(PlayerStats stats)
    {
        if(running)
        {
        stats.ApplyModifier(Stats, false);
        }
        running = false;
        
        yield return new WaitForSeconds(LeaveTime);
        
        stats.ApplyModifier(Stats, true);
        currentBuffCoroutine = null; // 終了したら初期化
        running = true;
    }

    public override void OnGetThisPassive(PlayerManager manager)
    {
        currentBuffCoroutine = null;
    }

}
