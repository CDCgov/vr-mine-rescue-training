using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainScreenController : MonoBehaviour
{
    //public GameObject RootMenu;0
    //public GameObject SinglePlayerRoot;1
    //public GameObject MultiplayerRoot;2
    //public GameObject DebriefRoot;3
    //public GameObject HelpRoot;5
    //public GameObject SettingsRoot;4
    //ScenarioSelection 6
    public List<GameObject> RootMenus;
    public SceneConfiguration SceneConfigurations;

    private InputAction _actionLoadEditorScene;

    public void BackToMainMenu()
    {
        foreach(GameObject menus in RootMenus)
        {
            menus.SetActive(false);
        }
        RootMenus[0].SetActive(true);
    }

    public void SinglePlayerMenu()
    {
        foreach (GameObject menus in RootMenus)
        {
            menus.SetActive(false);
        }
        RootMenus[1].SetActive(true);
    }

    public void MultiplayerMenu()
    {
        foreach (GameObject menus in RootMenus)
        {
            menus.SetActive(false);
        }
        RootMenus[2].SetActive(true);
    }

    public void DebriefMenu()
    {
        foreach (GameObject menus in RootMenus)
        {
            menus.SetActive(false);
        }
        RootMenus[3].SetActive(true);
    }
    public void SettingsMenu()
    {
        foreach (GameObject menus in RootMenus)
        {
            menus.SetActive(false);
        }
        RootMenus[4].SetActive(true);
    }

    public void HelpMenu()
    {
        foreach (GameObject menus in RootMenus)
        {
            menus.SetActive(false);
        }
        RootMenus[5].SetActive(true);
    }

    public void ScenarioMenu()
    {
        foreach (GameObject menus in RootMenus)
        {
            menus.SetActive(false);
        }
        RootMenus[6].SetActive(true);
        ScenarioMenuController scenarioMenuController = RootMenus[6].GetComponent<ScenarioMenuController>();
        if (SceneConfigurations.IsDM)
        {
            scenarioMenuController.SetMenuTypeLabel("Director Mode");
            return;
        }
        if (SceneConfigurations.IsSpectator)
        {
            scenarioMenuController.SetMenuTypeLabel("Spectator Mode");
            return;
        }
        scenarioMenuController.SetMenuTypeLabel("Participant");
        //Run Extra logic here
    }


    public void ExitVRMine()
    {
        Debug.Log("Exiting VR Mine");
        Application.Quit();
    }

    void Start()
    {
        _actionLoadEditorScene = InputSystem.actions.FindAction("LoadExternalAssetsEditor");
        if (_actionLoadEditorScene != null)
        {
            _actionLoadEditorScene.performed += OnActionLoadExternalAssetsEditor;
        }
    }

    void OnDestroy()
    {
        if (_actionLoadEditorScene != null)
        {
            _actionLoadEditorScene.performed -= OnActionLoadExternalAssetsEditor;
        }
    }

    private void OnActionLoadExternalAssetsEditor(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("ExternalAssetsEditorTest");
    }

}
