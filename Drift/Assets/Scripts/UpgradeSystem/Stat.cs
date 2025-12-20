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

    public event System.Action<float> OnValueChanged; //returns new value

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
    }

    public void Add(float value)
    {
        additiveValue += value;
        OnValueChanged?.Invoke(Value);
    }

    public void Multiply(float value)
    {
        multiplicativeValue *= value;
        OnValueChanged?.Invoke(Value);
    }
}
