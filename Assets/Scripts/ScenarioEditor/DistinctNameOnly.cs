using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectInfo))]
public class DistinctNameOnly : MonoBehaviour
{
    DistinctNameManager manager;
    ObjectInfo info;

    private void Start()
    {
        manager = FindObjectOfType<DistinctNameManager>();
        if(manager == null)
        {
            Debug.LogError("NO DISTINCT NAME MANAGER FOUND. PLEASE ADD ONE TO THE SCENE.");
            return;
        }
        info = GetComponent<ObjectInfo>();
        string newName = manager.GetNewDistinctName(info.DisplayName);

        info.componentInfo_Name.instanceDisplayName = newName;
        gameObject.name = newName;
    }

    private void OnDestroy()
    {
        if(manager!=null)
        {
            manager.RemoveDistinctName(info.DisplayName,info.componentInfo_Name.instanceDisplayName);
        }
    }


}
