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

    [HideInInspector] public bool useDirectRecoil = false;

    void Awake() 
    {
        if (handPoint != null)
        {
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
        if (Time.timeScale == 0) return;

        HandleRecoilRecovery();
        CameraController();
    }

    private void HandleRecoilRecovery()
    {
        if (useDirectRecoil) return;

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

    void CameraController()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        HandleMouseRotation(mouseY);
        ApplyRecoilOffsets();

        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleMouseRotation(float mouseY)
    {
        if (useCameraRecoil && !useDirectRecoil)
        {
            HandleRecoilCompensation(mouseY);
        }
        else
        {
            cameraRotationX -= mouseY;
        }

        cameraRotationX = Mathf.Clamp(cameraRotationX, -90f, 90f);
    }

    private void HandleRecoilCompensation(float mouseY)
    {
        if (mouseY < 0 && recoilX < 0) 
        {
            recoilX -= mouseY; 
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

    private void ApplyRecoilOffsets()
    {
        float currentRecoilX = useCameraRecoil ? recoilX : 0f;
        float currentRecoilY = useCameraRecoil ? recoilY : 0f; 

        transform.localRotation = Quaternion.Euler(cameraRotationX + currentRecoilX, currentRecoilY, 0f);
    }

    public void AddRecoil(float forceX, float forceY)
    {
        if (useDirectRecoil) return;
        if (useCameraRecoil) 
        {
            recoilX -= forceX;
            recoilY += Random.Range(-forceY, forceY);
        }
        lastShotTime = Time.time;
    }

    public void SetRecoilDirect(float x, float y)
    {
        recoilX = x;
        recoilY = y;
    }

    public void ClearRecoil()
    {
        recoilX = 0f;
    }
}