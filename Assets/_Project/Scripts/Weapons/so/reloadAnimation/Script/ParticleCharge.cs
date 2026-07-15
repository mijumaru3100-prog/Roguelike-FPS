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
        
        // コンポーネントとマテリアルの取得
        GameObject part = gun.SpecialParts[0];
        var ps = part.GetComponent<ParticleSystem>();
        Renderer rend = part.GetComponent<Renderer>();
        Material mat = rend.material;

        // ★元の色（現在のエミッション色）を自動で取得
        Color originalColor = mat.color;
        // 初期化：スタート時はエミッションを一度黒（発光なし）にする
        mat.SetColor("_EmissionColor", Color.black);

        Sequence seq = DOTween.Sequence();

     

        // 1. 銃を傾ける
        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(tiltAngle, tiltTime));
        seq.Join(mat.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0f),tiltTime));
        // 2. チャージエフェクト再生（傾け終わった瞬間に実行）
        seq.AppendCallback(() => {
            gun.StartReloadCountAnimation(tiltTime);
            PerticleRunner.Play(ps, chargeTime * 0.9f);
        });
        
        mat.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 1f),chargeTime).SetEase(Ease.InCubic);

        // 元の色をベースに、輝度だけを1024倍（強度10）にした目標色を作る
        Color targetEmission = new Color(originalColor.r, originalColor.g, originalColor.b) * 1024f;
        
        // じわーっと発光させる（chargeTime 分の時間を消費）
        seq.Append(mat.DOColor(targetEmission, "_EmissionColor", chargeTime).SetEase(Ease.InCubic));

        // 3. 銃の角度を初期位置に戻す
        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(defaultGunRot, tiltTime));
    
        seq.OnComplete(() => {
            gun.StopReloadCountAnimation();
            gun.OnReloadComplete();
        });
    }
}