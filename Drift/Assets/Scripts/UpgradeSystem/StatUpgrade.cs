using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    MaxHealth,
    MaxSpeed,
    AttackModeDuration,
    GearPickupRange
}

public enum ModifierType
{ 
    Additive,
    Multiplicative
}

[CreateAssetMenu(menuName = "Upgrades/Stat Upgrade")]
public class StatUpgrade : ScriptableObject
{
    public string upgradeName;
    public Sprite icon;
    public string upgradeDescription;

    public StatType stat;
    public ModifierType modifierType;
    public float value;
}
