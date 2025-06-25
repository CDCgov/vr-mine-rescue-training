using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentalMeshNode : NetworkNode {

    //private vars
    private List<Vector3> _meshPositions;
    private List<MeshNode> _cachedMeshPoints;
    private bool _VisCreated = false;
    private Renderer VisualizationRenderer;
    private Renderer LongRange;
    private Renderer _LongRange2;

    //public vars
    public float SatisfactoryCoverageRange = 183;
    public float Range = 610;
    public List<MeshNode> WireConnected;
    public List<HandheldRadio> Radios;
    public NetworkSystemManager NetSysManager;
    public bool debug = false;
    public Color VisualColor;
    public Material VisualMaterial;
    public bool ShowRangeDebug = false;
    public int NumberOfReflections = 0;


    // Use this for initialization
    void Start()
    {

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

        if (WireConnected == null)
        {
            WireConnected = new List<MeshNode>();
        }

        if (NetSysManager.NetworkSys == null)
        {
            NetSysManager.NetworkSys = new NetworkSystem();
            AddAllNodesToNetwork();
        }
        if (Radios == null)
        {
            Radios = new List<HandheldRadio>();
        }
        HandheldRadio[] radios = GameObject.FindObjectsOfType<HandheldRadio>();
        for (int i = 0; i < radios.Length; i++)
        {
            if (radios[i].NetworkID == NetworkID)
            {
                if (!Radios.Contains(radios[i]))
                {
                    Radios.Add(radios[i]);
                }
            }
        }

        CalculateStartConnectivity();

    }

    // Update is called once per frame
    void Update()
    {
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

    private void SendMessageThroughNodes(string message, NetworkNode current, List<HandheldRadio> radios, ref List<NetworkNode> visitedNodes)
    {
        visitedNodes.Add(current);
        List<HandheldRadio> radCopy = new List<HandheldRadio>(radios);
        if (current.GetType() != typeof(MeshToLeakyFeeder))
        {
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
            if (Physics.Linecast(transform.position, pos, layerMask))
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
        foreach (MeshNode node in meshNodes)
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
                    if (node.Range > Vector3.Distance(node.transform.position, meshNodes[i].transform.position))
                    {
                        if (node.MeshConnectionCheck(meshNodes[i]))
                        {
                            //node.ConnectToNode(meshNodes[i]);
                        }
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
        if (NetworkPaths == null)
        {
            NetworkPaths = new List<NetworkPath>();
        }
        NetworkPaths.Clear();
        for (int i = 0; i < meshNodes.Count; i++)
        {
            if (WireConnected.Contains(meshNodes[i]))
            {
                ConnectToNode(meshNodes[i]);
                Debug.Log(gameObject.name + " connect to wire" + meshNodes[i].name);
                continue;
            }
            if (this != meshNodes[i] && IsRepeating)
            {
                if (Range > Vector3.Distance(transform.position, meshNodes[i].transform.position))
                {
                    if (MeshConnectionCheck(meshNodes[i]))
                    {
                        ConnectToNode(meshNodes[i]);
                    }
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
        if (NetworkPaths == null)
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
        if (Vector3.Distance(transform.position, point) < Range)
        {

        }
        return false;
    }

    public override GameObject CreateNode()
    {
        GameObject go = new GameObject();
        return go;
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
            GameObject satisfactory = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(satisfactory.GetComponent<BoxCollider>());
            satisfactory.transform.localScale = new Vector3(SatisfactoryCoverageRange, 0.01f, SatisfactoryCoverageRange);
            satisfactory.transform.position = transform.position;
            satisfactory.transform.parent = transform;
            satisfactory.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.1f);
            VisualizationRenderer = satisfactory.GetComponent<Renderer>();

            GameObject longRange = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(longRange.GetComponent<BoxCollider>());
            longRange.transform.localScale = new Vector3(10, 0.01f, Range);
            longRange.transform.position = transform.position;
            longRange.transform.parent = transform;
            LongRange = longRange.GetComponent<Renderer>();
            LongRange.material.color = new Color(0, 1, 0, 0.1f);

            GameObject longRange2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(longRange2.GetComponent<BoxCollider>());
            longRange2.transform.localScale = new Vector3(Range, 0.01f, 10);
            longRange2.transform.position = transform.position;
            longRange2.transform.parent = transform;
            _LongRange2 = longRange.GetComponent<Renderer>();
            _LongRange2.material.color = new Color(0, 1, 0, 0.1f);

            _VisCreated = true;
        }
        else
        {
            VisualizationRenderer.enabled = !VisualizationRenderer.enabled;
            LongRange.enabled = !LongRange.enabled;
            _LongRange2.enabled = !_LongRange2.enabled;
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
}
