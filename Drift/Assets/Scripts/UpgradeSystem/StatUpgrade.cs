using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    MaxHealth,
    MaxSpeed,
    AttackModeDuration,
    PickupRange,
    DriftChargeRate
}

public enum ModifierType
{ 
    Additive,
    Multiplicative
}

[CreateAssetMenu(menuName = "Upgrades/Stat Upgrade")]
public class StatUpgrade : Upgrade
{
    public StatType stat;
    public ModifierType modifierType;
    public float value;

    //When a Player Stat upgrade is selected this applies that upgrade to the corresponding stat in the player(car)
    public override void Apply(Car player)
    {
        //get the player stat which we are about to upgrade 
        Stat targetStat = stat switch
        {
            StatType.MaxHealth => player.stats.MaxHealth,
            StatType.MaxSpeed => player.stats.MaxSpeed,
            StatType.AttackModeDuration => player.stats.AttackModeDuration,
            StatType.PickupRange => player.stats.PickupRange,
            StatType.DriftChargeRate => player.stats.DriftChargeRate,
            _ => null
        };

        if (targetStat == null) return;

        if (modifierType == ModifierType.Additive)
            targetStat.Add(value);
        else if (modifierType == ModifierType.Multiplicative)
            targetStat.Multiply(value);
    }
}
