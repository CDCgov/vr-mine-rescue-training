using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshToLeakyFeeder : NetworkNode {

    public int MeshNetworkID = 0;
    public int LeakyFeederNetworkID = 0;
    public NetworkSystemManager NetSysManager;

    public NetworkSystem _netSys;
    private MeshNode _nearestMeshNode;
    private LeakyFeederNode _nearestLFNode;
    
    // Use this for initialization
    void Start () {
        if(NetSysManager == null)
        {
            NetSysManager = GameObject.FindObjectOfType<NetworkSystemManager>();
            if(NetSysManager == null)
            {
                GameObject go = new GameObject();
                go.transform.position = new Vector3(0, 0, 0);
                go.AddComponent<NetworkSystemManager>();
                NetSysManager.NetworkSys = new NetworkSystem();
            }
        }
        _netSys = NetSysManager.NetworkSys;

        GetNetworks();
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    void GetNetworks()
    {
        //Get Mesh Network
        List<MeshNode> mNodes = _netSys.GetNodes<MeshNode>(MeshNetworkID);
        //Debug.Log(mNodes.Count + ", mesh node count");
        float distance = 9999;
        foreach (MeshNode node in mNodes)
        {
            float d = Vector3.Distance(node.transform.position, transform.position);
            if (d < distance)
            {
                distance = d;
                _nearestMeshNode = node;
                //Debug.Log("Found near mesh node: " + node.name + ", " + d);
            }
        }

        //Get LF Network
        List<LeakyFeederNode> lfNodes = _netSys.GetNodes<LeakyFeederNode>(LeakyFeederNetworkID);
        //Debug.Log(mNodes.Count + ", lf node count");
        distance = 9999;
        foreach (LeakyFeederNode node in lfNodes)
        {
            float d = Vector3.Distance(node.transform.position, transform.position);
            if (d < distance)
            {
                distance = d;
                _nearestLFNode = node;
                //Debug.Log("Found near lf node: " + node.name + ", " + d);
            }
        }
        NetworkPath path = new NetworkPath();
        if (NetworkPaths == null)
        {
            NetworkPaths = new List<NetworkPath>();
        }
        NetworkPaths.Clear();
        path.Capacity = 1;
        path.Impedance = 0;
        path.Source = this;
        path.Destination = _nearestMeshNode;
        NetworkPaths.Add(path);
        if(_nearestMeshNode.NetworkPaths == null)
        {
            _nearestMeshNode.NetworkPaths = new List<NetworkPath>();
        }
        path.Destination = this;
        path.Source = _nearestMeshNode;
        _nearestMeshNode.NetworkPaths.Add(path);

        path.Source = this;
        path.Destination = _nearestLFNode;
        NetworkPaths.Add(path);
        if(_nearestLFNode.NetworkPaths == null)
        {
            _nearestLFNode.NetworkPaths = new List<NetworkPath>();
        }
        path.Destination = this;
        path.Source = _nearestLFNode;
        _nearestLFNode.NetworkPaths.Add(path);
    }

    public void ReceiveMessage()
    {

    }

    public override List<NetworkPath> ComputeLocalConnectivity()
    {
        throw new NotImplementedException();
    }

    public override GameObject CreateNode()
    {
        throw new NotImplementedException();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }

    public override void SendToReceivers(string message)
    {
        
    }

    private void SendMessageThroughNodes(string message, NetworkNode current, List<HandheldRadio> radios, ref List<NetworkNode> visitedNodes)
    {
        visitedNodes.Add(current);
        List<HandheldRadio> radCopy = new List<HandheldRadio>(radios);
        
        foreach (NetworkPath path in current.NetworkPaths)
        {
            if (!visitedNodes.Contains(path.Destination))
            {
                SendMessageThroughNodes(message, path.Destination, radCopy, ref visitedNodes);
            }
        }
    }
}
