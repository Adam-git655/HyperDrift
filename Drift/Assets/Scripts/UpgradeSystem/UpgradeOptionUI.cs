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
    public GameObject DescriptionPanel;
    public TextMeshProUGUI Description;

    private Upgrade upgrade;

    private void Start()
    {
        DescriptionPanel.SetActive(false);
    }

    //setup the button with the upgrade data
    public void Setup(Upgrade data)
    {
        upgrade = data;
        Icon.sprite = upgrade.icon;
        Name.text = upgrade.upgradeName;
        Description.text = upgrade.upgradeDescription;

        //Show value in percentage or base numbers 
        if (data is StatUpgrade stat)
        {
            if (stat.modifierType == ModifierType.Additive)
                Value.text = "+" + stat.value.ToString();
            else
                Value.text = "+" + Mathf.RoundToInt(((stat.value - 1) * 100)).ToString() + "%";
        }
        else
        {
            Value.text = "+1";
        }
    }

    public void OnClick()
    {
        UpgradeManager.Instance.ApplyUpgrade(upgrade);
        UpgradeMenu.Instance.Close();
    }

    public void OnPointerEnter()
    {
        DescriptionPanel.SetActive(true);
    }

    public void OnPointerExit()
    {
        DescriptionPanel.SetActive(false);
    }
}
