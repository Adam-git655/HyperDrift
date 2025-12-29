using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Player Base Stats")]
public class PlayerBaseStats : ScriptableObject
{
    public float maxHealth;
    public float maxSpeed;
    public float attackModeDuration;
    public float pickupRange;
    public float damage;
    public float damageReduction;
    public float driftChargeRate;
    public int driftSegmentCount;
}
