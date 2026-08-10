using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/reloadAnimation/DualRevolver")]
public class DualRevolver : reloadAnimation
{
    [Header("基準となる時間（1.0倍の時）")]
    public float baseTiltTime = 0.4f;
    public float baseEjectTime = 0.4f;
    public float baseHandOverTime = 0.3f;
    public float baseGrabLoaderTime = 0.3f;
    public float baseInsertTime1 = 0.5f;
    public float baseInsertTime2 = 0.5f;
    public float baseCloseTime = 0.4f;

    [Header("アニメーションパラメータ")]
    public Vector3 reloadTiltAngle = new Vector3(15, -10, 0);
    public Vector3 ejectTiltAngle = new Vector3(-30, 0, 0);
    
    [Header("シリンダー設定 (SpecialPartsのインデックス)")]
    public int leftCylinderIndex = 0;
    public int rightCylinderIndex = 1;
    public Vector3 cylinderOpenPosition = new Vector3(0, 0, 0);

    [Header("左銃の預け先設定 (SpecialPartsのインデックス)")]
    public int leftGunIndex = 2;
    public Vector3 handOverLeftGunPos = new Vector3(0.1f, -0.05f, 0.1f);
    public Vector3 handOverLeftGunRot = new Vector3(10, 20, -10);

    [Header("マガジン/スピードローダー設定")]
    public Vector3 speedloaderSpawnPoint;
    public Vector3 leftCylinderReloadPoint;
    public Vector3 rightCylinderReloadPoint;
    
    [Header("手・手首の設定")]
    public Vector3 leftInsertHandAngle = new Vector3(0, 30, 0);
    public Vector3 rightInsertHandAngle = new Vector3(0, -30, 0);

    [Header("効果音")]
    public AudioClip cylinderOpenClip;
    public AudioClip shellEjectClip;
    public AudioClip speedloaderInsertClip;
    public AudioClip cylinderCloseClip;
    public AudioClip grabLoaderClip;

