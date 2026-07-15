using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody; 
    public Transform handPoint; 

    [HideInInspector] public float mouseX;
    [HideInInspector] public float mouseY;
    
    public float mouseSensitivity = 100f; 
    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;
    private float recoilX = 0f;
    private float recoilY = 0f; 
    private float recoilZ = 0f;
    private float lastShotTime;
    
    // HandPointの「本来あるべき場所」を記憶する
    private Vector3 initialHandPos;

    [Header("反動設定")]
    [SerializeField] private bool _useCameraRecoil = true;
    public bool useCameraRecoil 
    {
        get => _useCameraRecoil;
        set 
        {
            _useCameraRecoil = value;
            if (!_useCameraRecoil) recoilX = 0f;
        }
    }
    public float recoilSmoth = 5f;
    public float recoilSmoth_enshutu = 5f;

    void Awake() 
    {
        if (handPoint != null)
        {
            // 起動時の場所を「聖域」として保存しておく、よ
            initialHandPos = handPoint.localPosition; 
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Time.timeScale == 0) return; // ← ポーズ中は処理しない

        HandleRecoilRecovery();
        CameraController();
    }

    // このスクリプトのUpdateから呼び出される、よ
    private void HandleRecoilRecovery()
    {
        // 0.1秒〜0.2秒くらいで戻り始めると、キレが良くなる、ね
        if (Time.time > lastShotTime + 0.15f)
        {
            if (useCameraRecoil)
            {
                recoilX = Mathf.Lerp(recoilX, 0f, recoilSmoth * Time.deltaTime);
                recoilY = Mathf.Lerp(recoilY, 0f, recoilSmoth * Time.deltaTime);
            }
            else
            {
                recoilX = 0f;
                recoilY = 0f;
            }
        }
    }

    // このスクリプトのUpdateから呼び出される、よ
    void CameraController()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        HandleMouseRotation(mouseY);
        ApplyRecoilOffsets();

        playerBody.Rotate(Vector3.up * mouseX);
    }

    // このスクリプトのCameraControllerから呼び出される、よ
    private void HandleMouseRotation(float mouseY)
    {
        if (useCameraRecoil)
        {
            HandleRecoilCompensation(mouseY);
        }
        else
        {
            cameraRotationX -= mouseY;
        }

        cameraRotationX = Mathf.Clamp(cameraRotationX, -90f, 90f);
    }

    // このスクリプトのHandleMouseRotationから呼び出される、よ
    private void HandleRecoilCompensation(float mouseY)
    {
        // --- 視点の計算（リコイル相殺ロジック） ---
        if (mouseY > 0 && recoilX < 0) 
        {
            recoilX += mouseY; 
            if (recoilX > 0) 
            {
                cameraRotationX -= recoilX;
                recoilX = 0;
            }
        }
        else
        {
            cameraRotationX -= mouseY;
        }
    }

    // このスクリプトのCameraControllerから呼び出される、よ
    private void ApplyRecoilOffsets()
    {
        float currentRecoilX = useCameraRecoil ? recoilX : 0f;
        float currentRecoilY = useCameraRecoil ? recoilY : 0f; 

        // --- 1. カメラ（視点）の回転適用 ---
        transform.localRotation = Quaternion.Euler(cameraRotationX + currentRecoilX, currentRecoilY, 0f);
    }

    // normalshotActionのshotから呼び出される、よ
    public void AddRecoil(float forceX, float forceY)
    {
        if (useCameraRecoil) 
        {
            recoilX -= forceX;   // 視点を上に跳ね上げる
            recoilY += Random.Range(-forceY, forceY);
        }
        lastShotTime = Time.time;
    }

    // このスクリプトのPropertyのSetterなどから呼び出される、よ
    public void ClearRecoil()
    {
        recoilX = 0f;
    }
}