using UnityEngine;
using System.Collections.Generic;

public class Bomb : ShootableObject
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private GameObject ViewObject; 

    private bool hasExploded = false;

    public override void Reset() 
    {
        hasExploded = false;
        gameObject.layer = 0;
        ViewObject.SetActive(true);
    }

    public override void Attacked(Collider hitCollider, Vector3 AttackedDirection, float AttackedForce)
    {
        if (hasExploded) return;
        Explode();
    }

    private void Explode()
    {
        hasExploded = true;
        
        ViewObject.SetActive(false);
        gameObject.layer = 8;

        // エフェクトの生成
        if (explosionEffect != null)
        {
            explosionEffect.Play();
        }

        // 爆発範囲内のコライダーを取得
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        // 重複して処理しないようにするためのセット
        HashSet<EnemyHP> damagedEnemies = new HashSet<EnemyHP>();
        HashSet<ShootableObject> triggeredObjects = new HashSet<ShootableObject>();

        foreach (var hitCol in hitColliders)
        {
            // 自分自身は無視
            if (hitCol.gameObject == gameObject) continue;

            // 爆心地からの方向
            Vector3 direction = (hitCol.transform.position - transform.position).normalized;

            // 1. 敵へのダメージとノックバック
            EnemyHP enemyHP = hitCol.GetComponentInParent<EnemyHP>();
            if (enemyHP != null && !damagedEnemies.Contains(enemyHP))
            {
                // ダメージ処理
                enemyHP.TakeDamage(explosionDamage, DamageType.Impact);
                
                // ノックバック処理 (少し上に浮かせる)
                Vector3 knockbackDir = (direction + Vector3.up * 0.2f).normalized;
                enemyHP.ApplyKnockback(knockbackDir, knockbackForce);
                
                damagedEnemies.Add(enemyHP);
            }

            // 2. 他のShootableObjectの連鎖起動
            ShootableObject shootable = hitCol.GetComponentInParent<ShootableObject>();
            if (shootable != null && shootable != this && !triggeredObjects.Contains(shootable))
            {
                if (shootable is Pallet pallet)
                {
                    // パレットの場合は全体を吹き飛ばす
                    pallet.BreakAll(transform.position, explosionRadius, explosionForce);
                }
                else
                {
                    // 通常のアタックイベント
                    shootable.Attacked(hitCol, direction, explosionForce);
                }
                triggeredObjects.Add(shootable);
            }

            // 3. 物理演算オブジェクト（Rigidbody）への爆発力
            Rigidbody rb = hitCol.GetComponentInParent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }
        }
    }


    // エディタ上で爆発範囲を確認しやすくするためのギズモ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
