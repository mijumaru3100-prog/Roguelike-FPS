using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/reloadAnimation/DoubleBarrel")]
public class DoubleBarrel : reloadAnimation
{
    [Header("基準となる時間（1.0倍の時）")]
    public float baseOpenTime = 0.5f;
    public float baseDropTime = 0.4f;
    public float baseInsertTime = 0.5f;
    public float baseDropToInsertTime = 0.1f; 
    public float baseBoltMoveTime = 0.5f;

    [Header("リロードモーション設定")]
    public Vector3 OpenAngle;
    public Vector3 dropDistance;
    public Vector3 spawnPoint; 
    public float shakeStrength;

    public Vector3 handMagazinePoint;
    public Vector3 tekubiHandMagazineRotation;

     public Vector3 ReloadTekubiRot;

    public AudioClip magOpenClip; 
     public AudioClip magDropClip;    
    public AudioClip magInsertClip;         
    public AudioClip magCloseClip; 
    public float CloseSoundDelay = 0.1f;
   
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
        seq.Join(gun.MoveArm.transform.DOLocalMove(handMagazinePoint,dropToInsertTime));

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