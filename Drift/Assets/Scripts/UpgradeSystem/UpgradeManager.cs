using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private Car player;
    public List<Upgrade> allUpgrades;

    private void Awake()
    {
        //Singleton
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        player = FindObjectOfType<Car>();
    }

    //Get 'count'(3) random upgrades from the entire pool of all upgrades, which we will show in the UI after level up
    public List<Upgrade> GetRandomUpgrades(int count)
    {
        List<Upgrade> pool = new List<Upgrade>(allUpgrades);
        List<Upgrade> pickedUpgrades = new();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            pickedUpgrades.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return pickedUpgrades;
    }

    public void ApplyUpgrade(Upgrade upgrade)
    {
        upgrade.Apply(player);
    }
}
