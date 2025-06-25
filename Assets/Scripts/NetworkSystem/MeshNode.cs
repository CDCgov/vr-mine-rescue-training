using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshNode : NetworkNode {

    //private vars
    private List<Vector3> _meshPositions;
    private List<MeshNode> _cachedMeshPoints;
    private bool _VisCreated = false;
    private MeshRenderer VisualizationRenderer;

    //public vars
    public float Range = 60;
    public List<MeshNode> WireConnected;
    public List<HandheldRadio> Radios;
    public NetworkSystemManager NetSysManager;
    public bool debug = false;
    public Color VisualColor;
    public Material VisualMaterial;
    public bool ShowRangeDebug = false;
    public int NumberOfReflections = 0;


    public bool UseZones = true;
    public List<CommZone> CommZones;
    public List<ActorHost> ActorsInNode;
    public List<TrackingZone> Zones;

    // Use this for initialization
    void Awake () {        
        
        if (NetSysManager == null)
        {
            NetworkSystemManager networkManager = FindObjectOfType<NetworkSystemManager>();
            if (networkManager == null)
            {
                networkManager = transform.root.gameObject.AddComponent<NetworkSystemManager>();
                networkManager.NetworkSys = new NetworkSystem();
            }
            NetSysManager = networkManager;
        }

        Zones = new List<TrackingZone>();
        TrackingZone[] gettingZones = gameObject.GetComponentsInChildren<TrackingZone>();
        foreach (TrackingZone zone in gettingZones)
        {
            Zones.Add(zone);
        }

        if (WireConnected == null)
        {
            WireConnected = new List<MeshNode>();
        }

        if(NetSysManager.NetworkSys == null)
        {
            NetSysManager.NetworkSys = new NetworkSystem();
            AddAllNodesToNetwork();
        }
        if(Radios == null)
        {
            Radios = new List<HandheldRadio>();
        }
        HandheldRadio[] radios = GameObject.FindObjectsOfType<HandheldRadio>();
        for(int i = 0; i < radios.Length; i++)
        {
            if(radios[i].NetworkID == NetworkID)
            {
                if (!Radios.Contains(radios[i]))
                {
                    Radios.Add(radios[i]);
                }
            }
        }
        if(CommZones == null)
        {
            CommZones = new List<CommZone>();
        }
        else
        {
            CommZones.Clear();
        }
        for(int i = 0; i < transform.childCount; i++)
        {
            CommZone com = transform.GetChild(i).GetComponent<CommZone>();
            if (com != null)
            {
                CommZones.Add(com);
            }
        }

        CalculateStartConnectivity();
        
    }
    
    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Return))
        {            
            if (debug)
            {
                DestroyMeshNode();
            }
        }
        if (Input.GetKeyUp(KeyCode.F1))
        {
            Visualize();
        }
    }

    public override void SendToReceivers(string message)
    {
        //foreach(MeshNode node in _cachedMeshPoints)
        //{
        //    foreach(HandheldRadio rad in Radios)
        //    {
        //        if (node.InRange(rad.transform.position))
        //        {
        //            rad.ReceiveMessage(message);
        //        }
        //    }
        //}

        List<NetworkNode> visited = new List<NetworkNode>();
        //visited.Add(this);
        List<HandheldRadio> rads = new List<HandheldRadio>(Radios);

        SendMessageThroughNodes(message, this, rads, ref visited);        
    }

    private void SendMessageThroughNodes(string message, NetworkNode current, List<HandheldRadio> radios,ref List<NetworkNode> visitedNodes)
    {
        visitedNodes.Add(current);
        List<HandheldRadio> radCopy = new List<HandheldRadio>(radios);
        if (current.GetType() != typeof(MeshToLeakyFeeder)) {
            foreach (HandheldRadio rad in radios)
            {
                if (current.InRange(rad.transform.position))
                {
                    rad.ReceiveMessage(message);
                    radCopy.Remove(rad);
                    Debug.Log("Rad: " + rad.name + ", Source: " + current.name);
                    //Debug.DrawLine(current.transform.position, rad.transform.position);
                }
            }
        }
        foreach (NetworkPath path in current.NetworkPaths)
        {
            if (!visitedNodes.Contains(path.Destination))
            {
                SendMessageThroughNodes(message, path.Destination, radCopy, ref visitedNodes);
            }
        }
    }

    public override bool InRange(Vector3 pos)
    {        
        if (Vector3.Distance(pos, transform.position) < Range)
        {
            int layerMask = 1 << 20;
            layerMask = ~layerMask;
            if(Physics.Linecast(transform.position,pos, layerMask))
            {                
                return false;
            }
            else
            {
                Debug.DrawLine(transform.position, pos, Color.yellow);
                return true;
            }
        }
        return false;
    }

    public override List<NetworkPath> ComputeLocalConnectivity()
    {
        List<MeshNode> meshNodes = NetSysManager.NetworkSys.GetNodes<MeshNode>(NetworkID);
        _cachedMeshPoints = meshNodes;
        foreach(MeshNode node in meshNodes)
        {
            node.NetworkPaths.Clear();
            for (int i = 0; i < meshNodes.Count; i++)
            {
                if (WireConnected.Contains(meshNodes[i]))
                {
                    ConnectToNode(meshNodes[i]);
                    Debug.Log(gameObject.name + " connect to wire" + meshNodes[i].name);
                    continue;
                }
                if (node != meshNodes[i] && node.IsRepeating)
                {
                    if (!UseZones)
                    {
                        if (node.Range > Vector3.Distance(node.transform.position, meshNodes[i].transform.position))
                        {
                            if (node.MeshConnectionCheck(meshNodes[i]))
                            {
                                node.ConnectToNode(meshNodes[i]);
                            }
                        }
                    }
                    else
                    {
                        MeshNode foundNode = null;
                        foreach (CommZone z in node.CommZones)
                        {
                            if (z.GetComponent<BoxCollider>().bounds.Contains(meshNodes[i].transform.position))
                            {
                                foundNode = meshNodes[i];
                                Debug.Log("Found Node: " + foundNode.gameObject.name);
                                break;
                            }
                        }
                        node.ConnectToNode(meshNodes[i]);
                    }
                }
            }
        }
        return NetworkPaths;
    }

    public void CalculateStartConnectivity()
    {
        List<MeshNode> meshNodes = NetSysManager.NetworkSys.GetNodes<MeshNode>(NetworkID);
        _cachedMeshPoints = meshNodes;
        if(NetworkPaths == null)
        {
            NetworkPaths = new List<NetworkPath>();
        }
        NetworkPaths.Clear();
        for(int i = 0; i < meshNodes.Count; i++)
        {
            if (WireConnected.Contains(meshNodes[i]))
            {
                ConnectToNode(meshNodes[i]);
                Debug.Log(gameObject.name + " connect to wire" + meshNodes[i].name);
                continue;
            }
            if (this != meshNodes[i] && IsRepeating)
            {
                if (!UseZones)
                {
                    if (Range > Vector3.Distance(transform.position, meshNodes[i].transform.position))
                    {
                        if (MeshConnectionCheck(meshNodes[i]))
                        {
                            ConnectToNode(meshNodes[i]);
                        }
                    }
                }
                else
                {
                    Debug.Log("Entered here");
                    MeshNode foundNode = null;
                    foreach (CommZone z in CommZones)
                    {
                        if (z.GetComponent<BoxCollider>().bounds.Contains(meshNodes[i].transform.position))
                        {
                            foundNode = meshNodes[i];
                            Debug.Log("Found Node: " + foundNode.gameObject.name);
                            break;
                        }
                    }
                    ConnectToNode(meshNodes[i]);
                }
            }
        }
    }
    
    public bool MeshConnectionCheck(MeshNode target)
    {
        int layerMask = 1 << 12;
        layerMask = ~layerMask;
        if (Physics.Linecast(transform.position, target.transform.position, layerMask))
        {
            return false;
        }
        return true;
    }

    private void ConnectToNode(MeshNode target)
    {
        if(NetworkPaths == null)
        {
            NetworkPaths = new List<NetworkPath>();
        }

        NetworkPath connection = new NetworkPath();
        connection.Source = this;
        connection.Destination = target;
        connection.Impedance = 0;
        connection.Capacity = 1;

        NetworkPaths.Add(connection);
    }

    public bool CanConnectWithPoint(Vector3 point)
    {
        if(Vector3.Distance(transform.position, point) < Range)
        {

        }
        return false;
    }

    public override GameObject CreateNode()
    {
        throw new NotImplementedException();
    }

    public void DestroyMeshNode()
    {
        NetSysManager.NetworkSys.RemoveFromNetwork(this);
        ComputeLocalConnectivity();
        Destroy(gameObject);
    }

    public void AddAllNodesToNetwork()
    {
        MeshNode[] nodes = FindObjectsOfType<MeshNode>();
        NetworkSystemManager networkManager = FindObjectOfType<NetworkSystemManager>();
        if (networkManager == null)
        {
            networkManager = transform.root.gameObject.AddComponent<NetworkSystemManager>();
            networkManager.NetworkSys = new NetworkSystem();
        }
        foreach (MeshNode node in nodes)
        {
            if (!networkManager.NetworkSys._Nodes.Contains(node))
            {
                networkManager.NetworkSys.AddToNetwork(node);
            }
        }
    }

    void Visualize()
    {
        if (!_VisCreated)
        {
            Vector3 pos = transform.position;
            pos.y = pos.y - 0.5f;
            Vector3 dir = new Vector3(0, 0, 1);
            Ray castingRay = new Ray(pos, dir);
            RaycastHit hit;
            GameObject visObj = new GameObject();
            visObj.transform.position = transform.position;

            Mesh mesh = visObj.AddComponent<MeshFilter>().mesh;
            MeshRenderer mr = visObj.AddComponent<MeshRenderer>();
            mr.material = VisualMaterial;
            mr.material.color = VisualColor;
            List<Vector3> vertList = new List<Vector3>();
            List<int> triList = new List<int>();
            mesh.Clear();
            vertList.Add(pos);
            int layerMask = 1 << 20;
            layerMask = ~layerMask;

            for (int i = 0; i < 720; i++)
            {
                float angle = i / 2;
                dir = Vector3.Normalize(Quaternion.Euler(0, angle, 0) * Vector3.forward);
                castingRay.direction = dir;
                if (Physics.Raycast(castingRay, out hit, Range, layerMask))
                {
                    vertList.Add(hit.point);
                }
                else
                {
                    vertList.Add(pos + Range * dir);
                }
                if (i > 0)
                {
                    triList.Add(0);
                    triList.Add(i - 1);
                    triList.Add(i);
                }
            }
            //Final tri in the circle
            triList.Add(0);
            triList.Add(vertList.Count - 1);
            triList.Add(1);

            for (int i = 0; i < vertList.Count; i++)
            {
                vertList[i] = vertList[i] - pos;
            }

            mesh.vertices = vertList.ToArray();
            mesh.triangles = triList.ToArray();
            mesh.RecalculateNormals();
            _VisCreated = true;
            VisualizationRenderer = mr;
        }
        else
        {
            VisualizationRenderer.enabled = !VisualizationRenderer.enabled;
        }
    }

    private void OnDrawGizmos()
    {
        if (ShowRangeDebug)
        {
            Gizmos.DrawWireSphere(transform.position, Range);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.25f, 0.25f, 0.25f));
        Gizmos.color = Color.blue;
        if (NetworkPaths != null)
        {
            foreach (NetworkPath path in NetworkPaths)
            {
                Gizmos.DrawLine(path.Source.transform.position, path.Destination.transform.position);
            }
        }
    }

    public void UpdateActorList()
    {
        List<ActorHost> ActorsToRemove = new List<ActorHost>();
        foreach (ActorHost host in ActorsInNode)
        {
            bool foundHost = false;
            foreach (TrackingZone zone in Zones)
            {
                if (zone.ActorsInZone.Contains(host))
                {
                    foundHost = true;
                    if (!ActorsInNode.Contains(host))
                    {
                        ActorsInNode.Add(host);
                    }
                    break;
                }
            }
            if (!foundHost)
            {
                //ActorsInNode.Remove(host);
                ActorsToRemove.Add(host);
            }
        }
        foreach (ActorHost act in ActorsToRemove)
        {
            ActorsInNode.Remove(act);
        }
        //TManager.ActivateClosestNodes();
    }
}
