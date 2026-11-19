using UnityEngine;

/// <summary>
/// 武器に付与されるアビリティの基底クラス
/// 各アビリティはこれを継承して固有の効果を実装する。
/// </summary>
public abstract class WeaponAbility : MonoBehaviour
{
    [Header("UI設定")]
    public string abilityName = "アビリティ";
    [TextArea] public string description = "アビリティ効果の説明";
    public Sprite icon;
    public Color ActiveColor = Color.white;
    public Color InactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    /// <summary>射撃時に呼ばれるアビリティ効果</summary>
    public virtual void SpecialShot(GunBase baseGun)
    {
        baseGun.shotAction.shot(baseGun);
    }

    /// <summary>リロード完了時に呼ばれる</summary>
    public virtual void OnCompleteReload() { }

    /// <summary>被ダメージ時に呼ばれる</summary>
    public virtual void OnDamaged() { }
}
