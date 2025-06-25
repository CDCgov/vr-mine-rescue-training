using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_NetworkedObject : ModularComponentInfo, ISaveableComponent
{
    [Tooltip("Name of the components, unique within the LoadableAsset definition/prefab")]
    public string NetworkedObjectID = "NetObj1";
    public string UniqueIDString;

    public NetworkedObject NetworkedObject;

    public void Awake()
    {
        UniqueIDString = System.Guid.NewGuid().ToString();
    }

    public void LoadInfo(SavedComponent component)
    {
        var netID = component.GetParamValueString("NetID", null);
        if (netID == null)
            return;

        UniqueIDString = netID;

        if (NetworkedObject == null)
            TryGetComponent<NetworkedObject>(out NetworkedObject);

        if (NetworkedObject == null)
            return;

        NetworkedObject.UniqueIDString = netID;
        NetworkedObject.uniqueID = System.Guid.Parse(netID);
    }

    public string[] SaveInfo()
    {
        return new string[]
       {
            "NetID|" + UniqueIDString,
       };
    }

    public string SaveName()
    {
        return NetworkedObjectID;
    }
}
