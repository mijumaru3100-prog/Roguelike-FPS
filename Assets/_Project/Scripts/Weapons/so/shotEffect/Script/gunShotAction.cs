using UnityEngine;
using DG.Tweening;
[CreateAssetMenu(menuName = "Gun/PartAction/Move")]
public class PartsMove : GunPartAction
{
    [Header("パーツ移動・回転設定")]
    [Tooltip("発砲時の移動量（ローカル）")] public Vector3 targetPos;       
    [Tooltip("発砲時の回転量（ローカル角度）")] public Vector3 targetRot;       
    [Tooltip("移動にかかる時間")] public float duration = 0.05f;  
    [Tooltip("元の位置に戻る時間")] public float returnTime = 0.1f; 
    [Tooltip("イージング種別")] public Ease easeType = Ease.OutQuad;
    [Tooltip("最後の1発を撃った時に位置を戻さないか（スライドオープン等）")] public bool isFinalShotCancel = false;
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