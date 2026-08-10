using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PassiveUIManager : MonoBehaviour
{
    public PlayerManager pManager;
    public GameObject passiveItemPrefab;
    public Transform container;

    private Dictionary<string, (PassiveItemUI ui, int count, PassiveEffect representative)> uiItems 
        = new Dictionary<string, (PassiveItemUI ui, int count, PassiveEffect representative)>();

    private float timer = 0f;
void Update()
{
        foreach (var item in uiItems.Values)
        {
            item.ui.UpdateDisplay(item.count, item.representative);
        }
    
}

    public void RefreshUI()
    {
        foreach (Transform child in container) Destroy(child.gameObject);
        uiItems.Clear();

        var groupedPassives = pManager.activePassives.GroupBy(p => p.passiveName);

        foreach (var group in groupedPassives)
        {
            var representative = group.First();
            int count = group.Count();
            
            GameObject obj = Instantiate(passiveItemPrefab, container);
            PassiveItemUI ui = obj.GetComponent<PassiveItemUI>();
            ui.Setup(representative);
            
            uiItems.Add(group.Key, (ui, count, representative));
        }
    }
}