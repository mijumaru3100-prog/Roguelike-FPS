using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 

public class CoinDropEffectManager : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private TextMeshProUGUI moneyTextUI;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform boxPoint;
    
    [SerializeField] private Transform[] pathPoints; 

    private int displayedMoney = 0;

    private void Start()
    {
        //displayedMoney = playerManager.money;
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
            int coinCount = Mathf.Min(newMoney - oldMoney, 10); 
            StartCoroutine(SpawnCoinsCoroutine(coinCount, newMoney));
        }
        else
        {
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

            DOTween.To(() => displayedMoney, x => displayedMoney = x, targetMoney, 0.2f)
                .OnUpdate(UpdateMoneyText);

            moneyTextUI.transform.DOComplete();
            moneyTextUI.transform.DOPunchPosition(new Vector3(0, -10, 0), 0.1f, 1, 0.5f);
        });

        yield return new WaitForSeconds(0.08f);
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