using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertiaShieldWeapon : Weapon
{
    private Car car;

    private InertiaShieldWeaponData inertiaShieldWeaponData;
    private float shieldTime;

    public GameObject shieldVisual;

    protected override void Awake()
    {
        base.Awake();
        car = GetComponentInParent<Car>();
        inertiaShieldWeaponData = (InertiaShieldWeaponData) data;
        shieldTime = inertiaShieldWeaponData.shieldTime;
        shieldVisual.SetActive(false);
        car.canTakeDamage = true;
    }

    protected override void Fire()
    {
        //do nothing
    }

    public IEnumerator Activate()
    {
        car.canTakeDamage = false;
        shieldVisual.SetActive(true);

        yield return new WaitForSeconds(shieldTime);

        car.canTakeDamage = true;
        shieldVisual.SetActive(false);
    }

    protected override void ApplyLevelUp(int level)
    {
        shieldTime += 1f;
    }
}
