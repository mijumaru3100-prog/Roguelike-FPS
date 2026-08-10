using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Hericopter : MonoBehaviour
{
    public enum ActionState { Idle, MachineGun }
    
    [Header("現在の状態")]
    public ActionState currentState = ActionState.Idle;
    
    [Header("共通設定")]
    public PlayerManager pManager;
    public Transform target;
    public Transform model;
    public EnemyHP HP;
    
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
    public int damage = 1; 
    public float bulletSpeed = 10;

    [Header("回転設定")]
    public float rotationSpeed = 4f;
    public float IdleTime;
    
    public bool isChangingState;
    private float currentAngle;

    private Vector3 defaultWorldPosition;

    private void Start() 
    {
        currentState = ActionState.Idle;
        if (model != null) defaultWorldPosition = model.position;

        NextMuzzle = L_muzzlePoint;
    }

    private void LateUpdate()
{
    if (target == null) return;

    Vector3 lookPos = target.position;
    //lookPos.y = model.position.y;

    model.LookAt(lookPos);
}

    private void Update() 
    {
        switch (currentState)
        {
            case ActionState.Idle:
                Idle();
                break;
            
            case ActionState.MachineGun:
                MachineGunAttack();
                break;
        }
    }

    IEnumerator ChangeNextState(float time)
    {
        yield return new WaitForSeconds(time);
        
        switch (currentState)
        {
            case ActionState.Idle:
                currentState = ActionState.MachineGun;
                break;
            
            case ActionState.MachineGun:
                currentState = ActionState.Idle;
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
        if(bulletScript != null)
        {
            bulletScript.damage = damage;
            bulletScript.pManager = pManager;
            bulletScript.bulletSpeed = bulletSpeed;
        }

        if(NextMuzzle == L_muzzlePoint)
        {
            NextMuzzle = R_muzzlePoint;
        }
        else
        {
            NextMuzzle = L_muzzlePoint;
        }
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
    
}