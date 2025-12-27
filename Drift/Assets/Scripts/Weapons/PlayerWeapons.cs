using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    public int maxWeapons = 6;
    
    private Dictionary<WeaponType, Weapon> activeWeapons = new Dictionary<WeaponType, Weapon>();

    public IReadOnlyDictionary<WeaponType, Weapon> ActiveWeapons => activeWeapons;

    //If this is a new weapon, then instantiate its prefab.
    //If this weapon already exists, then just level it up.
    public void ApplyWeaponUpgrade(WeaponType type, GameObject weaponPrefab)
    {
        if (activeWeapons.TryGetValue(type, out Weapon weapon))
        {
            weapon.LevelUp();
        }
        else
        {
            AddWeapon(type, weaponPrefab);
        }
    }

    private void AddWeapon(WeaponType type, GameObject weaponPrefab)
    {
        if (activeWeapons.Count >= maxWeapons)
            return;

        Weapon weapon = Instantiate(weaponPrefab, transform).GetComponent<Weapon>();

        activeWeapons.Add(type, weapon);
    }
}
