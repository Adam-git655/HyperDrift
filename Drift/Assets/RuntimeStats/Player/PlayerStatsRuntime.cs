using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsRuntime
{
    public Stat MaxHealth;
    public Stat HealthRegen;
    public Stat MaxSpeed;
    public Stat AttackModeDuration;
    public Stat GearPickupRange;

    public PlayerStatsRuntime(PlayerBaseStats baseStats)
    {
        MaxHealth = new Stat(baseStats.maxHealth);
        HealthRegen = new Stat(baseStats.healthRegen);
        MaxSpeed = new Stat(baseStats.maxSpeed);
        AttackModeDuration = new Stat(baseStats.attackModeDuration);
        GearPickupRange = new Stat(baseStats.gearPickupRange);
    }
}
