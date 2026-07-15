using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[System.Serializable]
public class WeaponShopEntry
{
    public string weaponName;
    public int price;
    public Sprite icon;
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
        var candidates = weapons.Where(w => 
            manager.currentWeapon == null || w.weaponPrefab != manager.currentWeapon.gameObject
        ).ToList();
        if (candidates.Count == 0)
        {
            return weapons[Random.Range(0, weapons.Count)];
        }
        return candidates[Random.Range(0, candidates.Count)];
    }
}