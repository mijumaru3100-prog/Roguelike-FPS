using UnityEngine;
using DG.Tweening;

public class TitleManager : MonoBehaviour
{
    [Header("Title UI")]
    [SerializeField] private GameObject otherUI;

    [SerializeField] private CanvasGroup startText;
    [SerializeField] private CanvasGroup titleCanvas;

    [Header("Player")]
    [SerializeField] private Move playerMovement;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private PlayerManager pManager;

    [Header("Gun / Hand")]
    [SerializeField] private Transform gunRoot;
    [SerializeField] private float gunDownOffset = 0.3f;
    [SerializeField] private float gunMoveDuration = 0.35f;

    [Header("Camera Idle Motion")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraMoveX = 0.015f;
    [SerializeField] private float cameraMoveY = 0.01f;
    [SerializeField] private float cameraMoveDuration = 2.5f;

    [Header("Title Fade")]
    [SerializeField] private float titleFadeDuration = 1.0f;

    [Header("Start SE")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startSE;

    private bool started = false;

    private Vector3 gunOriginalLocalPosition;
    private Vector3 cameraOriginalLocalPosition;

    private void Start()
    {
        // -------------------------
        // 初期位置を保存
        // -------------------------

        if (gunRoot != null)
        {
            gunOriginalLocalPosition = gunRoot.localPosition;

            // 武器・手を下げる
            gunRoot.localPosition =
                gunOriginalLocalPosition + Vector3.down * gunDownOffset;
        }

        if (cameraTransform != null)
        {
            cameraOriginalLocalPosition = cameraTransform.localPosition;

            StartCameraIdleMotion();
        }

        // -------------------------
        // タイトル状態
        // -------------------------

        playerMovement.enabled = false;

        // タイトル中は視点だけ動かせる
        mouseLook.enabled = false;

        if (otherUI != null)
            otherUI.SetActive(false);

        pManager.IsTitle = true;



        // -------------------------
        // タイトルフェード
        // -------------------------

        if (titleCanvas != null)
        {
            titleCanvas.alpha = 0f;

            titleCanvas
                .DOFade(1f, titleFadeDuration)
                .SetEase(Ease.OutQuad);
        }

        // -------------------------
        // START文字点滅
        // -------------------------

        if (startText != null)
        {
            startText.alpha = 1f;

            startText
                .DOFade(0.2f, 0.7f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        // -------------------------
        // カーソル
        // -------------------------

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (started)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartGame();
        }
    }

    private void StartCameraIdleMotion()
    {
        if (cameraTransform == null)
            return;

        // 現在位置から少し右へ
        cameraTransform
            .DOLocalMoveX(
                cameraOriginalLocalPosition.x + cameraMoveX,
                cameraMoveDuration
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // 現在位置から少し上へ
        cameraTransform
            .DOLocalMoveY(
                cameraOriginalLocalPosition.y + cameraMoveY,
                cameraMoveDuration * 1.3f
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StartGame()
    {
        started = true;

        // -------------------------
        // カメラ揺れ停止
        // -------------------------

        if (cameraTransform != null)
        {
            cameraTransform.DOKill();

            cameraTransform
                .DOLocalMove(
                    cameraOriginalLocalPosition,
                    0.25f
                )
                .SetEase(Ease.OutQuad);
        }

        // -------------------------
        // START文字停止
        // -------------------------

        if (startText != null)
        {
            startText.DOKill();

            startText
                .DOFade(0f, 0.2f);
        }

        // -------------------------
        // タイトル消去
        // -------------------------

        if (titleCanvas != null)
        {
            titleCanvas.DOKill();

            titleCanvas
                .DOFade(0f, 0.3f);
        }

        // -------------------------
        // 武器を構える
        // -------------------------

        if (gunRoot != null)
        {
            gunRoot
                .DOLocalMove(
                    gunOriginalLocalPosition,
                    gunMoveDuration
                )
                .SetEase(Ease.OutQuad);
        }

        // -------------------------
        // SE
        // -------------------------

        if (audioSource != null && startSE != null)
        {
            audioSource.PlayOneShot(startSE);
        }

        // -------------------------
        // ゲーム開始
        // -------------------------

        DOVirtual.DelayedCall(gunMoveDuration, () =>
        {

            if (otherUI != null)
                otherUI.SetActive(true);

            playerMovement.enabled = true;
            mouseLook.enabled = true;

            pManager.IsTitle = false;

            Debug.Log("Game Start");
        });
    }

    private void OnDestroy()
    {
        // シーン終了時にTweenを止める
        if (cameraTransform != null)
            cameraTransform.DOKill();

        if (gunRoot != null)
            gunRoot.DOKill();

        if (startText != null)
            startText.DOKill();

        if (titleCanvas != null)
            titleCanvas.DOKill();
    }
}