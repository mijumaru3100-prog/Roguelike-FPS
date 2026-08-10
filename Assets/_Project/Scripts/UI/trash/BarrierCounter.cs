using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class  BarrierCounterUI : MonoBehaviour
{
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private int count = 0;

    private List<GameObject> spawnedIcons = new List<GameObject>();

    void Start()
    {
        UpdateIcons();
    }

    public void ChangeAmount(int amount)
    {
        count += amount;
        UpdateIcons();
    }

    private void UpdateIcons()
    {
        foreach (var icon in spawnedIcons)
        {
            Destroy(icon);
        }
        spawnedIcons.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject newIcon = Instantiate(iconPrefab, transform);
            spawnedIcons.Add(newIcon);
        }
    }
}