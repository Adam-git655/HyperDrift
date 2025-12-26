using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrailWeapon : Weapon
{
    private Car car;
    private FireTrailWeaponData fireTrailWeaponData;

    private FireTrailDamage damageTrail;
    [SerializeField] private LayerMask enemyLayer;

    protected override void Awake()
    {
        base.Awake();
        car = GetComponentInParent<Car>();
        fireTrailWeaponData = (FireTrailWeaponData) data;
    }

    private void Start()
    {
        car.SetTrailColor(fireTrailWeaponData.trailColor);

        //Spawn DamageTrail
        GameObject damageTrailObj = new GameObject("Fire Trail Damage");
        damageTrailObj.transform.SetParent(car.transform);
        damageTrailObj.transform.localPosition = Vector3.zero;

        damageTrail = damageTrailObj.AddComponent<FireTrailDamage>();
        damageTrail.enemyLayer = enemyLayer;

        damageTrail.Init(damage, cooldown, fireTrailWeaponData.damageRadius);
    }
     
    protected override void Update()
    {
        base.Update();
        damageTrail.UpdateTrail(car.GetAllTrailPoints());
    }

    protected override void Fire()
    {
        damageTrail.SetStats(damage, cooldown);
    }

    protected override void ApplyLevelUp(int level)
    {
        damage *= 1.5f;
        cooldown *= 0.85f;
        car.stats.DriftSegmentCount.Add(10);

        damageTrail.SetStats(damage, cooldown);
    }
}
