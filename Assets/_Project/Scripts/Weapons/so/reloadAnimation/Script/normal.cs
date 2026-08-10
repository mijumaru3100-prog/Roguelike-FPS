using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/reloadAnimation/normal")]
public class normalReload : reloadAnimation
{
    [Header("基準となる時間（1.0倍の時）")]
    public float baseTiltTime = 0.5f;
    public float baseDropTime = 0.4f;
    public float baseInsertTime = 0.5f;
    public float baseDropToInsertTime = 0.1f; 

    [Header("リロードモーション設定")]
    public Vector3 tiltAngle;
    public Vector3 dropDistance;
    public Vector3 spawnPoint; 
    public float shakeStrength;

    public Vector3 handMagazinePoint;
    public Vector3 tekubiHandMagazineRotation;

    public AudioClip magOutClip;    
    public AudioClip magInsertClip; 
    public AudioClip magHitClip;    

    public override void Play(GunBase gun)
    {
        bool isEmptyReload = gun.currentAmmo == 0;
        float speedMult = gun.GetTotalReloadSpeedMultiplier();

        float tiltTime = baseTiltTime / speedMult;
        float dropTime = baseDropTime / speedMult;
        float insertTime = baseInsertTime / speedMult;
        float dropToInsertTime = baseDropToInsertTime / speedMult;

        Vector3 defaultGunRot = gun.gunTiltModel.transform.localEulerAngles;
        Vector3 defaultArmPosition = gun.MoveArm.transform.localPosition;
        Vector3 defaultTekubiRot = gun.MoveTekubi.transform.localEulerAngles;
        Vector3 counterTekubiRot = defaultTekubiRot + tekubiHandMagazineRotation;
        
        Sequence seq = DOTween.Sequence();

        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(tiltAngle, tiltTime));
        seq.Join(gun.MoveArm.transform.DOLocalMove(handMagazinePoint, tiltTime));
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
        
        seq.OnComplete(() => {
            gun.StopReloadCountAnimation();
            gun.OnReloadComplete();
        });
    }
}