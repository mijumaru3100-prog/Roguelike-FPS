using UnityEngine;
using System.Collections;

public class MissileWarning : MonoBehaviour
{
    public Transform purpleCircle;

    public float warningTime = 2f;
    public float radius = 3f;
    public int damage = 1;

    private Vector3 targetPosition;
    private PlayerManager pManager;
    private Transform player;

    public void Initialize(
        Vector3 position,
        PlayerManager manager,
        Transform playerTransform)
    {
        targetPosition = position;
        pManager = manager;
        player = playerTransform;

        transform.position = position;

        StartCoroutine(Warning());
    }

    IEnumerator Warning()
    {
        float timer = 0f;

        while (timer < warningTime)
        {
            timer += Time.deltaTime;

            float progress = timer / warningTime;

            purpleCircle.localScale =
                Vector3.one * progress;

            yield return null;
        }

        // 着弾
        CheckDamage();

        Destroy(gameObject);
    }

    void CheckDamage()
    {
        Vector2 playerPos = new Vector2(
            player.position.x,
            player.position.z
        );

        Vector2 attackPos = new Vector2(
            targetPosition.x,
            targetPosition.z
        );

        float distance = Vector2.Distance(
            playerPos,
            attackPos
        );

        if (distance <= radius)
        {
            pManager.playerHP.TakeDamage(damage);
        }
    }
}