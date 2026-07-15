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
        Material mat = rend.material; // ※ここでマテリアルが複製されます

        // 実行前に、念のため現在動いているこのマテリアルへのTweenをすべてクリア
        mat.DOKill();

        mat.EnableKeyword("_EMISSION");
        Color baseColor = mat.color;

        if(isEmpty)
        {
            // 【弾がない時】
            // アルファを0にする（0秒切り替えなら直接代入が安全）
            mat.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            
            // Emissionを完全に「黒（消灯）」にする
            mat.SetColor("_EmissionColor", Color.black);
        }
        else
        {
            var ps = part.GetComponent<ParticleSystem>();
            PerticleRunner.Play(ps, ChargeTime*0.9f);

            // --- 1. アルファ（透明度）の制御 ---
            // 撃った瞬間は透明(0)
            mat.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            // fireRate かけて不透明(1)に戻す
            mat.DOColor(new Color(baseColor.r, baseColor.g, baseColor.b, 1f),ChargeTime).SetEase(Ease.InCubic);

            // --- 2. Emission（発光）の制御 【ここを修正】 ---
            // 撃った瞬間は一度完全に消灯する（黒にする）
            mat.SetColor("_EmissionColor", Color.black);
            
            // 目標の輝度（強度10 = 1024倍）を計算
            Color targetEmission = new Color(baseColor.r, baseColor.g, baseColor.b) * 1024f;
            // アルファと同じ時間をかけて、じわーっと強度10まで発光させる
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