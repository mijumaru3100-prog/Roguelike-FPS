using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/recoilAnimation/SimpleRecoilAnimation")]
public  class simpleRecoilAnimation : recoilAnimation 
{
// simpleRecoilAnimation.cs の中
// GunBaseのfireから呼び出される、よ
public override void Play(GunBase gun)
{
   gun.recoilPivot.DOKill();
    // 二つの動きを束ねる「シーケンス」を作るんだ、よ
    Sequence s = DOTween.Sequence();
    
    float flip =1f;
    if (Random.value > 0.5f)
    {
        flip =-1f;
    }
    Vector3 PosAmount = new Vector3(
    Mathf.Clamp(gun.recoilPivot.localPosition.x + gun.posRecoil.x * flip, -gun.MaxPosRecoil.x, gun.MaxPosRecoil.x),
    Mathf.Clamp(gun.recoilPivot.localPosition.y + gun.posRecoil.y , -gun.MaxPosRecoil.y, gun.MaxPosRecoil.y),
    Mathf.Clamp(gun.recoilPivot.localPosition.z + -gun.posRecoil.z , -gun.MaxPosRecoil.z, gun.MaxPosRecoil.z)
);

Vector3 RotAmount = new Vector3(
    Mathf.Clamp(gun.recoilPivot.localEulerAngles.x + gun.rotRecoil.x * flip, -gun.MaxrotRecoil.x, gun.MaxrotRecoil.x),
    Mathf.Clamp(gun.recoilPivot.localEulerAngles.y + gun.rotRecoil.y * flip, -gun.MaxrotRecoil.y, gun.MaxrotRecoil.y),
    Mathf.Clamp(gun.recoilPivot.localEulerAngles.z + gun.rotRecoil.z * flip, -gun.MaxrotRecoil.z, gun.MaxrotRecoil.z)
);
    
    // 位置と回転、両方を同時にスタートさせる
    s.Join(gun.recoilPivot.DOLocalMove(PosAmount, gun.animDuration).SetEase(Ease.OutQuad));
    s.Join(gun.recoilPivot.DOLocalRotate(RotAmount, gun.animDuration).SetEase(Ease.OutQuad));

    // 戻る動きも、セットにしてあげようね
    s.Append(gun.recoilPivot.DOLocalMove(Vector3.zero, gun.animDuration * 2f));
    s.Join(gun.recoilPivot.DOLocalRotate(Vector3.zero, gun.animDuration * 2f));
}
}