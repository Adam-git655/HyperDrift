using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UpgradeMenu : MonoBehaviour
{
    public static UpgradeMenu Instance;

    public GameObject panel;

    public UpgradeOptionUI[] options;

    public Tilemap tilemap;

    private void Awake()
    {
        //Singleton
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        panel.SetActive(false);
    }

    public void Open()
    {
        Time.timeScale = 0f; //pause game for upgrade
        panel.SetActive(true);
        tilemap.color = Color.gray;

        //get 3 random upgrades from upgrade manager
        List<StatUpgrade> upgrades = UpgradeManager.Instance.GetRandomUpgrades(options.Length);

        //setup the buttons with the upgrades
        for (int i = 0; i < options.Length; i++)
        {
            options[i].Setup(upgrades[i]);
        }
    }

    public void Close()
    {
        panel.SetActive(false);
        tilemap.color = Color.white;
        Time.timeScale = 1f; //resume game again
    }
}
