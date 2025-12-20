using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeOptionUI : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Value;
    private StatUpgrade upgrade;

    //setup the button with the upgrade data
    public void Setup(StatUpgrade data)
    {
        upgrade = data;
        Icon.sprite = upgrade.icon;
        Name.text = upgrade.upgradeName;

        //Show value in percentage or base numbers 
        if (upgrade.modifierType == ModifierType.Additive)
            Value.text = "+" + upgrade.value.ToString();
        else if (upgrade.modifierType == ModifierType.Multiplicative)
            Value.text = "+" + Mathf.RoundToInt(((upgrade.value - 1) * 100)).ToString() + "%";
    }

    public void OnClick()
    {
        UpgradeManager.Instance.ApplyPlayerUpgrade(upgrade);
        UpgradeMenu.Instance.Close();
    }
}
