using UnityEngine;

public class FlashBullet : bullet
{
    [Header("Flash Settings")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float radius = 5f;

    private bool hasFlashed = false;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (hasFlashed) return;
        if(other.CompareTag("Player")) return;
        if(other.CompareTag("Player"))

        hasFlashed = true;

        Flash();

        DeactivateBullet();
    }

    private void Flash()
    {
        Collider[] hits =
            Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            PlayerManager player =
                hit.GetComponentInParent<PlayerManager>();

            if (player != null)
            {
                player.Flash(duration);
            }

            NormalEnemyAI enemy =
                hit.GetComponentInParent<NormalEnemyAI>();

            if (enemy != null)
            {
                enemy.Stun(duration);
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        hasFlashed = false;
    }
}