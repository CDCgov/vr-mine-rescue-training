using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistinctNameManager : MonoBehaviour
{
    //Dictionary<string, int> distinctNameMap = new Dictionary<string, int>();
    Dictionary<string, List<string>> distinctNameMap = new Dictionary<string, List<string>>();

    private void Start()
    {
        ScenarioSaveLoad.Instance.onLoadStart += ClearAllNames;
    }

    void ClearAllNames()
    {
        distinctNameMap.Clear();
    }

    public void RemoveDistinctName(string id, string distinctName)
    {
        if (distinctNameMap.ContainsKey(id))
        {
            if (distinctNameMap[id].Contains(distinctName))
            {
                distinctNameMap[id].Remove(distinctName);
            }
            if (distinctNameMap[id].Count <= 0)
            {
                distinctNameMap.Remove(id);
            }
        }
    }

    /*public string GetNewDistinctName(string name)
    {
        if (distinctNameMap.ContainsKey(name))
        {
            int newNumber = distinctNameMap[name];
            Debug.Log("GRABBED NEW NUMBER:  " + newNumber);
            newNumber++;
            Debug.Log("NEW NUMBER IS NOW: " + newNumber);
            distinctNameMap[name] = newNumber;
            Debug.Log("NEW NUMBER LOGGED" + distinctNameMap[name]);
            string newName = name + "_" + newNumber;
            Debug.Log("RETURNING NAME OF: " + newName);
            return newName;
        }
        else
        {
            distinctNameMap.Add(name, 1);
            return name;
        }
    }*/

    public string GetNewDistinctName(string id)
    {
        if(distinctNameMap.ContainsKey(id))
        {
            string newName = id;
            
            int i = 1;
            List<string> storedNames = distinctNameMap[id];
            while(storedNames.Any(str => str.ToLower() == newName.ToLower()))
            {
                
                newName = id + "_" + i;
                i++;
            }
            distinctNameMap[id].Add(newName);
            return newName;
        }
        else
        {
            List<string> nameList = new List<string>();
            nameList.Add(id);
            distinctNameMap.Add(id, nameList);
            return id;
        }
    }
}
