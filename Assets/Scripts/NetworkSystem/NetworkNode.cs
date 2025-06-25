using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A data structure holding the connection between two NetworkNodes and any properties of it.
/// </summary>
[System.Serializable]
public struct NetworkPath
{
    public NetworkNode Source;
    public NetworkNode Destination;
    public float Impedance;
    public float Capacity;
}

/// <summary>
/// An abstract class that determines local connectivity of a node.
/// </summary>
public abstract class NetworkNode : MonoBehaviour
{
    [HideInInspector]
    public NetworkSystem NetworkSys;
    //[HideInInspector]
    public List<NetworkPath> NetworkPaths;
    public int NetworkID;
    public bool IsRepeating = true;

    public abstract List<NetworkPath> ComputeLocalConnectivity();
    public abstract GameObject CreateNode();

    private void OnDestroy()
    {
        //NetworkSys.RemoveFromNetwork(this);
    }

    public virtual void SendToReceivers(string message)
    {

    }

    public virtual bool InRange(Vector3 pos)
    {
        Debug.Log("Not overridden " + gameObject.name);
        return true;
    }

    public virtual void ShowTracking(GameObject sender)
    {
        Debug.Log(Time.time + " Tracking: " + sender.name + ", Last Node In Range: " + gameObject.name);
    }
}
