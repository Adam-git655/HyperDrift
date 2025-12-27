using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Player Base Stats")]
public class PlayerBaseStats : ScriptableObject
{
    public float maxHealth;
    public float maxSpeed;
    public float attackModeDuration;
    public float gearPickupRange;
    public float damage;
    public float driftChargeRate;
    public int driftSegmentCount;
}
