using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[System.Serializable]
public class WeaponShopEntry
{
    public string weaponName;
    public int price;
    
    public string Damage;
    public GameObject model;
    public string detailtext;
    public GameObject weaponPrefab; 
}
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Shop/WeaponDatabase")]
public class WeaponDatabase : ScriptableObject
{
    public List<WeaponShopEntry> weapons = new List<WeaponShopEntry>();
public WeaponShopEntry GetRandom(PlayerManager manager)
{
    Debug.Log($"[WeaponDatabase] weapons null: {weapons == null}");
Debug.Log($"[WeaponDatabase] weapons count: {weapons?.Count ?? -1}");

foreach (var w in weapons)
{
    Debug.Log(
        $"[WeaponDatabase] entry: {w?.weaponName}, " +
        $"prefab: {w?.weaponPrefab}, model: {w?.model}"
    );
}
    // 武器データが登録されていない場合
    if (weapons == null || weapons.Count == 0)
    {
        Debug.LogError("WeaponDatabase: weapons に武器が登録されていません。");
        return null;
    }

    var candidates = weapons
    .Where(w =>
        w != null &&
        w.weaponPrefab != null &&
        w.model != null
    )
    .ToList();

    // null のデータしかない場合
    if (candidates.Count == 0)
    {
        Debug.LogError("WeaponDatabase: 有効な WeaponShopEntry がありません。");
        return null;
    }

    return candidates[Random.Range(0, candidates.Count)];
}

    public WeaponShopEntry GetEntryByPrefab(GameObject obj)
{
    if (obj == null) return null;
    return weapons.FirstOrDefault(w => 
        w.weaponPrefab == obj || 
        obj.name.StartsWith(w.weaponPrefab.name)
    );
}
}