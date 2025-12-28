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
    public Stat DriftChargeRate;
    public Stat DriftSegmentCount;

    public PlayerStatsRuntime(PlayerBaseStats baseStats)
    {
        MaxHealth = new Stat(baseStats.maxHealth);
        MaxSpeed = new Stat(baseStats.maxSpeed);
        AttackModeDuration = new Stat(baseStats.attackModeDuration);
        PickupRange = new Stat(baseStats.pickupRange);
        Damage = new Stat(baseStats.damage);
        DriftChargeRate = new Stat(baseStats.driftChargeRate);
        DriftSegmentCount = new Stat(baseStats.driftSegmentCount);
    }
}
