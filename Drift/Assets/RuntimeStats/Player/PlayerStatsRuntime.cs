using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsRuntime
{
    public Stat MaxHealth;
    public Stat MaxSpeed;
    public Stat AttackModeDuration;
    public Stat GearPickupRange;
    public Stat Damage;

    public PlayerStatsRuntime(PlayerBaseStats baseStats)
    {
        MaxHealth = new Stat(baseStats.maxHealth);
        MaxSpeed = new Stat(baseStats.maxSpeed);
        AttackModeDuration = new Stat(baseStats.attackModeDuration);
        GearPickupRange = new Stat(baseStats.gearPickupRange);
        Damage = new Stat(baseStats.damage);
    }
}
