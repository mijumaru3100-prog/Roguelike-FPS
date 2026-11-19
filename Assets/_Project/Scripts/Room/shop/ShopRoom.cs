using UnityEngine;
using System.Collections.Generic;

public class ShopRoom : Stage
{
    [Header("ショップ")]
    [SerializeField]private List<StatsShop> StatsShops = new List<StatsShop>();
    [SerializeField]private List<weaponshop> weaponshops = new List<weaponshop>();
    [SerializeField]private List<passiveshop> passiveshops = new List<passiveshop>();
    [SerializeField]private List<AbilityShop> abilityShops = new List<AbilityShop>();
    void Start()
    {
        ResetShops();
    }

    public override void ResetStage()
    {
        ResetShops();
    }

    public void ResetShops()
    {
        base.ResetStage();
        base.StartStage();

        foreach(var shop in StatsShops)
        {
            if(shop != null) shop.ShopSetting();
        }

        foreach(var shop in weaponshops)
        {
            if(shop != null) shop.Set();
        }

        foreach(var shop in  passiveshops)
        {
            if(shop != null) shop.ShopSetting();
        }

        foreach(var shop in abilityShops)
        {
            if(shop != null) shop.Set();
        }
    }
}
