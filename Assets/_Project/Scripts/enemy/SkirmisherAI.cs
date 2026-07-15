using UnityEngine;
using UnityEngine.AI;

public class SkirmisherAI : EnemyAI
{
    [Header("スカーミッシャー固有設定")]
    [Tooltip("この距離より近づかれたら下がり始めます")]
    public float fleeTriggerDistance = 8f;
    [Tooltip("下がり始めた後、この距離まで離れたら停止します")]
    public float fleeStopDistance = 12f;
    
    [Tooltip("下がるときの移動速度（通常時より遅くしたい場合は小さく設定）")]
    public float fleeSpeed = 1.5f;

    private bool _isFleeingState = false;
    private float _originalStoppingDistance = 0f;
    private float _originalSpeed = 3.5f; // 通常時の速度を保存する変数

    protected override void Start()
    {
        base.Start();
        if (agent != null)
        {
            agent.updateRotation = false;
            _originalStoppingDistance = agent.stoppingDistance;
            _originalSpeed = agent.speed; // 初期設定の速度を保存
        }
    }

    protected override void UpdateMovement(float dist, bool canSee)
    {
        if (canSee)
        {
            // 1. 下がるべきかどうかの状態（ステート）を管理
            if (!_isFleeingState && dist < fleeTriggerDistance)
            {
                // トリガー距離より近づかれたら「退避状態」開始
                _isFleeingState = true;
                if (agent != null)
                {
                    agent.stoppingDistance = 0f;
                    agent.speed = fleeSpeed; // 下がるときの速度に変更
                }
            }
            else if (_isFleeingState && dist > fleeStopDistance)
            {
                // 十分に離れたら「退避状態」終了
                _isFleeingState = false;
                if (agent != null)
                {
                    agent.stoppingDistance = _originalStoppingDistance;
                    agent.speed = _originalSpeed; // 通常時の速度に戻す
                    agent.ResetPath(); // 停止
                }
            }

            // 2. 状態に応じた移動処理
            if (_isFleeingState)
            {
                Flee();
            }
            else
            {
                if (agent.hasPath)
                {
                    agent.ResetPath();
                }
            }
        }
        else
        {
            // プレイヤーが見えない場合は追跡する
            if (_isFleeingState)
            {
                _isFleeingState = false;
                if (agent != null)
                {
                    agent.stoppingDistance = _originalStoppingDistance;
                    agent.speed = _originalSpeed; // 通常時の速度に戻す
                }
            }
            if (agent != null) agent.destination = target.position;
        }
    }

    protected override float GetAnimationSpeed(float currentDist)
    {
        if (_isFleeingState && agent.hasPath && agent.velocity.sqrMagnitude > 0.1f)
        {
            return -0.5f; 
        }
        else if (!_isFleeingState && agent.hasPath && agent.velocity.sqrMagnitude > 0.1f)
        {
            return 0.5f;
        }
        
        return 0f;
    }

    protected override bool IsFleeing()
    {
        return _isFleeingState;
    }
}