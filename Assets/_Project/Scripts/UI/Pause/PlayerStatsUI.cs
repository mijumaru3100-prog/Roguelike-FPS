using UnityEngine;
using TMPro;

public class PlayerStatsDetailUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PlayerStats playerStats;

    [Header("表示先")]
    [SerializeField] private TextMeshProUGUI statsText;

    [Header("色")]
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    public void Refresh()
    {
        if (playerStats == null || statsText == null)
            return;

        string text = "";

        AddLine(ref text, "最大HP", playerStats.bonusMaxHP, false);
        AddLine(ref text, "最大HP", playerStats.maxHPMultiple, true);
        AddLine(ref text, "最大弾数", playerStats.bonusMaxAmmo, false);
        AddLine(ref text, "最大弾数", playerStats.maxAmmoMultiple, true);
        AddLine(ref text, "ダメージ", playerStats.bonusDamage, false);
        AddLine(ref text, "ダメージ", playerStats.damageMultiple, true);
        AddLine(ref text, "弱点ダメージ", playerStats.WeakPointBonus, true);
        AddLine(ref text, "連射速度", playerStats.fireRateMultiple, true);
        AddLine(ref text, "リロード速度", playerStats.reloadSpeedMultiple, true);
        AddLine(ref text, "移動速度", playerStats.MoveSpeedMultiple, true);
        AddLine(ref text, "ジャンプ力", playerStats.JumpHeightMultiple, true);






        // バフが1つもない
        if (string.IsNullOrEmpty(text))
        {
            statsText.text = "なし";
        }
        else
        {
            statsText.text = text.TrimEnd('\n');
        }
    }

    private void AddLine(
        ref string text,
        string label,
        float value,
        bool isPercentage)
    {
        // 0なら表示しない
        if (Mathf.Approximately(value, 0f))
            return;

        string valueText;

        if (isPercentage)
        {
            valueText = $"{value * 100f:+0;-0;0}%";
        }
        else
        {
            valueText = $"{value:+0;-0;0}";
        }

        // 正なら緑、負なら赤
        string colorHex = ColorUtility.ToHtmlStringRGB(
            value > 0 ? positiveColor : negativeColor
        );

        valueText = $"<color=#{colorHex}>{valueText}</color>";

        text += $"{label}  {valueText}\n";
    }
}