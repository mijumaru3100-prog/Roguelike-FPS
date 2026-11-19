using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverUI;


    public GameObject playerRoot;
    public CanvasGroup gameOverCanvasGroup;

    [SerializeField] private float fadeDuration = 1.5f;

    [SerializeField] private float deathForce = 3f;
[SerializeField] private float deathUpForce = 1f;

    private bool isGameOver = false;

    private void Start()
    {
        gameOverUI.SetActive(false);
    }

    [SerializeField] private CharacterController playerController;
[SerializeField] private Rigidbody playerRigidbody;
[SerializeField] private int deadLayer;

public void GameOver()
{
    isGameOver = true;

    // レイヤー変更（子も含む）
    ChangeLayerRecursively(playerRoot, deadLayer);

    // 移動を停止
    playerController.enabled = false;

    // 物理演算を有効化
    playerRigidbody.isKinematic = false;
    playerRigidbody.useGravity = true;

    Vector3 forceDirection =
    (playerRoot.transform.forward + Vector3.up * deathUpForce).normalized;

playerRigidbody.AddForce(
    forceDirection * deathForce,
    ForceMode.Impulse
);

    // 死亡画面
    gameOverUI.SetActive(true);
    gameOverCanvasGroup.alpha = 0f;

    gameOverCanvasGroup.DOFade(1f, fadeDuration)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            Time.timeScale = 0f;
        });
}

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }


private void ChangeLayerRecursively(GameObject target, int layer)
{
    target.layer = layer;

    foreach (Transform child in target.transform)
    {
        ChangeLayerRecursively(child.gameObject, layer);
    }
}
}