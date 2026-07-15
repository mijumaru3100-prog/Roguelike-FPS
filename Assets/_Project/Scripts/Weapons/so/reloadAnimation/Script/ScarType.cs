using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/reloadAnimation/SCARType")]
public class SCARTypeReload : reloadAnimation
{
    [Header("基準となる時間（1.0倍の時）")]
    public float baseTiltTime = 0.5f;
    public float baseDropTime = 0.4f;
    public float baseInsertTime = 0.5f;
    public float baseDropToInsertTime = 0.1f; 
    public float baseBoltMoveTime = 0.5f;
    public float baseHand_boltMoveTime = 0.5f;

    [Header("リロードモーション設定")]
    public Vector3 tiltAngle;
    public Vector3 dropDistance;
    public Vector3 spawnPoint; 
    public float shakeStrength;

    public Vector3 handMagazinePoint;
    public Vector3 tekubiHandMagazineRotation;

    [Header("ボルトを引くモーション設定")]
    public Vector3 boltHandPosition;
    public Vector3 boltHandPosition2;
    public Vector3 boltMoveDistance;
    public Vector3 tekubiBoltRotation;

    public float releaseTime = 0.05f; 
        
    public AudioClip magOutClip;    
    public AudioClip magInsertClip; 
    public AudioClip magHitClip;    
    public AudioClip boltClip;      

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

        // 1. 銃を傾け、手をマガジンへ
        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(tiltAngle, tiltTime));
        seq.Join(gun.MoveArm.transform.DOLocalMove(handMagazinePoint, tiltTime));
        seq.Join(gun.MoveTekubi.transform.DOLocalRotate(counterTekubiRot, tiltTime));

        // 手のマガジン追従用フラグとオフセット
        bool isTrackingMagazine = false;
        Vector3 localOffset = Vector3.zero;

        // 2. マガジンを抜く処理
        seq.AppendCallback(() => {
            gun.PlayReloadSound(magOutClip);    
            gun.OnMagazineEjected();
            
            // 追従を開始し、その時点のローカルオフセットを計算
            Vector3 armLocal = gun.transform.InverseTransformPoint(gun.MoveArm.transform.position);
            Vector3 magLocal = gun.transform.InverseTransformPoint(gun.magazineObject.transform.position);
            localOffset = armLocal - magLocal;
            isTrackingMagazine = true;
        });

        // 3~4. マガジン移動（手はUpdateで追従するため、シーケンスにはマガジンの動きだけを登録）
        seq.Append(gun.magazineObject.transform.DOLocalMove(dropDistance, dropTime).SetEase(Ease.InBack, 1f));
        seq.Append(gun.magazineObject.transform.DOLocalMove(spawnPoint, dropToInsertTime));
        seq.Append(gun.magazineObject.transform.DOLocalMove(gun.magazinePoint.localPosition, insertTime).SetEase(Ease.OutBack, 0.75f));
        
        // シーケンスの再生中、毎フレーム手をマガジンに同期させる
        seq.OnUpdate(() => {
            if (isTrackingMagazine)
            {
                Vector3 currentMagLocal = gun.transform.InverseTransformPoint(gun.magazineObject.transform.position);
                Vector3 targetArmLocal = currentMagLocal + localOffset;
                gun.MoveArm.transform.position = gun.transform.TransformPoint(targetArmLocal);
            }
        });

        // 音のタイミング
        float insertStartTime = tiltTime + dropTime + dropToInsertTime;
        float magInsertTiming = insertStartTime + insertTime * 0.7f;
        float magHitTiming = insertStartTime + insertTime * 0.9f;

        seq.InsertCallback(magInsertTiming, () => gun.PlayReloadSound(magInsertClip));
        seq.InsertCallback(magHitTiming, () => gun.PlayReloadSound(magHitClip));

        seq.AppendCallback(() => {
            // 追従を終了し、手を元の位置に戻す準備
            isTrackingMagazine = false;
            gun.StartReloadCountAnimation(tiltTime);
        });

        // 5. 銃の角度と手の位置を初期位置に戻す
        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(defaultGunRot, tiltTime));
        seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot, tiltTime));
        seq.Join(gun.MoveArm.transform.DOLocalMove(defaultArmPosition, tiltTime));
        
        if(isEmptyReload)
        {
            // --- 1. ボルトハンドルへ手を伸ばす ---
            seq.Append(gun.MoveArm.transform.DOLocalMove(boltHandPosition, hand_boltMoveTime).SetEase(Ease.OutCubic));
            seq.Join(gun.MoveTekubi.transform.DOLocalRotate(tekubiBoltRotation, hand_boltMoveTime).SetEase(Ease.OutCubic));
            seq.InsertCallback(seq.Duration() - 0.2f, () => { 
                gun.PlayReloadSound(boltClip); 
            });
            // --- 2. ボルトを引く ---
            seq.Append(gun.MoveArm.transform.DOLocalMove(boltHandPosition2, boltMoveTime).SetEase(Ease.OutQuad));
            
            // --- 3. ボルトを離す ---
            seq.Append(gun.SpecialParts[0].transform.DOLocalMove(boltMoveDistance, releaseTime).SetEase(Ease.InQuad));
            
            // 手を初期位置に戻す
            seq.Append(gun.MoveArm.transform.DOLocalMove(defaultArmPosition, hand_boltMoveTime).SetEase(Ease.OutCubic));
            seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot, hand_boltMoveTime).SetEase(Ease.OutCubic));
        }

        seq.OnComplete(() => {
            gun.StopReloadCountAnimation();
            gun.OnReloadComplete();
        });
    }
}