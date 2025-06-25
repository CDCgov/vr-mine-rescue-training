using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectInfo))]
public class TeamstopNameHelper : MonoBehaviour
{
    public POIManager POIManager;
    // Start is called before the first frame update
    void Start()
    {
        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);

        if (POIManager == null)
        {
            Debug.LogError("Couldn't find POI Manager");
            return;
        }
        
        ObjectInfo info = gameObject.GetComponent<ObjectInfo>();
        if(info.UserSuppliedName != null && (info.UserSuppliedName != info.AssetWindowName))
        {
            //Debug.Log($"User supplied name detected: {info.UserSuppliedName}");
            return;
        }
        string name = $"Teamstop_Custom_{POIManager.ActivePOIs.Count}";
        
        gameObject.name = name;
        info.InstanceName = name;
        
        //Debug.Log($"Configured Teamstop_Custom_{POIManager.ActivePOIs.Count}");        

    }

}
