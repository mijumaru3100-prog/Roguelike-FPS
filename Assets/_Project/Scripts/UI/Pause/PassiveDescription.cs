using UnityEngine;
using TMPro;

public class PassiveDescriptionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI passiveNameText;
    [SerializeField] private TextMeshProUGUI passiveDetailText;

    [SerializeField] private string passiveName = "PassiveDetail";
    [SerializeField] private string passiveDetail =
        "パッシブアイテムにカーソルを合わせると詳細を表示";

    private void Start()
    {
        ShowDefault();
    }

    public void ShowPassive(PassiveEffect passive)
    {
        if (passive == null)
        {
            ShowDefault();
            return;
        }

        passiveNameText.text = passive.passiveName;
        passiveDetailText.text = passive.detailtext;
    }

    public void ShowDefault()
    {
        passiveNameText.text = passiveName;
        passiveDetailText.text = passiveDetail;
    }
}