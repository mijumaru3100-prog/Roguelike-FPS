using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 

public class CoinDropEffectManager : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private TextMeshProUGUI moneyTextUI;
    [SerializeField] private GameObject coinPrefab; // 2D UI(Image)のコインプレハブ
    [SerializeField] private Transform spawnPoint;  // コイン出現位置 (①)
    [SerializeField] private Transform boxPoint;    // 箱（カウンター）の位置 (③/④)
    
    // 坂道のカーブを制御するための制御点（直線なら不要、ベジェ曲線なら配置）
    [SerializeField] private Transform[] pathPoints; 

    private int displayedMoney = 0; // 画面上に表示されている現在の数値

    private void Start()
    {
        displayedMoney = playerManager.money;
        UpdateMoneyText();    
    }

    private void OnEnable()
    {
        if (playerManager != null) playerManager.OnMoneyChanged += HandleMoneyChanged;
    }

    private void OnDisable()
    {
        if (playerManager != null) playerManager.OnMoneyChanged -= HandleMoneyChanged;
    }

    private void HandleMoneyChanged(int oldMoney, int newMoney)
    {
        if (newMoney > oldMoney)
        {
            // お金が増えたら、差分の数だけコイン演出を開始（例: 最大10枚などに制限すると重くならない）
            int coinCount = Mathf.Min(newMoney - oldMoney, 10); 
            StartCoroutine(SpawnCoinsCoroutine(coinCount, newMoney));
        }
        else
        {
            // お金が減ったときは演出なしで即座にテキスト更新
            displayedMoney = newMoney;
            UpdateMoneyText();
        }
    }

    private IEnumerator SpawnCoinsCoroutine(int count, int targetMoney)
{
    for (int i = 0; i < count; i++)
    {
        GameObject coin = Instantiate(coinPrefab, spawnPoint.position, Quaternion.identity, this.transform);
        CoinMovement coinMovement = coin.GetComponent<CoinMovement>();
        
        coinMovement.StartMoving(pathPoints, boxPoint, () =>
        {
            // --- コインが箱に入った瞬間の処理 ---

            // ① カウンターの数値をDOTweenで滑らかに増加させる
            // 現在の表示金額から、ターゲットの金額まで0.2秒かけてアニメーション
            DOTween.To(() => displayedMoney, x => displayedMoney = x, targetMoney, 0.2f)
                .OnUpdate(UpdateMoneyText); // 数値が変わるたびにテキストを更新

            // ② 箱（カウンターUI）を一瞬だけピクッと揺らす（画像デザインポイントの再現）
            // 10ピクセル下に、0.1秒で、1回だけ弾むように揺らす
            moneyTextUI.transform.DOComplete(); // 連続で入ったときのために前の揺れをリセット
            moneyTextUI.transform.DOPunchPosition(new Vector3(0, -10, 0), 0.1f, 1, 0.5f);
        });

        // コインを連続で流す間隔
        yield return new WaitForSeconds(0.08f); // 少し短くすると密度が出て気持ちいいです
    }
}

    private void UpdateMoneyText()
    {
        if (moneyTextUI != null)
        {
            moneyTextUI.text = displayedMoney.ToString("000");
        }
    }
}