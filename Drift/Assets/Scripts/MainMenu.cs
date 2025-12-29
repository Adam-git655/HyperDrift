using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject upgradePanel;

    private void Start()
    {
        upgradePanel.SetActive(false);
    }

    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene("Game");
    }

    public void OnUpgradeButtonPressed()
    {
        upgradePanel.SetActive(true);
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }

}
