using UnityEngine;
using DG.Tweening;
[CreateAssetMenu(menuName = "Gun/PartAction/Move")]
public class PartsMove : GunPartAction
{
    public Vector3 targetPos;       
    public Vector3 targetRot;       
    public float duration = 0.05f;  
    public float returnTime = 0.1f; 
    public Ease easeType = Ease.OutQuad;
    public bool isFinalShotCancel = false;
    public override void Execute(GunBase gun, GameObject part, Vector3 defaultPos, Vector3 defaultRot)
    {
        if (part == null) return;
        part.transform.DOKill();
        if (isFinalShotCancel && gun.currentAmmo == 0)
        {
            if (targetPos != Vector3.zero)
            {
                part.transform.DOLocalMove(defaultPos + targetPos, duration).SetEase(easeType);
            }
            if (targetRot != Vector3.zero)
            {
                part.transform.DOLocalRotate(defaultRot + targetRot, duration).SetEase(easeType);
            }
        }
        else
        {
            if (targetPos != Vector3.zero)
                part.transform.DOLocalMove(defaultPos + targetPos, duration).SetEase(easeType)
                .OnComplete(() => part.transform.DOLocalMove(defaultPos, returnTime));
            if (targetRot != Vector3.zero)
                part.transform.DOLocalRotate(defaultRot + targetRot, duration).SetEase(easeType)
                .OnComplete(() => part.transform.DOLocalRotate(defaultRot, returnTime));
        }
    }
}