using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerMetaProgressionStats
{
    public int damageRank;
    public int healthRank;
    public int armorRank;

    public float DamageMultiplier => 1f + damageRank * 0.10f;
    public float BonusMaxHealth => healthRank * 10f;
    public float DamageReduction => armorRank * 0.10f;
}
