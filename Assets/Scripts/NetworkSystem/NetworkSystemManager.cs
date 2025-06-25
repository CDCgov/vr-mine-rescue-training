using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Unity component holding network system information
/// </summary>
public class NetworkSystemManager : MonoBehaviour {
    public NetworkSystem NetworkSys;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Debug.Log(NetworkSys._Nodes.Count);
        }
    }

    public List<T> GetNodes<T>()
    {
        List<T> myList = new List<T>();
        if (NetworkSys._Nodes != null)
        {
            Debug.Log(NetworkSys._Nodes.Count);

            foreach (NetworkNode node in NetworkSys._Nodes)
            {
                if (node.GetType() == typeof(T))
                {
                    myList.Add((T)Convert.ChangeType(node, typeof(T)));
                }
            }
        }
        else
        {
            Debug.Log("Nodes not implemented yet");
        }
        return myList;
    }

    /// <summary>
    /// Get all nodes of a type from the network system, corresponding to a specified networkID
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="networkID"></param>
    /// <returns></returns>
    public List<T> GetNodes<T>(int networkID)
    {
        List<T> myList = new List<T>();
        if (NetworkSys._Nodes != null)
        {
            Debug.Log(NetworkSys._Nodes.Count);

            foreach (NetworkNode node in NetworkSys._Nodes)
            {
                if (node.GetType() == typeof(T) && node.NetworkID == networkID)
                {
                    myList.Add((T)Convert.ChangeType(node, typeof(T)));
                }
            }
        }
        else
        {
            Debug.Log("Nodes not implemented yet");
        }
        return myList;
    }
}
