using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/reloadAnimation/SCARType")]
public class SCARTypeReload : reloadAnimation
{
    [Header("基準となる時間（1.0倍の時）")]
    [Tooltip("銃を傾ける時間")] public float baseTiltTime = 0.5f;
    [Tooltip("マガジンを落とす時間")] public float baseDropTime = 0.4f;
    [Tooltip("マガジンを挿入する時間")] public float baseInsertTime = 0.5f;
    [Tooltip("マガジン落下から挿入開始までの待ち時間")] public float baseDropToInsertTime = 0.1f; 
    [Tooltip("ボルトを引く時間")] public float baseBoltMoveTime = 0.5f;
    [Tooltip("ボルト操作の手の移動時間")] public float baseHand_boltMoveTime = 0.5f;

    [Header("リロードモーション設定")]
    [Tooltip("リロード時の銃の傾き角度")] public Vector3 tiltAngle;
    [Tooltip("マガジンの落下距離")] public Vector3 dropDistance;
    [Tooltip("新マガジンの出現位置")] public Vector3 spawnPoint; 
    [Tooltip("シェイク強度")] public float shakeStrength;

    [Tooltip("リロード時の腕の位置")]
    [UnityEngine.Serialization.FormerlySerializedAs("handMagazinePoint")]
    public Vector3 ArmMagazinePoint;

    [Tooltip("リロード時の手首の回転補正")]
    public Vector3 tekubiHandMagazineRotation;

    [Header("ボルトを引くモーション設定")]
    [Tooltip("ボルト操作時の手の初期位置")] public Vector3 boltHandPosition;
    [Tooltip("ボルトを引いた時の手の位置")] public Vector3 boltHandPosition2;
    [Tooltip("ボルトパーツの移動量")] public Vector3 boltMoveDistance;
    [Tooltip("ボルト操作時の手首の回転補正")] public Vector3 tekubiBoltRotation;
    [Tooltip("ボルトのリリース時間")] public float releaseTime = 0.05f; 
        
    [Header("効果音設定")]
    [Tooltip("マガジン取り出し音")] public AudioClip magOutClip;    
    [Tooltip("マガジン挿入音")] public AudioClip magInsertClip; 
    [Tooltip("マガジン叩き込み音")] public AudioClip magHitClip;    
    [Tooltip("ボルト操作音")] public AudioClip boltClip;      

    public override void Play(GunBase gun)
    {
        bool isEmptyReload = gun.currentAmmo == 0;
        float speedMult = gun.GetTotalReloadSpeedMultiplier();

        float tiltTime = baseTiltTime / speedMult;
        float dropTime = baseDropTime / speedMult;
        float insertTime = baseInsertTime / speedMult;
        float dropToInsertTime = baseDropToInsertTime / speedMult;
        float boltMoveTime = baseBoltMoveTime / speedMult;
        float hand_boltMoveTime = baseHand_boltMoveTime / speedMult;

        Vector3 defaultGunRot = gun.gunTiltModel.transform.localEulerAngles;
        Vector3 defaultArmPosition = gun.MoveArm.transform.localPosition;
        Vector3 defaultTekubiRot = gun.MoveTekubi.transform.localEulerAngles;
        Vector3 counterTekubiRot = defaultTekubiRot + tekubiHandMagazineRotation;
        
        Sequence seq = DOTween.Sequence();

        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(tiltAngle, tiltTime));
        seq.Join(gun.MoveArm.transform.DOLocalMove(ArmMagazinePoint, tiltTime));
        seq.Join(gun.MoveTekubi.transform.DOLocalRotate(counterTekubiRot, tiltTime));

        bool isTrackingMagazine = false;
        Vector3 localOffset = Vector3.zero;

        seq.AppendCallback(() => {
            gun.PlayReloadSound(magOutClip);    
            gun.OnMagazineEjected();
            
            Vector3 armLocal = gun.transform.InverseTransformPoint(gun.MoveArm.transform.position);
            Vector3 magLocal = gun.transform.InverseTransformPoint(gun.magazineObject.transform.position);
            localOffset = armLocal - magLocal;
            isTrackingMagazine = true;
        });

        seq.Append(gun.magazineObject.transform.DOLocalMove(dropDistance, dropTime).SetEase(Ease.InBack, 1f));
        seq.Append(gun.magazineObject.transform.DOLocalMove(spawnPoint, dropToInsertTime));
        seq.Append(gun.magazineObject.transform.DOLocalMove(gun.magazinePoint.localPosition, insertTime).SetEase(Ease.OutBack, 0.75f));
        
        seq.OnUpdate(() => {
            if (isTrackingMagazine)
            {
                Vector3 currentMagLocal = gun.transform.InverseTransformPoint(gun.magazineObject.transform.position);
                Vector3 targetArmLocal = currentMagLocal + localOffset;
                gun.MoveArm.transform.position = gun.transform.TransformPoint(targetArmLocal);
            }
        });

        float insertStartTime = tiltTime + dropTime + dropToInsertTime;
        float magInsertTiming = insertStartTime + insertTime * 0.7f;
        float magHitTiming = insertStartTime + insertTime * 0.9f;

        seq.InsertCallback(magInsertTiming, () => gun.PlayReloadSound(magInsertClip));
        seq.InsertCallback(magHitTiming, () => gun.PlayReloadSound(magHitClip));

        seq.AppendCallback(() => {
            isTrackingMagazine = false;
            gun.StartReloadCountAnimation(tiltTime);
        });

        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(defaultGunRot, tiltTime));
        seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot, tiltTime));
        seq.Join(gun.MoveArm.transform.DOLocalMove(defaultArmPosition, tiltTime));
        
        if(isEmptyReload)
        {
            seq.Append(gun.MoveArm.transform.DOLocalMove(boltHandPosition, hand_boltMoveTime).SetEase(Ease.OutCubic));
            seq.Join(gun.MoveTekubi.transform.DOLocalRotate(tekubiBoltRotation, hand_boltMoveTime).SetEase(Ease.OutCubic));
            seq.InsertCallback(seq.Duration() - 0.2f, () => { 
                gun.PlayReloadSound(boltClip); 
            });
            seq.Append(gun.MoveArm.transform.DOLocalMove(boltHandPosition2, boltMoveTime).SetEase(Ease.OutQuad));
            
            seq.Append(gun.SpecialParts[0].transform.DOLocalMove(boltMoveDistance, releaseTime).SetEase(Ease.InQuad));
            
            seq.Append(gun.MoveArm.transform.DOLocalMove(defaultArmPosition, hand_boltMoveTime).SetEase(Ease.OutCubic));
            seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot, hand_boltMoveTime).SetEase(Ease.OutCubic));
        }

        seq.OnComplete(() => {
            gun.StopReloadCountAnimation();
            gun.OnReloadComplete();
        });
    }
}