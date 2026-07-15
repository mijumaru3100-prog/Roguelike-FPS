using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[System.Serializable]
public class PassiveShopEntry
{

    public PassiveEffect effect; 
    public List<PassiveEffect> additionalEffects = new List<PassiveEffect>(); 
    public PlayerStats statBuff; 
}
[CreateAssetMenu(fileName = "PassiveDatabase", menuName = "Shop/PassiveDatabase")]
public class PassiveDatabase : ScriptableObject
{
    public List<PassiveShopEntry> passives = new List<PassiveShopEntry>();
    public PassiveShopEntry GetRandom(PlayerManager manager)
    {
        var candidates = passives.Where(p => 
            p.effect != null && (p.effect.canStack || !manager.activePassives.Contains(p.effect))
        ).ToList();
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }
}