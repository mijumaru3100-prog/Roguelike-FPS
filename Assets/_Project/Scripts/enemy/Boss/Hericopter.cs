using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Hericopter : EnemyAI
{
    public enum ActionState { Idle, MachineGun,Missile}
    
    [Header("現在のアクション状態")]
    public ActionState currentActionState = ActionState.Idle;
    
    [Header("共通設定 (EnemyAI由来)")]
    public Transform model;
    
    [Header("射撃（移動）設定")]
    public float AttackShiftTime;
    public float MachineGunAttackTime;
    
    public LayerMask obstacleLayer;
    public Transform AttckPoint_1A;
    public Transform AttckPoint_1B;
    public Transform AttckPoint_2A;
    public Transform AttckPoint_2B;
    public Transform AttckPoint_3A;
    public Transform AttckPoint_3B;
    [Header("射撃（攻撃）設定")]
    public float FireRate = 0.08f;
    public Transform L_muzzlePoint;
    public Transform R_muzzlePoint;
    private Transform NextMuzzle ;
    public float spreadAngle = 30;

    [Header("ミサイル")]
public GameObject missileWarningPrefab;
public GameObject missilePrefab;
public Transform missileSpawnPoint;

public int missileCount = 3;
public float missileRadius = 5f;
public float missileInterval = 0.3f;

    [Header("回転設定")]
    public float rotationSpeed = 4f;
    public float IdleTime;
    
    public bool isChangingState;
    private float currentAngle;

    private Vector3 defaultWorldPosition;

    private void Start() 
    {
        currentActionState = ActionState.Idle;
        isChangingState = false;
        if (model != null) defaultWorldPosition = model.position;

        NextMuzzle = L_muzzlePoint;
    }

    private void LateUpdate()
{
    if (target == null) return;

    Vector3 lookPos = target.position;
    //lookPos.y = model.position.y;

    model.LookAt(lookPos);
    L_muzzlePoint.LookAt(lookPos);
    R_muzzlePoint.LookAt(lookPos);
}

    private void Update() 
    {
        switch (currentActionState)
        {
            case ActionState.Idle:
                Idle();
                break;
            
            case ActionState.MachineGun:
                MachineGunAttack();
                break;

            case ActionState.Missile:
                MissileAttack();
                break;
        }
    }

    IEnumerator ChangeNextState(float time)
{
    yield return new WaitForSeconds(time);

    switch (currentActionState)
    {
        case ActionState.Idle:
            currentActionState = ActionState.MachineGun;
            break;

        case ActionState.MachineGun:
            currentActionState = ActionState.Missile;
            break;

        case ActionState.Missile:
            currentActionState = ActionState.Idle;
            break;
    }

    isChangingState = false;
}

    public void Idle()
    {
        currentAngle += rotationSpeed * Time.deltaTime;  
        transform.localEulerAngles = new Vector3(0, currentAngle, 0);
        
        if (!isChangingState)
        {
            isChangingState = true;
            StartCoroutine(ChangeNextState(IdleTime));
        }
    }
    
    public void MachineGunAttack()
    {
        if (isChangingState) return;
        isChangingState = true;

        Transform atackStartPoint = null;
        Transform atackEndPoint = null;

        float angle = currentAngle % 360;
        if (angle < 0) angle += 360;

        if (angle < 60 && angle >= 0)
        {
            atackStartPoint = AttckPoint_1A;
            atackEndPoint   = AttckPoint_1B;
        }
        else if (angle < 120 && angle >= 60)
        {
            atackStartPoint = AttckPoint_1B;
            atackEndPoint   = AttckPoint_1A;
        }
        else if (angle < 180 && angle >= 120)
        {
            atackStartPoint = AttckPoint_2A;
            atackEndPoint   = AttckPoint_2B;
        }
        else if (angle < 240 && angle >= 180)
        {
            atackStartPoint = AttckPoint_2B;
            atackEndPoint   = AttckPoint_2A;
        }
        else if (angle < 300 && angle >= 240)
        {
            atackStartPoint = AttckPoint_3A;
            atackEndPoint   = AttckPoint_3B;
        }
        else if (angle < 360 && angle >= 300)
        {
            atackStartPoint = AttckPoint_1A;
            atackEndPoint   = AttckPoint_1B;
        }

        if (atackStartPoint == null || atackEndPoint == null)
        {
            StartCoroutine(ChangeNextState(0));
            return;
        }

        if (model != null) defaultWorldPosition = model.position;

        Sequence seq = DOTween.Sequence();

        seq.Append(model.DOMove(atackStartPoint.position, AttackShiftTime));
        seq.AppendCallback(() =>
        {
            StartCoroutine(FireMachineGun(MachineGunAttackTime));
        });
        
        seq.Append(model.DOMove(atackEndPoint.position, MachineGunAttackTime));
        
        seq.Append(model.DOMove(defaultWorldPosition, AttackShiftTime));
        
        seq.OnComplete(() => {
            StartCoroutine(ChangeNextState(0));
        });
    }

    IEnumerator FireMachineGun(float FireTime)
{
    float startTime = Time.time;
    while (Time.time < startTime + FireTime)
    {
        Shoot();
        yield return new WaitForSeconds(FireRate);
    }
}

    public void Shoot()
    {
        GameObject b =pManager.bulletPool.Get();
        b.transform.position = NextMuzzle.position;
        
        float randomPitch = Random.Range(-spreadAngle, spreadAngle);
            
        Quaternion spreadRotation = Quaternion.Euler(randomPitch, 0, 0);
        b.transform.rotation = NextMuzzle.rotation * spreadRotation;
        
        bullet bulletScript = b.GetComponent<bullet>();

        if(NextMuzzle == L_muzzlePoint)
        {
            NextMuzzle = R_muzzlePoint;
        }
        else
        {
            NextMuzzle = L_muzzlePoint;
        }
    }

    public void MissileAttack()
{
    if (isChangingState) return;

    isChangingState = true;

    StartCoroutine(MissileAttackCoroutine());
}

IEnumerator MissileAttackCoroutine()
{
    // 1発目：必ずプレイヤーの現在位置
    Vector3 targetPos = target.position;

    CreateMissileWarning(targetPos);
    LaunchMissile(targetPos);

    yield return new WaitForSeconds(missileInterval);

    // 残りはプレイヤー周辺にランダム配置
    for (int i = 1; i < missileCount; i++)
    {
        Vector2 random = Random.insideUnitCircle * missileRadius;

        targetPos = target.position + new Vector3(
            random.x,
            0f,
            random.y
        );

        CreateMissileWarning(targetPos);
        LaunchMissile(targetPos);

        yield return new WaitForSeconds(missileInterval);
    }

    yield return new WaitForSeconds(2f);

    isChangingState = false;
    currentActionState = ActionState.Idle;
}

void CreateMissileWarning(Vector3 targetPos)
{
    GameObject warning = Instantiate(
        missileWarningPrefab,
        targetPos,
        Quaternion.identity
    );

    MissileWarning warningScript =
        warning.GetComponent<MissileWarning>();

    warningScript.Initialize(targetPos,pManager,target.transform);
}

public void LaunchMissile(Vector3 targetPosition)
{
    GameObject missile =
        Instantiate(
            missilePrefab,
            missileSpawnPoint.position,
            missileSpawnPoint.rotation
        );

    HelicopterMissile missileScript =
        missile.GetComponent<HelicopterMissile>();

    missileScript.SetTarget(targetPosition);
}

    public void DrawAttackPath(Transform start, Transform end, Color color)
{
    if (start == null || end == null) return;

    Gizmos.color = color;
    Gizmos.DrawSphere(start.position, 0.3f);
    Gizmos.DrawSphere(end.position, 0.3f);
    Gizmos.DrawLine(start.position, end.position);
}

public void OnDrawGizmos()
{
    DrawAttackPath(AttckPoint_1A, AttckPoint_1B, Color.red);
    DrawAttackPath(AttckPoint_2A, AttckPoint_2B, Color.green);
    DrawAttackPath(AttckPoint_3A, AttckPoint_3B, Color.blue);
}
    
    public override void OnDie()
    {
        StartCoroutine(OnDieCoroutine());
    }

    private IEnumerator OnDieCoroutine()
    {
        if (currentState == EnemyState.Dead) yield break;

        currentState = EnemyState.Dead;

        StopCoroutine("RecoverFromDamage");
        
        if (body != null)
        {
            body.transform.DOKill(); 
        }

        if (collider != null) collider.enabled = false;
        
        // 弱点コライダーをすべて無効化する
        if (HP != null && HP.weakPointColliders != null)
        {
            foreach (var col in HP.weakPointColliders)
            {
                if (col != null) col.enabled = false;
            }
        }

        Debug.Log(gameObject.name + " has died.");

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

        Time.timeScale = 0.2f;

        yield return new WaitForSecondsRealtime(3f);

        Time.timeScale = 1f;

        Destroy(gameObject);
    }

}