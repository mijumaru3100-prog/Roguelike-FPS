using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Gun/PartAction/AlphaChange")]
public class AlphaChange : GunPartAction
{
    public override void Execute(GunBase gun, GameObject part, Vector3 defaultPos, Vector3 defaultRot)
    {
        bool isEmpty = gun.currentAmmo <= 0;
        float ChargeTime = gun.fireRate;
        
        Renderer rend = part.GetComponent<Renderer>();
        Material mat = rend.material;

        mat.DOKill();

        mat.EnableKeyword("_EMISSION");
        Color baseColor = mat.color;

        if(isEmpty)
        {
            mat.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            
            mat.SetColor("_EmissionColor", Color.black);
        }
        else
        {
            var ps = part.GetComponent<ParticleSystem>();
            PerticleRunner.Play(ps, ChargeTime*0.9f);

            mat.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            mat.DOColor(new Color(baseColor.r, baseColor.g, baseColor.b, 1f),ChargeTime).SetEase(Ease.InCubic);

            mat.SetColor("_EmissionColor", Color.black);
            
            Color targetEmission = new Color(baseColor.r, baseColor.g, baseColor.b) * 1024f;
            mat.DOColor(targetEmission, "_EmissionColor", ChargeTime).SetEase(Ease.InCubic);

            DOVirtual.DelayedCall(ChargeTime, () => 
            {
                if (gun != null)
                {
                    gun.PlayMuzzleFlash();
                }
            }); 
        }
    }
}