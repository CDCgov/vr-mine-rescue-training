using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioLoadUIManager : MonoBehaviour
{
    public Button loadButton;
    public Button cancelButton;
    private string CUSTOM_SCENARIO_FILE_PATH;
    public GameObject loadableScenarioPrefab;
    public GameObject content;
    string currentSelectedScenarioName;
    Button selectedScenarioButton;
    List<CustomScenarioData> customScenarioUIList = new List<CustomScenarioData>();

    void Awake()
    {
        CUSTOM_SCENARIO_FILE_PATH = ScenarioSaveLoad.Instance.GetScenarioFilePath();//Application.persistentDataPath + "/Scenarios/";
        GetCustomScenarioData();
    }

    private void OnEnable()
    {
        CUSTOM_SCENARIO_FILE_PATH = ScenarioSaveLoad.Instance.GetScenarioFilePath();//Application.persistentDataPath + "/Scenarios/";
        GetCustomScenarioData();
    }

    public void GetCustomScenarioData()
    {
        customScenarioUIList.Clear();

        foreach (var data in CustomScenarioUtil.GetCustomScenarios())
        {
            customScenarioUIList.Add(data);
        }

        //CUSTOM_SCENARIO_FILE_PATH = ScenarioSaveLoad.Instance.GetScenarioFilePath();//Application.persistentDataPath + "/Scenarios/";
        //List<string> customScenarioNames = JSONFileManagement.GetJSONFileNamesWithoutExtensions(CUSTOM_SCENARIO_FILE_PATH);
        //Debug.Log("CUSTOM SCENARIO PATH:  " + CUSTOM_SCENARIO_FILE_PATH);
        //foreach (string scenarioName in customScenarioNames)
        //{
        //    CustomScenarioData newUI = new CustomScenarioData();
        //    newUI.ScenarioName = scenarioName;
        //    newUI.ScenarioDate = JSONFileManagement.GetDateTimeForFile(CUSTOM_SCENARIO_FILE_PATH + scenarioName + ".json");
        //    customScenarioUIList.Add(newUI);
        //    //AddScenario(scenarioName + ".json", scenarioName, JSONFileManagement.GetDateTimeForFile(CUSTOM_SCENARIO_FILE_PATH + scenarioName+".json"));
        //}
        SortByDate();
    }

    string lastSortMethod; //TODO Don't really like this
    public void SortByDate()
    {
        
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        List<CustomScenarioData> sortedList = new List<CustomScenarioData>();
        sortedList = customScenarioUIList.OrderBy(x => x.ScenarioDate).ToList();

        if (lastSortMethod == "date")
        {
            sortedList.Reverse();
            lastSortMethod = string.Empty;
        }
        else
        {
            lastSortMethod = "date";
        }
        foreach (CustomScenarioData customScenarioUI in sortedList)
        {
            AddScenario(customScenarioUI);
        }
    }

    public void SortByName()
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        List<CustomScenarioData> sortedList = new List<CustomScenarioData>();
        sortedList = customScenarioUIList.OrderBy(x => x.ScenarioName).ToList();


        if (lastSortMethod == "name")
        {
            sortedList.Reverse();
            lastSortMethod = string.Empty;
        }
        else
        {
            lastSortMethod = "name";
        }
        foreach (CustomScenarioData customScenarioUI in sortedList)
        {
            AddScenario(customScenarioUI);
        }
    }

    void AddScenario(CustomScenarioData ui)
    {
        GameObject scenario = Instantiate(loadableScenarioPrefab, content.transform);
        HighlightTextMeshUI uiText = scenario.GetComponent<HighlightTextMeshUI>();

        scenario.name = ui.ScenarioName + ".json";
        Button btn = uiText.SelectButton;
        uiText.texts[0].text = ui.ScenarioName;
        if (uiText.texts.Count > 1)
        {
            uiText.texts[1].text = ui.ScenarioDate.ToString();
        }

        btn.onClick.AddListener(delegate { SelectScenario(ui.ScenarioName + ".json", btn); } );
    }

    public void SelectScenario(string scenarioName, Button btn)
    {
        Debug.Log("SELECTED: " + scenarioName);
        if(selectedScenarioButton != null)
        {
            selectedScenarioButton.GetComponent<Image>().color = Color.clear;
        }
        selectedScenarioButton = btn;
        selectedScenarioButton.GetComponent<Image>().color = Color.gray;
        currentSelectedScenarioName = scenarioName;
    }

    public void LoadScenario()
    {
        if (String.IsNullOrEmpty(currentSelectedScenarioName) || ScenarioInitializer.Instance == null)
            return;
        ScenarioInitializer.Instance.GenerateMineOnStart = false;
        ScenarioInitializer.Instance.LoadScenarioFromMainMenu(currentSelectedScenarioName);
    }


    //public void LoadScenario()
    //{
    //    if (!String.IsNullOrEmpty(currentSelectedScenarioName))
    //    {
    //        Debug.Log("LOADING: " + currentSelectedScenarioName);
    //        if(AssetLoader.Instance.IsEditorMode)
    //        {
    //            Debug.Log("IN EDITORMODE");
    //            ScenarioSaveLoad.Instance.LoadScenarioFromFile(currentSelectedScenarioName);
    //        }
    //        else
    //        {
    //            Debug.Log("NOT IN EDITORMODE");
    //            ScenarioInitializer.Instance.LoadScenarioFromMainMenu(currentSelectedScenarioName);
    //        }

    //    }
    //}

}
