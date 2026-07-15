using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using DG.Tweening;
public abstract class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Normal, Damaged, Dead }
    [Header("現在の状態")]
    public EnemyState currentState = EnemyState.Normal;
    [Header("共通設定")]
    public Transform target;
    protected NavMeshAgent agent;
    protected Animator anim;
    public float detectionRange = 20f;
    public PlayerManager pManager;
    public EnemyHP HP;
    public GameObject body;
    public Collider collider;

    public int CoinAmount = 10;
    [Header("射撃・スナイパー設定")]
    public GunBase gun;
    public float keepDistance = 10f;
    public LayerMask obstacleLayer;
    [Header("射撃設定")]
    public float fireRate = 0.5f;
    [Header("エイム設定")]
    public bool showAimDebug = false;
    public float aimHeightOffset = 0f;
    public float myEyeHeight = 1.5f;
    public float shootingAngleThreshold = 20f;
    [Header("回転設定")]
    [Tooltip("旋回スピード（値が小さいほどゆっくり回転します）")]
    public float rotationSpeed = 4f;
    protected float nextFireTime;
    protected MeshRenderer _renderer;
    protected MaterialPropertyBlock _propBlock;
    protected static readonly int _colorID = Shader.PropertyToID("_Color");
    protected Color _originalColor;
    protected static readonly int SpeedHash = Animator.StringToHash("Speed");
    protected static readonly int AttackHash = Animator.StringToHash("Attack");
    protected static readonly int DamageHash = Animator.StringToHash("Damage");
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        anim = GetComponentInChildren<Animator>(); 
        if (gun == null) gun = GetComponentInChildren<GunBase>();
    }
    protected virtual void Update()
    {
        if (target == null || currentState != EnemyState.Normal) return;
        float dist = Vector3.Distance(transform.position, target.position);
        Vector3 aimTargetPos = target.position + Vector3.up * aimHeightOffset;
        bool canSee = CanSeePlayer(aimTargetPos); 
        HandleRotation();
        UpdateMovement(dist, canSee);

if (anim != null)
{
    float speedForAnim = 0f;
    
    // 目的地があり、かつ実際にAgentが物理的に動いている場合のみ数値を設定
    if (agent.hasPath && agent.remainingDistance > 0.1f && agent.velocity.sqrMagnitude > 0.1f)
    {
        speedForAnim = GetAnimationSpeed(dist);
    }
    // 動いていない（ResetPathされたなど）なら、強制的にアニメーション速度を0にする
    else
    {
        speedForAnim = 0f;
    }

    float currentParam = anim.GetFloat(SpeedHash);
    // 戻る速度（5f）が遅いと滑るので、10f〜15fくらいにしてピタッと止まるようにする
    float smoothedSpeed = Mathf.Lerp(currentParam, speedForAnim, Time.deltaTime * 12f); 
    anim.SetFloat(SpeedHash, smoothedSpeed);
}
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);
        if (canSee && dist <= detectionRange && angle < shootingAngleThreshold)
        {
            if (Time.time >= nextFireTime)
            {
                if (gun != null) gun.tryFire(); 
                if (anim != null) anim.SetTrigger(AttackHash);
                nextFireTime = Time.time + fireRate;
            }
        }
    }
    protected abstract void UpdateMovement(float dist, bool canSee);
    protected abstract float GetAnimationSpeed(float currentDist);
    public virtual void OnDamage()
    {
        if (currentState == EnemyState.Dead) return;
        currentState = EnemyState.Damaged;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        if (anim != null) anim.SetTrigger(DamageHash);
        if (body != null) body.transform.DOShakePosition(0.5f, new Vector3(0.1f, 0.1f, 0.1f), 10, 90f, true);
        StopCoroutine("RecoverFromDamage"); 
        StartCoroutine(RecoverFromDamage(0.5f));
    }
    protected virtual IEnumerator RecoverFromDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentState != EnemyState.Dead)
        {
            currentState = EnemyState.Normal;
            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        }
    }
    protected bool CanSeePlayer(Vector3 targetUpperPos)
    {
        Vector3 eyePos = transform.position + Vector3.up * myEyeHeight;
        Vector3 direction = (targetUpperPos - eyePos).normalized;
        RaycastHit hit;
        int mask = ~LayerMask.GetMask("Ignore Raycast", "invisibleWall", "Enemy"); 
        Debug.DrawRay(eyePos, direction * detectionRange, Color.red);
        if (Physics.Raycast(eyePos, direction, out hit, detectionRange, mask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == target;
        }
        return false;
    }
    protected virtual void HandleRotation()
    {
        if (target == null) return;
        Vector3 aimTarget = target.position + Vector3.up * aimHeightOffset;
        if (gun != null) gun.transform.LookAt(aimTarget);
        bool isFleeing = IsFleeing();
        if (isFleeing || agent.velocity.sqrMagnitude < 0.2f)
        {
            Vector3 targetPos = target.position;
            targetPos.y = transform.position.y;
            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            Vector3 moveDir = agent.velocity.normalized;
            moveDir.y = 0;
            if (moveDir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
            }
        }
    }
    protected virtual bool IsFleeing()
    {
        return false;
    }
    protected void Flee() 
    {
        Vector3 fleeDir = (transform.position - target.position).normalized;
        Vector3 targetPos = transform.position + fleeDir * 5f;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas)) agent.destination = hit.position; 
    }
    protected virtual void LateUpdate()
    {
        if (body != null)
        {
            Vector3 pos = body.transform.localPosition;
            pos.x = 0; pos.z = 0;
            body.transform.localPosition = pos;
        }
    }
   public virtual void OnDie()
{
    if (currentState == EnemyState.Dead) return;

    currentState = EnemyState.Dead;

    // 1. 走っている可能性のあるダメージ復帰コルーチンを完全に止める
    StopCoroutine("RecoverFromDamage");
    
    // 2. DOTweenの揺れアニメーションが残っていたら強制終了する
    if (body != null)
    {
        body.transform.DOKill(); 
    }

    collider.enabled = false;
    if (HP != null && HP.weakPointCollider != null)
    {
        HP.weakPointCollider.enabled = false;
    }

    if (agent != null && agent.isOnNavMesh)
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    if (anim != null)
    {
        anim.SetFloat(SpeedHash, 0);
        anim.ResetTrigger(AttackHash);
        anim.ResetTrigger(DamageHash);
        // Deadトリガーを最優先で実行
        anim.SetTrigger(DeadHash);
    }

    Debug.Log(gameObject.name + " has died.");

    // コインの生成処理
    if (pManager != null && pManager.coinPool != null)
    {
        GameObject c = pManager.coinPool.Get();
        c.transform.position = transform.position;
        c.transform.rotation = transform.rotation;
        Coin CoinScript = c.GetComponent<Coin>();
        if (CoinScript != null)
        {
            CoinScript.moneyAmount = CoinAmount;
            CoinScript.manager = pManager;
        }
    }

    // 3. 破棄までの時間を少し長め（例: 2秒〜3秒など、モーションの長さに合わせる）にする
    // もしくはアニメーション終了時にイベントでDestroyする形が理想です
    Destroy(gameObject, 2.5f); 
}

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (!showAimDebug) return;

        // 目の位置
        Vector3 eyePos = transform.position + Vector3.up * myEyeHeight;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePos, 0.15f);

        // 検知範囲 (足元)
        UnityEditor.Handles.color = new Color(0f, 0.8f, 1f, 0.15f);
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, detectionRange);

        if (target == null)
        {
            // ターゲットがいない場合、正面方向に視野コーンを描画
            DrawAimCone(eyePos, transform.forward, shootingAngleThreshold, Color.yellow);
            return;
        }

        // ターゲットの狙い位置
        Vector3 targetAimPos = target.position + Vector3.up * aimHeightOffset;
        Vector3 dirToTarget = (targetAimPos - eyePos).normalized;

        // 視線チェック (Raycast再現)
        int mask = ~LayerMask.GetMask("Ignore Raycast", "invisibleWall", "Enemy");
        bool canSee = false;
        Vector3 hitPoint = targetAimPos;

        if (Physics.Raycast(eyePos, dirToTarget, out RaycastHit hit, detectionRange, mask, QueryTriggerInteraction.Ignore))
        {
            canSee = (hit.transform == target);
            hitPoint = hit.point;
        }

        // 視線の描画
        if (canSee)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(eyePos, targetAimPos);
            Gizmos.DrawWireSphere(targetAimPos, 0.2f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePos, hitPoint);
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(hitPoint, targetAimPos);
            Gizmos.DrawWireSphere(targetAimPos, 0.2f);
        }

        // 射撃角度判定の描画
        float angle = Vector3.Angle(transform.forward, (target.position - transform.position).normalized);
        Color coneColor = (canSee && angle < shootingAngleThreshold) ? new Color(0f, 1f, 0f, 0.12f) : new Color(1f, 0.9f, 0f, 0.08f);
        DrawAimCone(eyePos, transform.forward, shootingAngleThreshold, coneColor);
    }

    private void DrawAimCone(Vector3 origin, Vector3 forward, float angleThreshold, Color color)
    {
        UnityEditor.Handles.color = color;
        Vector3 leftLimit = Quaternion.AngleAxis(-angleThreshold, Vector3.up) * forward;
        Vector3 rightLimit = Quaternion.AngleAxis(angleThreshold, Vector3.up) * forward;

        // 扇形の描画 (Handles.DrawSolidArcはUNITY_EDITOR内のみ使用可能)
        UnityEditor.Handles.DrawSolidArc(origin, Vector3.up, leftLimit, angleThreshold * 2f, 3f);
        
        // 境界線
        UnityEditor.Handles.color = new Color(color.r, color.g, color.b, 0.7f);
        Gizmos.color = new Color(color.r, color.g, color.b, 0.7f);
        Gizmos.DrawLine(origin, origin + leftLimit * 3f);
        Gizmos.DrawLine(origin, origin + rightLimit * 3f);
    }
#endif
}