using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsRuntime
{
    public Stat MaxHealth;
    public Stat MaxSpeed;
    public Stat AttackModeDuration;
    public Stat PickupRange;
    public Stat Damage;
    public Stat DamageReduction;
    public Stat DriftChargeRate;
    public Stat DriftSegmentCount;

    public PlayerStatsRuntime(PlayerBaseStats baseStats, PlayerMetaProgressionStats playerMeta)
    {
        MaxHealth = new Stat(baseStats.maxHealth + playerMeta.BonusMaxHealth);
        MaxSpeed = new Stat(baseStats.maxSpeed);
        AttackModeDuration = new Stat(baseStats.attackModeDuration);
        PickupRange = new Stat(baseStats.pickupRange);
        Damage = new Stat(baseStats.damage * playerMeta.DamageMultiplier);
        DamageReduction = new Stat(baseStats.damageReduction + playerMeta.DamageReduction);
        DriftChargeRate = new Stat(baseStats.driftChargeRate);
        DriftSegmentCount = new Stat(baseStats.driftSegmentCount);
    }
}
