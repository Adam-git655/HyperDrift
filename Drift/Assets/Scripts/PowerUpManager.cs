using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PowerUps
{ 
    None,
    Damage,
    Health,
    Armor
}

public class PowerUpManager : MonoBehaviour
{
    public PowerUpOption selectedPowerUp;
    public TextMeshProUGUI currentSalvageCoresText;

    private void Start()
    {
        currentSalvageCoresText.text = Globals.totalSalvageCores.ToString();
    }

    public void OnCrossButtonPressed()
    {
        gameObject.SetActive(false);
    }

    public void OnBuyButtonPressed()
    {
        if (Globals.totalSalvageCores < selectedPowerUp.powerUpCost)
            return;

        if (selectedPowerUp.powerUpRank >= selectedPowerUp.ranks.childCount)
            return;

        Globals.totalSalvageCores -= selectedPowerUp.powerUpCost;
        currentSalvageCoresText.text = Globals.totalSalvageCores.ToString();

        switch (selectedPowerUp.powerUpType)
        {
            //add 10%
            case PowerUps.Damage:
                Globals.playerMeta.damageRank++;
                break;

            //add by 10
            case PowerUps.Health:
                Globals.playerMeta.healthRank++;
                break;
            
            //add 10%
            case PowerUps.Armor:
                Globals.playerMeta.armorRank++;
                break;
        }

        selectedPowerUp.UpdateRank();
    }
}
