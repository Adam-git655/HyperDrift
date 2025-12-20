using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class GearGaugeManager : MonoBehaviour
{
    public static GearGaugeManager Instance;
    public Slider gearGauge;
    [SerializeField] private int currentGearExp = 0;
    [SerializeField] private int maxGearExp = 20;
    [SerializeField] private int maxGearExpIncrease = 20;
    [SerializeField] private int currentLevel = 1;

    private void Awake()
    {
        //Singleton
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        gearGauge.value = currentGearExp;
        gearGauge.maxValue = maxGearExp;
    }

    public void AddGearExp(int amount)
    {
        currentGearExp += amount;
        gearGauge.value = currentGearExp; //Increase visual slider val

        if (currentGearExp >= maxGearExp)
        {
            OnLevelUp();
        }
    }

    private void OnLevelUp()
    {
        //Showing Ability upgrade menu
        UpgradeMenu.Instance.Open();

        //Increasing level
        currentLevel++;

        //Increasing max gear exp required to level up again
        currentGearExp = 0;
        maxGearExp += maxGearExpIncrease;

        //Change gearGauge visual slider values
        gearGauge.value = currentGearExp;
        gearGauge.maxValue = maxGearExp;
    }
}
