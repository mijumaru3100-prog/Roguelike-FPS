using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/PartAction/Rotation")]
public class PartsRotation : GunPartAction
{      
    public Vector3 targetRot;       
    public float duration = 0.05f;  
    public float returnTime = 0.1f; 
    public Ease easeType = Ease.OutQuad;

    [Header("周期設定")]
    [Tooltip("何回に1回発動させるか（例：3なら3回に1回）")]
    public int triggerInterval = 3; 

    private int callCount = 0;
    public int FirstcallCount = 0;

    private bool FirstCall = true;

    public override void Execute(GunBase gun, GameObject part, Vector3 defaultPos, Vector3 defaultRot)
    {
        if (part == null) return;

        if(FirstCall)
        {
            callCount = FirstcallCount;
        }

        callCount++;

        if (callCount < triggerInterval) return;

        callCount = 0;

        part.transform.DOKill();
        part.transform.DOLocalRotate(defaultRot + targetRot, duration).SetEase(easeType)
            .OnComplete(() => part.transform.DOLocalRotate(defaultRot, returnTime));
    }
}