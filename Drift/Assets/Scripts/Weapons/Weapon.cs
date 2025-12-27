using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public WeaponData data;

    protected float damage;
    protected float cooldown;

    protected int level = 1;
    protected float cooldownTimer;

    protected virtual void Awake()
    {
        damage = data.baseDamage;
        cooldown = data.baseCooldown;
    }

    protected virtual void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            Fire();
            cooldownTimer = cooldown;
        }
    }

    public void LevelUp()
    {
        level++;
        ApplyLevelUp(level);
    }

    protected abstract void ApplyLevelUp(int level);
    protected abstract void Fire();
}
