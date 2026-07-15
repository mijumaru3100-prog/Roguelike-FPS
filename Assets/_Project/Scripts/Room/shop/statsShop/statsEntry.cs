using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[System.Serializable]
public class StatsShopEntry
{
    public string statName;
    public int defaultPrice;
    public Sprite icon;
    public GameObject model;
    public string detailtext;
    public PlayerStats stats;    
    public bool isApplying = true;

}
[CreateAssetMenu(fileName = "StatsDatabase", menuName = "Shop/StatsDatabase")]
public class StatsDatabase : ScriptableObject
{
    public List<StatsShopEntry> stats = new List<StatsShopEntry>();
    public StatsShopEntry GetRandom()
    {
        return stats[Random.Range(0, stats.Count)];
    }
}