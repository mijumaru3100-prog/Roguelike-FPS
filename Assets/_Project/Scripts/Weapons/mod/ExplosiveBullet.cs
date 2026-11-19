using UnityEngine;
using System.Collections.Generic;

public class ExplosionBullet : bullet
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private ParticleSystem explosionEffect;

    private bool hasExploded = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        hasExploded = false;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (hasExploded) return;

        hasExploded = true;

        Explode();

        DeactivateBullet();
    }

    private void Explode()
    {
        Vector3 explosionPosition = transform.position;

        if (explosionEffect != null)
        {
            ParticleSystem effect = Instantiate(
                explosionEffect,
                explosionPosition,
                Quaternion.identity
            );

            effect.Play();

            Destroy(
                effect.gameObject,
                effect.main.duration + effect.main.startLifetime.constantMax
            );
        }

        Collider[] hitColliders =
            Physics.OverlapSphere(
                explosionPosition,
                explosionRadius
            );

        HashSet<EnemyHP> damagedEnemies =
            new HashSet<EnemyHP>();

        HashSet<ShootableObject> triggeredObjects =
            new HashSet<ShootableObject>();

        foreach (Collider hitCol in hitColliders)
        {
            if (hitCol.gameObject == gameObject)
                continue;

            Vector3 direction =
                (hitCol.transform.position - explosionPosition).normalized;

            EnemyHP enemyHP =
                hitCol.GetComponentInParent<EnemyHP>();

            if (enemyHP != null && !damagedEnemies.Contains(enemyHP))
            {
                enemyHP.TakeDamage(
                    explosionDamage,
                    DamageType.Impact
                );

                Vector3 knockbackDirection =
                    (direction + Vector3.up * 0.2f).normalized;

                enemyHP.ApplyKnockback(
                    knockbackDirection,
                    knockbackForce
                );

                damagedEnemies.Add(enemyHP);
            }

            ShootableObject shootable =
                hitCol.GetComponentInParent<ShootableObject>();

            if (shootable != null &&
                !triggeredObjects.Contains(shootable))
            {
                if (shootable is Pallet pallet)
                {
                    pallet.BreakAll(
                        explosionPosition,
                        explosionRadius,
                        explosionForce
                    );
                }
                else
                {
                    shootable.Attacked(
                        hitCol,
                        direction,
                        explosionForce
                    );
                }

                triggeredObjects.Add(shootable);
            }

            Rigidbody rb =
                hitCol.GetComponentInParent<Rigidbody>();

            if (rb != null && !rb.isKinematic)
            {
                rb.AddExplosionForce(
                    explosionForce,
                    explosionPosition,
                    explosionRadius,
                    1f,
                    ForceMode.Impulse
                );
            }
        }
    }
}