    public override void Play(GunBase gun)
    {
        float speedMult = gun.GetTotalReloadSpeedMultiplier();

        float tiltTime = baseTiltTime / speedMult;
        float ejectTime = baseEjectTime / speedMult;
        float handOverTime = baseHandOverTime / speedMult;
        float grabLoaderTime = baseGrabLoaderTime / speedMult;
        float insertTime1 = baseInsertTime1 / speedMult;
        float insertTime2 = baseInsertTime2 / speedMult;
        float closeTime = baseCloseTime / speedMult;

        Vector3 defaultGunRot = gun.gunTiltModel.transform.localEulerAngles;
        Vector3 defaultArmPosition = gun.MoveArm.transform.localPosition;
        Vector3 defaultTekubiRot = gun.MoveTekubi.transform.localEulerAngles;
        Vector3 defaultLoaderPos = gun.magazineObject != null ? gun.magazineObject.transform.localPosition : Vector3.zero;

        Transform leftCylinder = null;
        Transform rightCylinder = null;
        Transform leftGun = null;

        if (gun.SpecialParts.Count > leftCylinderIndex && gun.SpecialParts[leftCylinderIndex] != null)
        {
            leftCylinder = gun.SpecialParts[leftCylinderIndex].transform;
        }
        if (gun.SpecialParts.Count > rightCylinderIndex && gun.SpecialParts[rightCylinderIndex] != null)
        {
            rightCylinder = gun.SpecialParts[rightCylinderIndex].transform;
        }
        if (gun.SpecialParts.Count > leftGunIndex && gun.SpecialParts[leftGunIndex] != null)
        {
            leftGun = gun.SpecialParts[leftGunIndex].transform;
        }

        Vector3 defaultLeftCylinderPos = leftCylinder != null ? leftCylinder.localPosition : Vector3.zero;
        Vector3 defaultRightCylinderPos = rightCylinder != null ? rightCylinder.localPosition : Vector3.zero;
        Vector3 defaultLeftGunPos = leftGun != null ? leftGun.localPosition : Vector3.zero;
        Vector3 defaultLeftGunRot = leftGun != null ? leftGun.localEulerAngles : Vector3.zero;

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() => {
            if (cylinderOpenClip != null) gun.PlayReloadSound(cylinderOpenClip);
        });
        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(reloadTiltAngle, tiltTime));
        if (leftCylinder != null)
        {
            seq.Join(leftCylinder.DOLocalMove(cylinderOpenPosition, tiltTime));
        }
        if (rightCylinder != null)
        {
            seq.Join(rightCylinder.DOLocalMove(cylinderOpenPosition, tiltTime));
        }

        seq.AppendCallback(() => {
            if (shellEjectClip != null) gun.PlayReloadSound(shellEjectClip);
            gun.OnMagazineEjected(); 
        });
        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(ejectTiltAngle, ejectTime).SetEase(Ease.OutQuad));
        seq.Append(gun.gunTiltModel.transform.DOShakePosition(ejectTime * 0.5f, 0.05f, 10));

        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(reloadTiltAngle, handOverTime));
        if (leftGun != null)
        {
            seq.Join(leftGun.DOLocalMove(handOverLeftGunPos, handOverTime).SetEase(Ease.OutCubic));
            seq.Join(leftGun.DOLocalRotate(handOverLeftGunRot, handOverTime).SetEase(Ease.OutCubic));
        }
        seq.Join(gun.MoveArm.transform.DOLocalMove(handOverLeftGunPos, handOverTime).SetEase(Ease.OutCubic));

        seq.Append(gun.MoveArm.transform.DOLocalMove(speedloaderSpawnPoint, grabLoaderTime).SetEase(Ease.OutCubic));
        seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot, grabLoaderTime));
        seq.AppendCallback(() => {
            if (grabLoaderClip != null) gun.PlayReloadSound(grabLoaderClip);
            if (gun.magazineObject != null)
            {
                gun.magazineObject.SetActive(true);
                gun.magazineObject.transform.localPosition = speedloaderSpawnPoint;
            }
        });

        if (gun.magazineObject != null)
        {
            seq.Append(gun.magazineObject.transform.DOLocalMove(leftCylinderReloadPoint, insertTime1).SetEase(Ease.OutBack));
            seq.Join(gun.MoveArm.transform.DOLocalMove(leftCylinderReloadPoint, insertTime1).SetEase(Ease.OutBack));
            seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot + leftInsertHandAngle, insertTime1));
        }
        seq.AppendCallback(() => {
            if (speedloaderInsertClip != null) gun.PlayReloadSound(speedloaderInsertClip);
            gun.StartReloadCountAnimation(insertTime1);
        });

        seq.Append(gun.MoveArm.transform.DOLocalMove(speedloaderSpawnPoint, grabLoaderTime).SetEase(Ease.OutCubic));
        seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot, grabLoaderTime));
        seq.AppendCallback(() => {
            if (grabLoaderClip != null) gun.PlayReloadSound(grabLoaderClip);
            if (gun.magazineObject != null)
            {
                gun.magazineObject.transform.localPosition = speedloaderSpawnPoint;
            }
        });

        if (gun.magazineObject != null)
        {
            seq.Append(gun.magazineObject.transform.DOLocalMove(rightCylinderReloadPoint, insertTime2).SetEase(Ease.OutBack));
            seq.Join(gun.MoveArm.transform.DOLocalMove(rightCylinderReloadPoint, insertTime2).SetEase(Ease.OutBack));
            seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot + rightInsertHandAngle, insertTime2));
        }
        seq.AppendCallback(() => {
            if (speedloaderInsertClip != null) gun.PlayReloadSound(speedloaderInsertClip);
            if (gun.magazineObject != null) gun.magazineObject.SetActive(false);
        });

        seq.AppendCallback(() => {
            if (cylinderCloseClip != null) gun.PlayReloadSound(cylinderCloseClip);
        });
        seq.Append(gun.gunTiltModel.transform.DOLocalRotate(defaultGunRot, closeTime));
        seq.Join(gun.MoveArm.transform.DOLocalMove(defaultArmPosition, closeTime));
        seq.Join(gun.MoveTekubi.transform.DOLocalRotate(defaultTekubiRot, closeTime));
        if (leftGun != null)
        {
            seq.Join(leftGun.DOLocalMove(defaultLeftGunPos,closeTime).SetEase(Ease.OutCubic));
            seq.Join(leftGun.DOLocalRotate(defaultLeftGunRot, closeTime).SetEase(Ease.OutCubic));
        }
        
        if (leftCylinder != null)
        {
            seq.Join(leftCylinder.DOLocalMove(defaultLeftCylinderPos, closeTime));
        }
        if (rightCylinder != null)
        {
            seq.Join(rightCylinder.DOLocalMove(defaultRightCylinderPos, closeTime));
        }

        seq.OnComplete(() => {
            gun.StopReloadCountAnimation();
            if (gun.magazineObject != null)
            {
                gun.magazineObject.transform.localPosition = defaultLoaderPos;
                gun.magazineObject.SetActive(true);
            }
            gun.OnReloadComplete();
        });
    }
}
