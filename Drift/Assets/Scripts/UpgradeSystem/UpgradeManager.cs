using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private Car player;
    public List<StatUpgrade> allUpgrades;

    private void Awake()
    {
        //Singleton
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        player = FindObjectOfType<Car>();
    }

    //Get 3 random upgrades from the entire pool of all upgrades, which we will show in the UI after level up
    public List<StatUpgrade> GetRandomUpgrades(int count)
    {
        List<StatUpgrade> pool = new List<StatUpgrade>(allUpgrades);
        List<StatUpgrade> pickedUpgrades = new();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            pickedUpgrades.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return pickedUpgrades;
    }


    //When a Player Stat upgrade is selected this applies that upgrade to the corresponding stat in the player(car)
    public void ApplyPlayerUpgrade(StatUpgrade upgrade)
    {
        //get the player stat which we are about to upgrade 
        Stat stat = upgrade.stat switch
        {
            StatType.MaxHealth => player.stats.MaxHealth,
            StatType.MaxSpeed => player.stats.MaxSpeed,
            StatType.AttackModeDuration => player.stats.AttackModeDuration,
            StatType.GearPickupRange => player.stats.GearPickupRange,
            StatType.DriftChargeRate => player.stats.DriftChargeRate,
            _ => null
        };

        if (stat == null) return;

        if (upgrade.modifierType == ModifierType.Additive)
            stat.Add(upgrade.value);
        else if (upgrade.modifierType == ModifierType.Multiplicative)
            stat.Multiply(upgrade.value);
    }
}
