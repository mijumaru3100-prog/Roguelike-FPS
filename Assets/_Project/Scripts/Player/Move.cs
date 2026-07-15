using UnityEngine;

public class Move:MonoBehaviour
{
    //まずは使う関数やスクリプトの定義
    public CharacterController controller; 
    public PlayerManager pManager; 
    private Vector3 currentHorizontalVelocity;
    private Vector3 VerticalVelocity;

    // 移動しているかどうか（外部から取得可能）
    public bool isMoving => currentHorizontalVelocity.magnitude > 0.05f;

    //ここでステータス
    public float moveSpeed = 10f;
    public float gravity = -9.8f;
    public float fallMultiple = 3;
    public float jumpHeight = 2f;
    public float warkAccelaration = 10f;
    public float airAcceleration = 2f;
    [Header("速度は常にyaruki倍されてる(yaruki=現在速度/目標速度)")]
    public float minimumYaruki = 0.4f;
    public float slowDownYaruki = 1.5f;

    [Header("スイッチ")]
    public bool canNotWark = false;
    public bool canNotJump = false;




 
  void Update()
 {
    currentHorizontalVelocity_changer();
    VerticalVelocity_controller();
    Vector3 velocity = currentHorizontalVelocity + VerticalVelocity;
    controller.Move(velocity * Time.deltaTime);

if (transform.position.y < -100f)
{
    ResetPosition();
}
}

public void ResetPosition() 
{
// 1. キャラクターコントローラーを一旦止める
    CharacterController controller = GetComponent<CharacterController>();
    if (controller != null) controller.enabled = false;

    // 2. 速度（慣性）をリセットする（Rigidbodyを使っている場合）
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null) rb.linearVelocity = Vector3.zero;

    // 3. 座標をリスポーン地点に合わせる
    // .position を代入します
    transform.position = pManager.dungeonManager.currentRoomScript.respawnPoint.transform.position;;

    // 4. コントローラーを再起動
    if (controller != null) controller.enabled = true;
}
 public float inputX,inputZ;

// このスクリプトのUpdateから呼び出される、よ
void currentHorizontalVelocity_changer()
{
    if(canNotWark) return;

    inputX = Input.GetAxis("Horizontal");
    inputZ = Input.GetAxis("Vertical");

    Vector3 moveDirection = (inputX * transform.right + inputZ * transform.forward);
    if (moveDirection.magnitude > 1f) moveDirection.Normalize();

    bool hasInput = moveDirection.sqrMagnitude > 0.01f;

    // 停止専用
    if (!hasInput && controller.isGrounded)
    {
        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity,Vector3.zero,warkAccelaration * 2f * Time.deltaTime);
        if (currentHorizontalVelocity.magnitude < 0.05f)currentHorizontalVelocity = Vector3.zero;
        return;
    }

    float accel = controller.isGrounded ? warkAccelaration : airAcceleration;
    accel *= yarukiChecker(moveDirection);

    float dot = Vector3.Dot(
        currentHorizontalVelocity.normalized,
        moveDirection.normalized
    );

    if (dot < -0.1f)
        accel *= 3f;

    Vector3 targetVelocity = moveDirection * moveSpeed *(1+pManager.sharedStats.MoveSpeedMultiple);

    currentHorizontalVelocity = Vector3.Lerp(
        currentHorizontalVelocity,
        targetVelocity,
        accel * Time.deltaTime
    );
}

   //nowAccerarationの計算に使う
   // このスクリプトのcurrentHorizontalVelocity_changerから呼び出される、よ
   public float yarukiChecker(Vector3 moveDirection)
   {
    float yaruki = 0;

    if(moveSpeed <= 0)
    {
        Debug.Log("速度が０より小さいよ(yarukiChecker)");
        return yaruki;
    }

    if(moveDirection.magnitude < 0.01)
    {
        yaruki = slowDownYaruki;
        return yaruki;
    }

    yaruki = currentHorizontalVelocity.magnitude / moveSpeed;
    //やる気の値を整える(x~1)
    if(yaruki >  1){yaruki = 1;}
    if(yaruki < minimumYaruki){yaruki = minimumYaruki;}
    
    return yaruki;
   }

 // このスクリプトのUpdateから呼び出される、よ
 void VerticalVelocity_controller()
 {
    if(canNotJump) return;

    float nowgravity;

    if(controller.isGrounded && VerticalVelocity.y < 0)
    {
        VerticalVelocity.y = -2f;
    }
    else
    {
        if(VerticalVelocity.y < 0)
        {
            nowgravity = gravity * fallMultiple;
        }
        else
        {
            nowgravity = gravity;
        }
        VerticalVelocity.y += nowgravity * Time.deltaTime;
    }

    if(Input.GetButtonDown("Jump") && controller.isGrounded)
    {
        VerticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity * (1+pManager.sharedStats.JumpHeightMultiple));

        foreach (var p in pManager.activePassives)
        {
            p.OnJump(pManager);
        }
    }
 }
 
}