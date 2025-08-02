using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public GameObject upgradeMenu;
    void Start()
    {
        upgradeMenu.SetActive(false);
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

    }
    public void OnCarAttackDurationUpgraded()
    {

    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }

}
