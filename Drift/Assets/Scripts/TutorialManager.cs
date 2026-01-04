using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] TutorialPopUpPanels;
    public float waitTimePostPopup = 5f;
    public GameObject enemySpawner;
    public GameObject dronePrefab;
    public Car car;

    public InputActionReference clickAction;
    public InputActionReference steerAction;
    public InputActionReference driftAction;
    public InputActionReference hyperDriftAction;

    private int popUpIndex = 0;

    public enum Actions
    {
        Steer,
        Drift,
        ActivateHyperDrift,
        NoActionReq
    }

    public Actions[] requiredActions;

    void OnEnable()
    {
        clickAction.action.Enable();
        steerAction.action.Enable();
        driftAction.action.Enable();
        hyperDriftAction.action.Enable();
    }

    void OnDisable()
    {
        clickAction.action.Disable();
        steerAction.action.Disable();
        driftAction.action.Disable();
        hyperDriftAction.action.Disable();
    }

    void Awake()
    {
        foreach (var popUp in TutorialPopUpPanels)
            popUp.SetActive(false);

        //PlayerPrefs.DeleteKey("HasLaunched"); // for debugging and testing purposes

        //Check if game opened for first time
        if (PlayerPrefs.GetInt("HasLaunched", 0) == 0)
        {
            StartCoroutine(PlayTutorial());

            PlayerPrefs.SetInt("HasLaunched", 1);
            PlayerPrefs.Save();
        }
        else
        {
            enemySpawner.SetActive(true);
            Globals.StartGameplayTimer();
        }
    }

    private IEnumerator PlayTutorial()
    {
        //dont allow enemies to spawn before tutorial finishes
        enemySpawner.SetActive(false);

        while (popUpIndex < TutorialPopUpPanels.Length)
        {
            //pause game and show popup
            TutorialPopUpPanels[popUpIndex].SetActive(true);
            car.DisableInput();
            Time.timeScale = 0f;

            //wait for mouse click
            yield return StartCoroutine(WaitForClick());

            //resume game after mouse click and hide popup
            TutorialPopUpPanels[popUpIndex].SetActive(false);
            car.EnableInput();
            Time.timeScale = 1f;

            //when instruction in popup is followed, then move to the next popup and repeat until all popus are over
            yield return StartCoroutine(WaitForAction());

            popUpIndex++;
        }

        //allow enemies to spawn now
        enemySpawner.SetActive(true);

        //start gameplay timer
        Globals.StartGameplayTimer();
    }

    private IEnumerator WaitForClick()
    {
        while (!clickAction.action.WasPerformedThisFrame())
            yield return null;
    }

    private IEnumerator WaitForAction()
    {
        switch(requiredActions[popUpIndex])
        {
            case Actions.Steer:
                while (!steerAction.action.WasPerformedThisFrame())
                    yield return null;
                break;

            case Actions.Drift:
                while (!driftAction.action.WasPerformedThisFrame())
                    yield return null;
                break;

            case Actions.ActivateHyperDrift:
                enemySpawner.GetComponent<EnemySpawner>().SpawnDronesForTutorial(5);
                while(!hyperDriftAction.action.WasPerformedThisFrame())
                    yield return null;
                break;

            case Actions.NoActionReq:
                break;
        }

        yield return new WaitForSeconds(waitTimePostPopup);
    }
}
