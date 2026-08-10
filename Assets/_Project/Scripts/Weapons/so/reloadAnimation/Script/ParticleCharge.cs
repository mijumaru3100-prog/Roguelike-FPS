using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/reloadAnimation/ParticleCharge")]
public class ParticleCharge : reloadAnimation
{
    [Header("基準となる時間（1.0倍の時）")]
    public float originalTiltTime = 0.5f;
    public float originalChargeTime = 0.4f;

    [Header("リロードモーション設定")]
    public Vector3 tiltAngle;
      
    public AudioClip ChargeClip;      

    public override void Play(GunBase gun)
    {
        float speedMult = gun.GetTotalReloadSpeedMultiplier();

        float tiltTime = originalTiltTime / speedMult;
        float chargeTime = originalChargeTime / speedMult;

        Vector3 defaultGunRot = gun.gunTiltModel.transform.localEulerAngles;
        
        GameObject part = gun.SpecialParts[0];
        var ps = part.GetComponent<ParticleSystem>();
        Renderer rend = part.GetComponent<Renderer>();
        Material mat = rend.material;

        Color originalColor = mat.color;
        mat.SetColor("_EmissionColor", Color.black);

        Sequence seq = DOTween.Sequence();

        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(tiltAngle, tiltTime));
        seq.Join(mat.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0f),tiltTime));
        seq.AppendCallback(() => {
            gun.StartReloadCountAnimation(tiltTime);
            PerticleRunner.Play(ps, chargeTime * 0.9f);
        });
        
        mat.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 1f),chargeTime).SetEase(Ease.InCubic);

        Color targetEmission = new Color(originalColor.r, originalColor.g, originalColor.b) * 1024f;
        
        seq.Append(mat.DOColor(targetEmission, "_EmissionColor", chargeTime).SetEase(Ease.InCubic));

        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(defaultGunRot, tiltTime));
    
        seq.OnComplete(() => {
            gun.StopReloadCountAnimation();
            gun.OnReloadComplete();
        });
    }
}