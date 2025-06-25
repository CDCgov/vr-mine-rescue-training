using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIListScenarios : MonoBehaviour
{
    public GameObject Prefab;
    public bool IncludeCustomScenarios = true;

    private string CUSTOM_SCENARIO_FILE_PATH;


    void Awake()
    {
        InitCustomScenarioPath();
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        List<ScenarioData> scenarioList = new List<ScenarioData>();
        InitCustomScenarioPath();

        var loadHandle = Addressables.LoadAssetsAsync<ScenarioData>("ScenarioData", (scenarioData) => {
            scenarioList.Add(scenarioData);
        });
        yield return loadHandle;
        Addressables.Release(loadHandle);

        scenarioList.Sort((s1, s2) =>
        {
            return string.Compare(s1.ScenarioName, s2.ScenarioName);
        });

        foreach (var data in scenarioList)
        {
            AddScenario(data);
        }

        if (IncludeCustomScenarios)
            AddCustomScenarios();
    }

    void InitCustomScenarioPath()
    {
        if (ScenarioSaveLoad.Instance != null)
            CUSTOM_SCENARIO_FILE_PATH = ScenarioSaveLoad.Instance.GetScenarioFilePath();//Application.persistentDataPath + "/Scenarios/";
        else
            CUSTOM_SCENARIO_FILE_PATH = null;
    }


    void AddCustomScenarios()
    {
        InitCustomScenarioPath();

        if (CUSTOM_SCENARIO_FILE_PATH == null)
            return;

        List<string> customScenarioNames = JSONFileManagement.GetJSONFileNamesWithoutExtensions(CUSTOM_SCENARIO_FILE_PATH);
        foreach(string scenarioName in customScenarioNames)
        {
            AddScenario(scenarioName + ".json", scenarioName);
        }
    }

    void AddScenario(string SceneName,string ScenarioName)
    {
        if (Prefab == null || SceneName == null || ScenarioName == null ||
            ScenarioName.Length <= 0 || SceneName.Length <= 0)
            return;

        //Debug.Log($"UIListScenarios: Adding {ScenarioName}");

        var go = Instantiate<GameObject>(Prefab, transform, false);
        //go.transform.SetParent(transform, false);
        go.name = SceneName;
        go.SetActive(true);

        var selectBtn = go.GetComponent<UIBtnSelectScene>();
        if (selectBtn != null)
            selectBtn.SceneName = SceneName;

        var txtObjs = go.GetComponentsInChildren<TMPro.TMP_Text>();
        foreach (var txt in txtObjs)
        {
            txt.text = ScenarioName;
        }

        
    }

    void AddScenario(ScenarioData data)
    {
        AddScenario(data.SceneName, data.ScenarioName);
    }

}
