using UnityEngine;
using DG.Tweening;

public abstract class shotAction:ScriptableObject
{
    // gunbase_saigenのfireから呼び出される、よ
    public abstract void shot(GunBase baseGun);

public void MakeHitEffect(PlayerManager pManager, RaycastHit hit, Vector3 rayDir)
{
    RaycastHit surfaceHit;

    // 少しだけ外に出す（重要）
    Vector3 origin = hit.point + hit.normal * 0.01f;

    // 壁方向に探す（hit.normalが正解）
    if (Physics.Raycast(origin, -hit.normal, out surfaceHit, 0.05f))
    {
        hit = surfaceHit;
    }

    GameObject mark = pManager.BulletMarkPool.Get();
    

    mark.transform.position = hit.point + hit.normal * 0.002f; // ←超微調整

    mark.transform.rotation =
        Quaternion.LookRotation(hit.normal) *
        Quaternion.Euler(0, 0, Random.Range(0, 360));

    mark.transform.localScale = mark.transform.localScale * Random.Range(0.8f, 1.2f);

    mark.GetComponent<Renderer>().material =
        pManager.bulletHoleMats[Random.Range(0, pManager.bulletHoleMats.Length)];

    DOVirtual.DelayedCall(30f,
        () => pManager.BulletMarkPool.ReturnToPool(mark));

    // ヒットエフェクト
    GameObject effect = pManager.HitEffectPool.Get();
    ParticleSystem effectComponent = effect.GetComponent<ParticleSystem>();  

    effect.transform.SetPositionAndRotation(
        hit.point,
        Quaternion.LookRotation(hit.normal)
    );

    effectComponent.Play();
    DOVirtual.DelayedCall(1f,
        () => pManager.HitEffectPool.ReturnToPool(effect));
}
}