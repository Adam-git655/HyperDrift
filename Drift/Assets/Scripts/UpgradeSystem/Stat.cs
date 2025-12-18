using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    public float BaseValue { get; private set; }

    private float additiveValue;
    private float multiplicativeValue = 1f;

    public float Value => (BaseValue + additiveValue) * multiplicativeValue;

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
    }

    public void Add(float value)
    {
        additiveValue += value;
    }

    public void Multiply(float value)
    {
        multiplicativeValue *= value;
    }
}
