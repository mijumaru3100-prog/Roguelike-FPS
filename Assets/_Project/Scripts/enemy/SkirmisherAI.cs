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
    private float _originalSpeed = 3.5f;

    protected override void Start()
    {
        base.Start();
        if (agent != null)
        {
            agent.updateRotation = false;
            _originalStoppingDistance = agent.stoppingDistance;
            _originalSpeed = agent.speed;
        }
    }

    protected override void UpdateMovement(float dist, bool canSee)
    {
        if (canSee)
        {
            if (!_isFleeingState && dist < fleeTriggerDistance)
            {
                _isFleeingState = true;
                if (agent != null)
                {
                    agent.stoppingDistance = 0f;
                    agent.speed = fleeSpeed;
                }
            }
            else if (_isFleeingState && dist > fleeStopDistance)
            {
                _isFleeingState = false;
                if (agent != null)
                {
                    agent.stoppingDistance = _originalStoppingDistance;
                    agent.speed = _originalSpeed;
                    agent.ResetPath();
                }
            }

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
            if (_isFleeingState)
            {
                _isFleeingState = false;
                if (agent != null)
                {
                    agent.stoppingDistance = _originalStoppingDistance;
                    agent.speed = _originalSpeed;
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