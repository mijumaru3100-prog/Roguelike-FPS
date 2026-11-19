using UnityEngine;

public class WeaponBobbing : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Move playerMovement; 
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private PlayerManager pManager;
    [Header("歩行時の揺れの設定")]
    public float walkingBobbingSpeed = 10f; 

    [Header("ADS時の揺れの設定")]
    public float adsMultiple_v = 0.3f;
    public float adsMultiple_h = 0.3f;
    [Header("回転")]
    public float tiltAmount = 3f;
    public float tiltSmooth = 8f;
    [Header("ジャンプ")]
     public float jumpBobbingMultiple =  1f;

    [Header("停止慣性")]
    public float stopKickAmount = 0.015f;

    [Header("足音")]
[SerializeField] private AudioSource footstepSource;
[SerializeField] private AudioClip[] footstepClips;
[SerializeField] private AudioClip LandingSound;
    

    private float timer = 0f;
    private Vector3 defaultPosition;
    private float landingOffset = 0f;
    private bool wasGrounded;
    private bool wasMoving;
    private float stopOffset;
    private GunBase gun;

    void Start()
    {
        gun = pManager.currentWeapon;
        defaultPosition = transform.localPosition;
    }
    private float previousSin;

void Update()
{
    Bobbing();
    Rotation();

    Footstep();

    if (!playerMovement.isMoving)
{
    previousSin = Mathf.Sin(timer);
    Reset();
}

    if (!wasGrounded && playerMovement.controller.isGrounded)
{
    landingOffset = -0.05f;
    PlayLandingSound();
}

    wasGrounded = playerMovement.controller.isGrounded;

    landingOffset = Mathf.Lerp(
        landingOffset,
        0f,
        Time.deltaTime * 10f
    );

    if (wasMoving && !playerMovement.isMoving)
    {
        stopOffset = stopKickAmount;
    }

    stopOffset = Mathf.Lerp(
        stopOffset,
        0f,
        Time.deltaTime * 8f
    );

    wasMoving = playerMovement.isMoving;
}

    void Bobbing()
    {
        if(playerMovement.isMoving)
        {
            timer += Time.deltaTime * walkingBobbingSpeed;
        }
            float verticalOffset = Mathf.Sin(timer) * gun.bobbingAmount;
            float horizontalOffset = Mathf.Cos(timer * 2.0f) * gun.sideBobbingAmount;

            float currentBackwardOffset = gun.BackwardOffset;

            if(pManager.currentWeapon.isAiming)
            {
                verticalOffset *= adsMultiple_v;
                horizontalOffset *= adsMultiple_h;
                currentBackwardOffset = 0f;
            }
        

            Vector3 targetPos = new Vector3(
                defaultPosition.x + horizontalOffset,
                defaultPosition.y + verticalOffset + landingOffset,
                defaultPosition.z + currentBackwardOffset + stopOffset
            );

            if (playerMovement.controller.velocity.y > 0.1f)
            {
                targetPos.y += 0.02f * jumpBobbingMultiple;
            }
            if (playerMovement.controller.velocity.y < -0.1f)
            {
                targetPos.y -= 0.02f * jumpBobbingMultiple;
            }

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                targetPos,
                Time.deltaTime * 10f
            );
    }

    void Rotation()
    {
        float moveX = playerMovement.inputX;
        Quaternion moveTilt = Quaternion.Euler(
            0f,
            0f,
            -moveX * tiltAmount
        );
        
        Quaternion swayRotation;
        if(playerMovement.pManager.currentWeapon.isAiming)
        {
            swayRotation = Quaternion.Euler(
                mouseLook.mouseY * gun.swayAmount *(-0.3f),
                mouseLook.mouseX * gun.swayAmount*(0.3f),
                mouseLook.mouseX * gun.swayAmount*(0.3f)
            );
        }
        else
        {
            swayRotation = Quaternion.Euler(
                mouseLook.mouseY * gun.swayAmount *(-1f),
                mouseLook.mouseX * gun.swayAmount,
                mouseLook.mouseX * gun.swayAmount*(-1f)
            );
        }
        Quaternion targetRotation = moveTilt * swayRotation;

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * tiltSmooth
        );
    }

    void Reset()
    {
        timer = timer;
        transform.localPosition = Vector3.Lerp(transform.localPosition, defaultPosition, Time.deltaTime * 5f);
        
        transform.localRotation = Quaternion.Lerp(
        transform.localRotation,
        Quaternion.identity,
        Time.deltaTime * tiltSmooth
        );
    }

    void Footstep()
{
    bool grounded = playerMovement.controller.isGrounded;

    if (!playerMovement.isMoving || !grounded)
    {
        previousSin = Mathf.Sin(timer);
        return;
    }

    float currentSin = Mathf.Sin(timer);

    if (previousSin < 0f && currentSin >= 0f)
    {
        PlayFootstep();
    }

    previousSin = currentSin;
}

void PlayFootstep()
{
    if (footstepSource == null || footstepClips.Length == 0)
        return;

    AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

    footstepSource.pitch = Random.Range(0.95f, 1.05f);
    footstepSource.PlayOneShot(clip);
}

void PlayLandingSound()
{
    footstepSource.PlayOneShot(LandingSound);
}
}