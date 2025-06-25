using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct CustomScenarioData
{
    public string ScenarioName;
    public DateTime ScenarioDate;
}

public static class CustomScenarioUtil
{
    public static IEnumerable<CustomScenarioData> GetCustomScenarios(string path = null)
    {
        if (path == null)
            path = ScenarioSaveLoad.Instance.GetScenarioFilePath();

        List<string> customScenarioNames = JSONFileManagement.GetJSONFileNamesWithoutExtensions(path);
        

        foreach (string scenarioName in customScenarioNames)
        {
            var filePath = System.IO.Path.Combine(path, scenarioName + ".json");

            CustomScenarioData data = new CustomScenarioData();
            data.ScenarioName = scenarioName;
            data.ScenarioDate = JSONFileManagement.GetDateTimeForFile(filePath/*path + scenarioName + ".json"*/);
            //Debug.Log($"Got date for {filePath} of {data.ScenarioDate.ToString()}");
            yield return data;

        }
    }

    public static void GetAllCustomScenarioData(ref List<CustomScenarioData> list, string path = null)
    {
        if (list == null)
            list = new List<CustomScenarioData>();

        list.Clear();

        foreach (var data in GetCustomScenarios(path))
        {
            list.Add(data);
        }
    }

    public static void SortByScenarioName(this List<CustomScenarioData> list, bool reverse)
    {
        list.Sort((a, b) =>
        {
            var result = String.Compare(a.ScenarioName, b.ScenarioName, true);
            if (reverse)
                return result * -1;
            return result;
        });
    }

    public static void SortByScenarioDate(this List<CustomScenarioData> list, bool reverse)
    {
        list.Sort((a, b) =>
        {
            var result = DateTime.Compare(a.ScenarioDate, b.ScenarioDate);
            if (reverse)
                return result * -1;
            return result;
        });
    } 

    public static string GetScenarioFilename(string scenarioName)
    {
        return System.IO.Path.Combine(ScenarioSaveLoad.Instance.GetScenarioFilePath(), scenarioName + ".json");
    }
}
