using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/reloadAnimation/DoubleBarrel")]
public class DoubleBarrel : reloadAnimation
{
    [Header("基準となる時間（1.0倍の時）")]
    [Tooltip("銃身を開く時間")] public float baseOpenTime = 0.5f;
    [Tooltip("薬莢/マガジンを落とす時間")] public float baseDropTime = 0.4f;
    [Tooltip("マガジンを挿入する時間")] public float baseInsertTime = 0.5f;
    [Tooltip("落下から挿入開始までの待ち時間")] public float baseDropToInsertTime = 0.1f; 

    [Header("リロードモーション設定")]
    [Tooltip("開閉時の傾き角度")] public Vector3 OpenAngle;
    [Tooltip("薬莢の落下先位置")] public Vector3 dropDistance;
    [Tooltip("新弾丸の出現位置")] public Vector3 spawnPoint; 
    [Tooltip("シェイク強度")] public float shakeStrength;

    [Tooltip("リロード時の腕の位置")]
    [UnityEngine.Serialization.FormerlySerializedAs("handMagazinePoint")]
    public Vector3 ArmMagazinePoint;

    [Tooltip("リロード時の手首の回転補正")]
    public Vector3 tekubiHandMagazineRotation;

    [Header("効果音設定")]
    [Tooltip("銃身を開く音")] public AudioClip magOpenClip; 
    [Tooltip("排莢/落下音")] public AudioClip magDropClip;    
    [Tooltip("弾丸装填音")] public AudioClip magInsertClip;         
    [Tooltip("銃身を閉じる音")] public AudioClip magCloseClip; 
    [Tooltip("閉じる音の遅延時間")] public float CloseSoundDelay = 0.1f;
   
    public override void Play(GunBase gun)
    {
        float speedMult = gun.GetTotalReloadSpeedMultiplier();

        float OpenTime = baseOpenTime / speedMult;
        float dropTime = baseDropTime / speedMult;
        float insertTime = baseInsertTime / speedMult;
        float dropToInsertTime = baseDropToInsertTime / speedMult;
       
        Vector3 defaultGunRot = gun.gunTiltModel.transform.localEulerAngles;
        Vector3 defaultArmPosition = gun.MoveArm.transform.localPosition;
        Vector3 defaultTekubiRot = gun.MoveTekubi.transform.localEulerAngles;
        Vector3 defaultMagazinePoint = gun.magazineObject.transform.localPosition;
     
        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() => {
            gun.PlayReloadSound(magOpenClip);    
        });
        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(OpenAngle, OpenTime));

        seq.AppendCallback(() => { 
            gun.OnMagazineEjected();
            gun.PlayReloadSound(magDropClip);    
        });
        seq.Append(gun.magazineObject.transform.DOLocalMove(dropDistance,dropTime));
        seq.Join(gun.MoveArm.transform.DOLocalMove(spawnPoint,dropTime));

        seq.AppendCallback(() => {
            gun.magazineObject.transform.position = defaultMagazinePoint;
        });

        seq.Append(gun.magazineObject.transform.DOLocalMove(defaultMagazinePoint,dropToInsertTime));
        seq.Join(gun.MoveArm.transform.DOLocalMove(ArmMagazinePoint,dropToInsertTime));

        seq.AppendCallback(() => {
            gun.StartReloadCountAnimation(OpenTime);
            gun.PlayReloadSound(magInsertClip);
            DOVirtual.DelayedCall(CloseSoundDelay, () => 
            {
                gun.PlayReloadSound(magCloseClip);
            }); 
        });

        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(defaultGunRot, OpenTime));
        seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot, OpenTime));
        seq.Join(gun.MoveArm.transform.DOLocalMove(defaultArmPosition, OpenTime));

        seq.OnComplete(() => {
            gun.StopReloadCountAnimation();
            gun.OnReloadComplete();
        });
    }
}