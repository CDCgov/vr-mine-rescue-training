using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


struct NetworkData
{

}

/// <summary>
/// A base class that handles connectivity of objects (of different types) via a graph. It includes both pressure/flow rate based and Instantaneous networks.
/// </summary>
[System.Serializable]
public class NetworkSystem {

    public int NetworkID;
    public List<NetworkNode> _Nodes;

    public void AddToNetwork(NetworkNode node)
    {
        //Debug.Log("Tried adding " + node.name + " to the system. . .");
        if (_Nodes == null)
        {
            _Nodes = new List<NetworkNode>();
            
        }
        //check if node already exists in the system
        else if (_Nodes.Contains(node))
        {
            return;
        }
        //Debug.Log(_Nodes.Count + ", pre");
        _Nodes.Add(node);
        //Debug.Log(_Nodes.Count + ", post");
        if(_Nodes.Count > 0)
        {
            foreach(NetworkNode n in _Nodes)
            {
                //Debug.Log(n.name);
            }
        }
    }

    public void RemoveFromNetwork(NetworkNode node)
    {
        if (_Nodes != null)
        {
            if (_Nodes.Remove(node))
            {
                Debug.Log(node.gameObject.name + " removed from Network System");
                RecalculateConnectivity();
            }
        }
    }

    /// <summary>
    /// Determines if two nodes are connected in the graph
    /// </summary>
    /// <param name="node1"></param>
    /// <param name="node2"></param>
    /// <param name="impedance"></param>
    /// <param name="capacity"></param>
    /// <returns></returns>
    public bool AreConnected(NetworkNode node1, NetworkNode node2, out float impedance, out float capacity)
    {
        bool isConnected = false;
        impedance = 1;
        capacity = 0;
        List<NetworkNode> visitedNodes = new List<NetworkNode>();
        visitedNodes.Add(node1);
        isConnected = SearchForConnection(node1, node2, ref visitedNodes);
        
        return isConnected;
    }

    /// <summary>
    /// Recursively searches through network paths to find the path that connects two nodes. TODO: Include logic to have it find the route with highest capacity and lowest impedance
    /// </summary>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <param name="visited"></param>
    /// <returns></returns>
    private bool SearchForConnection(NetworkNode current, NetworkNode target, ref List<NetworkNode> visited)
    {
        foreach(NetworkPath path in current.NetworkPaths)
        {
            if (!visited.Contains(path.Destination))
            {
                NetworkNode dest = path.Destination;
                visited.Add(dest);
                if(dest == target)
                {
                    return true;
                }
                else
                {
                    SearchForConnection(dest, target, ref visited);
                }
            }
        }
        return false;
    }	  

    /// <summary>
    /// Method to re-evaluate the connectivity graph should an event happen in-game to destroy a node
    /// </summary>
    private void RecalculateConnectivity()
    {
        foreach(NetworkNode node in _Nodes)
        {
            foreach(NetworkPath path in node.NetworkPaths)
            {
                if(path.Destination == null)
                {
                    node.NetworkPaths.Remove(path);
                }
            }
        }
    }

    /// <summary>
    /// Gets all network path connections to a particular node.
    /// </summary>
    /// <param name="node">Node to check</param>
    /// <returns></returns>
    public List<NetworkPath> GetConnections(NetworkNode node)
    {        
        return node.NetworkPaths;
    }

    //Change type from string to some sorty of node type?
    public NetworkPath[] GetConnectedNodesOfType(NetworkNode node, string type)
    {
        NetworkPath[] foo = null;
        return foo;
    }

    public Type GetNodeType(int networkID)
    {
        foreach(NetworkNode node in _Nodes)
        {
            if(node.NetworkID == networkID)
            {
                return node.GetType();
            }
        }

        return null;
    }

    public List<NetworkNode> GetAllNodesOfID(int networkID)
    {
        List<NetworkNode> output = new List<NetworkNode>();
        foreach(NetworkNode node in _Nodes)
        {
            if(node.NetworkID == networkID)
            {
                output.Add(node);
            }
        }
        return output;
    }

    /// <summary>
    /// Get all nodes from the network system of a specific type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public List<T> GetNodes<T>()
    {
        List<T> myList = new List<T>();
        if (_Nodes != null)
        {
            Debug.Log(_Nodes.Count);

            foreach (NetworkNode node in _Nodes)
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
        if (_Nodes != null)
        {
            //Debug.Log(_Nodes.Count);
            List<NetworkNode> copy = new List<NetworkNode>(_Nodes);

            foreach (NetworkNode node in copy)
            {                
                if (node != null)
                {
                    //Debug.Log(node.name + ": " + node.GetType());
                    if (node.GetType() == typeof(T) && node.NetworkID == networkID)
                    {
                        myList.Add((T)Convert.ChangeType(node, typeof(T)));
                    }
                }
                else
                {
                    _Nodes.Remove(node);
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
