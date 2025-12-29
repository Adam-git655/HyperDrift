using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpOption : MonoBehaviour
{
    public string powerUpName;
    public string description;
    public int powerUpCost;
    public PowerUps powerUpType;

    public GameObject BuyPanel;
    public TextMeshProUGUI DescriptionTextTitle;
    public TextMeshProUGUI DescriptionTextDescription;
    public TextMeshProUGUI DescriptionTextCost;
    public PowerUpManager powerUpManager;

    public Transform ranks;
    public int powerUpRank = 0;

    private void Start()
    {
        BuyPanel.SetActive(false);

        switch (powerUpType)
        {
            case PowerUps.Damage:
                powerUpRank = Globals.playerMeta.damageRank;
                break;
            case PowerUps.Health:
                powerUpRank = Globals.playerMeta.healthRank;
                break;
            case PowerUps.Armor:
                powerUpRank = Globals.playerMeta.armorRank;
                break;
        }

        VisuallyUpdateRank();
    }

    public void OnPowerUpButtonPressed()
    {
        BuyPanel.SetActive(true);
        DescriptionTextTitle.text = powerUpName;
        DescriptionTextDescription.text = description;
        DescriptionTextCost.text = powerUpCost.ToString();

        powerUpManager.selectedPowerUp = this;
    }

    public void UpdateRank()
    {
        powerUpRank++;
        VisuallyUpdateRank();
    }

    private void VisuallyUpdateRank()
    {
        for (int i = 0; i < ranks.childCount; ++i)
        {
            if (i < powerUpRank)
            {
                ranks.GetChild(i).GetComponent<Image>().color = Color.green;
            }
        }
    }
}
