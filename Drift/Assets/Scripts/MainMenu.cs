using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public GameObject upgradeMenu;
    public Text gearsLeft;
    public int healthCost = 50;
    public int attackDurationCost = 70;
    public Text currentHealthText;
    public Text currentAttackDurationText;

    void Start()
    {
        upgradeMenu.SetActive(false);
    }

    private void Update()
    {
        gearsLeft.text = Globals.totalGears.ToString();
        currentHealthText.text = Globals.maxCarHealth.ToString();
        currentAttackDurationText.text = Globals.attackModeDuration.ToString();
    }

    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnUpgradeButtonPressed()
    {
        upgradeMenu.SetActive(true);
        transform.GetChild(1).GetComponent<Image>().color = Color.gray;
        transform.GetChild(2).GetComponent<Image>().color = Color.gray;
        transform.GetChild(2).GetChild(0).GetComponent<Image>().color = Color.gray;
        transform.GetChild(3).GetComponent<Image>().color = Color.gray;
        transform.GetChild(3).GetChild(0).GetComponent<Image>().color = Color.gray;
        transform.GetChild(4).GetComponent<Image>().color = Color.gray;
        transform.GetChild(4).GetChild(0).GetComponent<Image>().color = Color.gray;
    }

    public void OnUpgradeMenuCrossPressed()
    {
        upgradeMenu.SetActive(false);
        transform.GetChild(1).GetComponent<Image>().color = Color.white;
        transform.GetChild(2).GetComponent<Image>().color = Color.white;
        transform.GetChild(2).GetChild(0).GetComponent<Image>().color = Color.white;
        transform.GetChild(3).GetComponent<Image>().color = Color.white;
        transform.GetChild(3).GetChild(0).GetComponent<Image>().color = Color.white;
        transform.GetChild(4).GetComponent<Image>().color = Color.white;
        transform.GetChild(4).GetChild(0).GetComponent<Image>().color = Color.white;
    }

    public void OnCarHealthUpgraded()
    {
        if (Globals.totalGears >= healthCost)
        {
            Globals.totalGears -= healthCost;
            Globals.maxCarHealth += 20f;
        }
        
    }
    public void OnCarAttackDurationUpgraded()
    {
        if (Globals.totalGears >= attackDurationCost)
        {
            Globals.totalGears -= attackDurationCost;
            Globals.attackModeDuration += 2f;
        }
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }

}
