using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUI : MonoBehaviour
{
    string inputFileName;
    public GameObject overwriteWarningUI;
    public InputField nameUIField;

    public void SaveCurrentSceneFromNameInput()
    {
        Debug.Log("SAVING USING: " + inputFileName);
        if (String.IsNullOrEmpty(inputFileName))
        {
            inputFileName = GameObject.Find("SaveScenarioNameInput").GetComponent<InputField>().text;
            Debug.Log("INPUTFILENAME WAS NULL REPLACED WITH: " + inputFileName);
            if (String.IsNullOrEmpty(inputFileName))
            {
                Debug.LogWarning("Attempted to save with no text entered! No ");
                return;
            }
        }
        if(JSONFileManagement.CheckForExistingFileName(ScenarioSaveLoad.Instance.GetScenarioFilePath() + "/" + inputFileName + ".json"))
        {
            Debug.Log("EXISTING FILE " + inputFileName + " FOUND AT: " + ScenarioSaveLoad.Instance.GetScenarioFilePath() + "/" + inputFileName + ".json");
            if(ScenarioSaveLoad.Instance.WorkingScenarioName == inputFileName)
            {
                Debug.Log("FILE " + inputFileName + " MATCHES WORKING FILE NAME OF: " + ScenarioSaveLoad.Instance.WorkingScenarioName);
                _ = ScenarioSaveLoad.Instance.SaveCurrentScenario(inputFileName);
            }
            else
            {
                Debug.Log("WORKING FILE NAME MISMATCH, DEPLOY OVERWRITE WARNING");
                overwriteWarningUI.SetActive(true);
            }

        }
        else
        {
            Debug.Log("NO EXISTING FILE BY NAME: " + inputFileName + " FOUND AT: " + ScenarioSaveLoad.Instance.GetScenarioFilePath() + "/" + inputFileName + ".json");
            _ = ScenarioSaveLoad.Instance.SaveCurrentScenario(inputFileName);
        }
        
    }

    public void SaveSceneAfterOverwriteWarning()
    {
        _ = ScenarioSaveLoad.Instance.SaveCurrentScenario(inputFileName);
    }

    public void ClearWorkingName()
    {
        inputFileName = String.Empty;
    }

    public void SetCurrentScenarioName(string scenarioName)
    {
        inputFileName = scenarioName;
    }
    public void PrepopulateInputWithWorkingScenarioName()
    {
        string name = ScenarioSaveLoad.Instance.WorkingScenarioName;
        if (!String.IsNullOrEmpty(name))
        {
            nameUIField.text = name;
        }
    }

    public void InitiateFileChangeDestination()
    {
        FolderBrowserDialog dialog = new FolderBrowserDialog();
        if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ScenarioSaveLoad.Instance.ChangeSaveDestination(dialog.SelectedPath);
        }

        /* var path = EditorUtility.SaveFilePanel(
     "Choose new scenario save destination",
 "",
     "",
     "");*/
    }


}
