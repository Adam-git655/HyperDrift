using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{ 
    FireTrail,
    Grinders,
    EMPPulse
}


[CreateAssetMenu(menuName = ("Upgrades/Weapon Upgrade"))]
public class WeaponUpgrade : Upgrade
{
    public WeaponType weaponType;
    public GameObject weaponPrefab;

    //When a Weapon upgrade is selected this adds/upgrades the weapon to the car
    public override void Apply(Car player)
    {
        player.weaponController.ApplyWeaponUpgrade(weaponType, weaponPrefab);
    }
}
