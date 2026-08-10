using UnityEngine;

public class ChargerAI : EnemyAI
{
    [Header("チャージャー設定")]
    [SerializeField] private bool isAggro = false;

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
            agent.stoppingDistance = keepDistance;
        }
    }

    protected override void UpdateMovement(float dist, bool canSee)
    {
        if (!isAggro)
        {
            if (dist <= detectionRange && canSee)
            {
                isAggro = true;
            }
        }

        if (isAggro)
        {
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
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }
            }
        }
        else
        {
            if (agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
    }

    protected override float GetAnimationSpeed(float currentDist)
    {
        return 0.5f;
    }

    public override void OnDamage()
    {
        base.OnDamage();
        isAggro = true;
    }
}