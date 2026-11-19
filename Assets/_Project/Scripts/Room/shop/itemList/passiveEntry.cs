using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[CreateAssetMenu(fileName = "PassiveDatabase", menuName = "Shop/PassiveDatabase")]
public class PassiveDatabase : ScriptableObject
{
    public List<PassiveEffect> passives = new List<PassiveEffect>();
    public  PassiveEffect GetRandom(PlayerManager manager)
    {
        var candidates = passives.Where(p => 
            (p.canStack || !manager.activePassives.Contains(p))
        ).ToList();
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }
}