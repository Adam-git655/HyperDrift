using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Upgrade : ScriptableObject
{
    public string upgradeName;
    public Sprite icon;
    public string upgradeDescription;
    public abstract void Apply(Car player);
}
