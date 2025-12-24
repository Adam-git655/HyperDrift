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

    public event System.Action<float, float> OnValueChanged; //returns old and new value

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
    }

    public void Add(float value)
    {
        float oldValue = Value;
        additiveValue += value;
        OnValueChanged?.Invoke(oldValue, Value);
    }

    public void Multiply(float value)
    {
        float oldValue = Value;
        multiplicativeValue *= value;
        OnValueChanged?.Invoke(oldValue, Value);
    }
}
