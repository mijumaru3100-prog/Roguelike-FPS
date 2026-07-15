using UnityEngine;

public class ChargerAI : EnemyAI
{
    [Header("チャージャー設定")]
    [SerializeField] private bool isAggro = false; // プレイヤーを発見して追跡中か

    [SerializeField] private float strafeDistance = 1.5f;
[SerializeField] private float changeInterval = 0.5f;

private float timer;
private int strafeDir = 1;
private float currentOffset;

    protected override void Start()
    {
        base.Start();
        if (agent != null)
        {
            // インスペクターの keepDistance（初期値10、突進なら2〜3等に変更推奨）を停止距離に設定
            agent.stoppingDistance = keepDistance;
        }
    }

    protected override void UpdateMovement(float dist, bool canSee)
    {
        // --- 1. 接近を開始する条件の判定 ---
        if (!isAggro)
        {
            // 「検知範囲内」かつ「視界が通っている（壁の裏じゃない）」なら発見！
            if (dist <= detectionRange && canSee)
            {
                isAggro = true;
            }
        }

        // --- 2. 発見した後の移動・停止制御 ---
        if (isAggro)
        {
            // プレイヤーとの距離が、設定した停止距離より離れている時だけ追いかける
            if (dist > agent.stoppingDistance)
{
    if (agent.isOnNavMesh)
    {
        agent.isStopped = false;

        timer += Time.deltaTime;
        if (timer >= changeInterval)
{
    timer = 0f;
    strafeDir *= -1;
    currentOffset = Random.Range(1.0f, 2.0f);
}

        Vector3 right = Vector3.Cross(Vector3.up, (target.position - transform.position).normalized);

        Vector3 destination = target.position + right * strafeDir * currentOffset;
        agent.SetDestination(destination);
    }
}
            else
            {
                // 停止距離内に入ったらその場でピタッと止まる
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }
            }
        }
        else
        {
            // まだ見つけていない時は動かない
            if (agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
    }

    protected override float GetAnimationSpeed(float currentDist)
    {
        // 動いている時だけアニメーションを再生させるため、適当な数値を返す
        return 0.5f;
    }

    // ダメージを受けたら強制的にプレイヤーの方向を向いて戦闘状態にする
    public override void OnDamage()
    {
        base.OnDamage();
        isAggro = true;
    }
}