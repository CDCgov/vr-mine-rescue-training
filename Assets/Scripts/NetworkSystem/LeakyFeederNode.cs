using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LeakyFeederNodePair
{
    public LeakyFeederNode node1;
    public LeakyFeederNode node2;
}

public class LeakyFeederNode : NetworkNode {
    //Private Variables
    private string[] _message = { "Get", "to", "the", "mine", "portal!" };
    private float _delay = 0;
    private int _messageIndex = 0;
    private bool _bDrawDebugLine = false;
    private Vector3 _v3DbLineStart = Vector3.zero;
    private Vector3 _v3DbLineEnd = Vector3.zero;
    private bool _VisCreated;
    private MeshRenderer _VisualizationRenderer;

    //Public Variables
    public bool UseCommZones = false;
    public float Range = 30;
    public float Strength = 1;
    public List<HandheldRadio> Radios;

    public List<LeakyFeederNode> PriorNodes;
    public List<LeakyFeederNode> NextNodes;
    public List<CommZone> Zones;

    public bool IsTransmitSource;

    // Use this for initialization
    void Start () {
        ComputeLocalConnectivity();

        AddAllRadiosInScene();

        if(UseCommZones && Zones.Count == 0)
        {
            Zones = new List<CommZone>();
            for (int i=0; i < transform.childCount; i++)
            {
                CommZone z = transform.GetChild(i).GetComponent<CommZone>();
                if (z != null)
                {
                    Zones.Add(z);
                }
            }
        }

    }
    
    // Update is called once per frame
    void Update () {
        //if (IsTransmitSource)
        //{
        //    if (Time.time > _delay)
        //    {
        //        SendToRadios(Time.time.ToString() + ": " + _message[_messageIndex]);
        //        _messageIndex++;
        //        if(_messageIndex >= _message.Length)
        //        {
        //            _messageIndex = 0;
        //        }
        //        _delay = Time.time + 1;
        //    }
        //}
        if (_bDrawDebugLine)
        {
            Debug.DrawLine(_v3DbLineStart, _v3DbLineEnd, Color.red);
        }
    }

    public override List<NetworkPath> ComputeLocalConnectivity()
    {
        if (NetworkPaths == null)
        {
            NetworkPaths = new List<NetworkPath>();
        }
        else
        {
            NetworkPaths.Clear();
        }
        

        if (PriorNodes != null)
        {
            foreach (LeakyFeederNode node in PriorNodes)
            {
                NetworkPath priorPhonePath = new NetworkPath();
                priorPhonePath.Source = this;
                priorPhonePath.Destination = node;
                priorPhonePath.Impedance = 0;
                priorPhonePath.Capacity = 1;
                NetworkPaths.Add(priorPhonePath);
                //Bidirectional
                priorPhonePath.Source = node;
                priorPhonePath.Destination = this;
                priorPhonePath.Impedance = 0;
                priorPhonePath.Capacity = 1;
                NetworkPaths.Add(priorPhonePath);
            }
        }
        if (NextNodes != null)
        {
            foreach (LeakyFeederNode node in NextNodes)
            {
                NetworkPath nextPhonePath = new NetworkPath();
                nextPhonePath.Source = this;
                nextPhonePath.Destination = node;
                nextPhonePath.Impedance = 0;
                nextPhonePath.Capacity = 1;
                NetworkPaths.Add(nextPhonePath);

                nextPhonePath.Source = node;
                nextPhonePath.Destination = this;
                nextPhonePath.Impedance = 0;
                nextPhonePath.Capacity = 1;
                NetworkPaths.Add(nextPhonePath);
            }
        }
        return NetworkPaths;
    }

    public override GameObject CreateNode()
    {
        GameObject nodeGameObject = new GameObject();
        LeakyFeederNode node = nodeGameObject.AddComponent<LeakyFeederNode>();
        nodeGameObject.transform.position = transform.position;

        node.NetworkID = NetworkID;
        node.NetworkSys = NetworkSys;
       
        nodeGameObject.transform.parent = transform.root;
        nodeGameObject.name = "Leaky Node " + transform.root.childCount;

        if (node.PriorNodes == null)
        {
            node.PriorNodes = new List<LeakyFeederNode>();
        }
        node.PriorNodes.Add(this);
        if (NextNodes == null)
        {
            NextNodes = new List<LeakyFeederNode>();
        }
        NextNodes.Add(node);

        NetworkSystemManager networkManager = GameObject.FindObjectOfType<NetworkSystemManager>();
        if (networkManager == null)
        {
            networkManager = transform.root.gameObject.AddComponent<NetworkSystemManager>();
            networkManager.NetworkSys = new NetworkSystem();
        }

        //This in place for the first node added to the scene, make sure it gets added to the network! Does nothing if it's already in there
        if (NetworkSys == null)
        {
            NetworkSys = networkManager.NetworkSys;
            NetworkSys.AddToNetwork(this);

        }
        if (NetworkPaths == null)
        {
            NetworkPaths = new List<NetworkPath>();
        }
        //Adds new node to the network system
        node.NetworkSys = networkManager.NetworkSys;
        NetworkSys.AddToNetwork(node);
        node.NetworkPaths = new List<NetworkPath>();
        //Adds a path to the newly created node
        NetworkPath newPath = new NetworkPath();
        newPath.Destination = node;
        newPath.Source = this;
        newPath.Impedance = 0;
        newPath.Capacity = 1;
        NetworkPaths.Add(newPath);

        //Create a return path (in this case, phone is bi-directional)
        newPath.Source = node;
        newPath.Destination = this;
        newPath.Impedance = 0;
        newPath.Capacity = 1;        

        return nodeGameObject;
    }

    /// <summary>
    /// Create node using Hanging Cable system
    /// </summary>
    /// <param name="prior">Reference to the node that spawned this new node</param>
    /// <param name="root">The transform of the object holding all leaky feeder nodes</param>
    /// <returns></returns>
    public GameObject CreateNode(ref List<LeakyFeederNode> nodeList, int priorIndex,Transform root)
    {
        GameObject nodeGameObject = new GameObject();
        LeakyFeederNode node = nodeGameObject.AddComponent<LeakyFeederNode>();
        
        //nodeGameObject.transform.position = prior.transform.position;

        node.NetworkID = nodeList[priorIndex].NetworkID;
        node.NetworkSys = nodeList[priorIndex].NetworkSys;

        nodeGameObject.transform.parent = root;
        nodeGameObject.name = "Leaky Node " + root.childCount;

        if (node.PriorNodes == null)
        {
            node.PriorNodes = new List<LeakyFeederNode>();
        }
        node.PriorNodes.Add(nodeList[priorIndex]);

        if (nodeList[priorIndex].NextNodes == null)
        {
            nodeList[priorIndex].NextNodes = new List<LeakyFeederNode>();
        }
        nodeList[priorIndex].NextNodes.Add(node);

        NetworkSystemManager networkManager = FindObjectOfType<NetworkSystemManager>();
        if (networkManager == null)
        {
            networkManager = transform.root.gameObject.AddComponent<NetworkSystemManager>();
            networkManager.NetworkSys = new NetworkSystem();
        }
        // This in place for the first node added to the scene, make sure it gets added to the network!Does nothing if it's already in there
        if (NetworkSys == null)
        {
           NetworkSys = networkManager.NetworkSys;
           NetworkSys.AddToNetwork(nodeList[priorIndex]);
        }
        if (nodeList[priorIndex].NetworkPaths == null)
        {
            nodeList[priorIndex].NetworkPaths = new List<NetworkPath>();
        }

        //Adds new node to the network system
        node.NetworkSys = networkManager.NetworkSys;
        NetworkSys.AddToNetwork(node);
        node.NetworkPaths = new List<NetworkPath>();
        //Adds a path to the newly created node
        NetworkPath newPath = new NetworkPath();
        newPath.Destination = node;
        newPath.Source = nodeList[priorIndex];
        newPath.Impedance = 0;
        newPath.Capacity = 1;
        nodeList[priorIndex].NetworkPaths.Add(newPath);

        //Create a return path (in this case, phone is bi-directional)
        newPath.Source = node;
        newPath.Destination = nodeList[priorIndex];
        newPath.Impedance = 0;
        newPath.Capacity = 1;
        node.NetworkPaths.Add(newPath);

        nodeList.Add(node);
        return nodeGameObject;
    }

    public override bool InRange(Vector3 point)
    {
        if (UseCommZones)
        {
            foreach(CommZone zone in Zones)
            {
                Collider col = zone.GetComponent<BoxCollider>();
                if (col.bounds.Contains(point))
                {
                    Strength = zone.ZoneStrength;
                    return true;
                }                
            }
            return false;
        }
        else
        {
            List<LeakyFeederNode> check = new List<LeakyFeederNode>();
            List<LeakyFeederNodePair> pairsInRange = new List<LeakyFeederNodePair>();

            CheckPaths(this, point, ref check, ref pairsInRange);

            if (pairsInRange.Count == 0)
            {
                _bDrawDebugLine = false;
                return false;
            }
            else
            {
                //Debug.Log(pairsInRange.Count + "," + Time.time);
            }
            //bool lineOfSight = false;
            foreach (LeakyFeederNodePair pair in pairsInRange)
            {
                Vector3 dir = Vector3.Normalize(pair.node2.transform.position - pair.node1.transform.position);
                for (int i = 0; i < 50; i++)
                {
                    float scalarMult = (i * 0.02f) * Vector3.Magnitude(pair.node2.transform.position - pair.node1.transform.position);
                    Vector3 start = pair.node1.transform.position + (scalarMult * dir);
                    int layerMask = 1 << 12;
                    layerMask = ~layerMask;

                    if (!Physics.Linecast(point, start, layerMask))
                    {
                        if (Vector3.Distance(point, start) < Range)
                        {
                            //Debug.DrawLine(start, point, Color.red);
                            _bDrawDebugLine = true;
                            _v3DbLineStart = start;
                            _v3DbLineEnd = point;
                            return true;//YES
                        }
                    }
                }
            }
            _bDrawDebugLine = false;
            return false;
        }
    }

    public override void SendToReceivers(string message)
    {
        //foreach(HandheldRadio radio in Radios)
        //{
        //    if (InRange(radio.transform.position))
        //    {
        //        radio.ReceiveMessage(message);
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
        foreach (HandheldRadio rad in radios)
        {
            if (current.InRange(rad.transform.position))
            {
                if (Strength < 1)
                {
                    Debug.Log("In here lolololol");
                    //Change characters in the message at random based on the signal srength. (For each character, if my random number is > strength val, I make it a random digit
                    //UnityEngine.Random r = new UnityEngine.Random();
                    System.Text.StringBuilder builder = new System.Text.StringBuilder(message);
                    for(int i = 0; i < builder.Length; i++)
                    {
                        if(UnityEngine.Random.Range(0,1) > Strength)
                        {
                            builder[i] = (char)(UnityEngine.Random.Range(65, 122) % 255);
                        }                        
                    }
                    rad.ReceiveMessage(builder.ToString());
                }
                else
                {
                    rad.ReceiveMessage(message);
                }
                //Debug.DrawLine(current.transform.position, rad.transform.position);
                radCopy.Remove(rad);
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

    private void CheckPaths(LeakyFeederNode current, Vector3 point,ref List<LeakyFeederNode> check, ref List<LeakyFeederNodePair> pairsInRange)
    {

        foreach (NetworkPath path in current.NetworkPaths)
        {
            if (path.Destination.GetType() == typeof(LeakyFeederNode))
            {
                LeakyFeederNode dest = (LeakyFeederNode)path.Destination;
                if (!check.Contains(dest))
                {
                    check.Add(dest);
                    Vector3 pos1 = current.transform.position;
                    Vector3 pos2 = dest.transform.position;
                    Vector3 dir = (pos2 - pos1);
                    //Vector3 center = (pos1 + pos2) / 2f;

                    //Check if point of interest is within sphere of range at Node points
                    float sphereDist1 = (pos1 - point).sqrMagnitude;
                    float sphereDist2 = (pos2 - point).sqrMagnitude;
                    //float rangeSquare1 = Range * Range;
                    if (sphereDist1 <= Range * Range)
                    {
                        LeakyFeederNodePair pairInRange = new LeakyFeederNodePair();
                        pairInRange.node1 = current;
                        pairInRange.node2 = dest;

                        if (!pairsInRange.Contains(pairInRange))
                        {
                            pairsInRange.Add(pairInRange);
                        }

                        //return true; //maybe return the current node somehow as well? or maybe just return the node else the whole thing returns null
                    }
                    if (sphereDist2 <= dest.Range * dest.Range)
                    {
                        LeakyFeederNodePair pairInRange = new LeakyFeederNodePair();
                        pairInRange.node1 = current;
                        pairInRange.node2 = dest;
                        if (!pairsInRange.Contains(pairInRange))
                        {
                            pairsInRange.Add(pairInRange);
                        }
                        //return true; //maybe return the current node somehow as well? or maybe just return the node else the whole thing returns null
                    }


                    Vector3 pDir = point - pos1;
                    float dot = Vector3.Dot(dir, pDir);
                    float lengthSq = dir.sqrMagnitude;

                    if (dot < 0f || dot > lengthSq)
                    {
                        // return false; //Continue iterating rather than return false
                        //DO NOTHING
                    }
                    else
                    {
                        float dSq = pDir.x * pDir.x + pDir.y * pDir.y + pDir.z + pDir.z - dot * dot / lengthSq;

                        if (dSq > Range * Range)
                        {
                            //return false;
                            //DO NOTHING
                        }
                        else
                        {
                            //return true;
                            LeakyFeederNodePair pairInRange = new LeakyFeederNodePair();
                            pairInRange.node1 = current;
                            pairInRange.node2 = dest;
                            if (!pairsInRange.Contains(pairInRange))
                            {
                                pairsInRange.Add(pairInRange);
                                //Debug.Log(dSq);
                            }
                        }
                    }
                    CheckPaths(dest, point, ref check, ref pairsInRange);
                }
            }
        }
    }

    public void AddAllRadiosInScene()
    {
        HandheldRadio[] radios = FindObjectsOfType<HandheldRadio>();
        if(Radios == null)
        {
            Radios = new List<HandheldRadio>();
        }
        foreach(HandheldRadio rad in radios)
        {
            if (!Radios.Contains(rad) && rad.NetworkID == NetworkID)
            {
                Radios.Add(rad);
            }
            else
            {
                List<HandheldRadio> copy = new List<HandheldRadio>();
                for (int i = 0; i < Radios.Count; i++)
                {
                    //if()
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
     
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));
    }
    //Gizmos.color = Color.green;
    //Gizmos.DrawWireCube(transform.position, new Vector3(0.25f, 0.25f, 0.25f));

    //Gizmos.color = Color.yellow;
    //if (NetworkPaths != null)
    //{
    //    foreach (NetworkPath path in NetworkPaths)
    //    {
    //        if (path.Source == null)
    //        {
    //            Debug.Log("Source null: " + path.Source.name);
    //        }
    //        if (path.Destination == null)
    //        {
    //            Debug.Log("Dest null: " + path.Destination.name);
    //        }
    //        Gizmos.DrawLine(path.Source.transform.position, path.Destination.transform.position);
    //    }
    //}


    public void AddHangingCableNodes(Transform creator, GameObject initialNode, HangingCable cable)
    {
        //HangingCable cable = GetComponent<HangingCable>();
        int nodeCount = 0;
        GameObject lfParent = new GameObject();
        lfParent.name = "LeakyFeederNodes";
        lfParent.transform.parent = creator;
        lfParent.transform.localPosition = Vector3.zero;
        initialNode.transform.parent = lfParent.transform;
        if(cable != null)
        {
            List<HangingGeometry.CableNode> nodes = cable.CableNodes;
            foreach(HangingGeometry.CableNode node in nodes)
            {
                GameObject lfNode = CreateNode();
                lfNode.transform.position = node.Position;
                lfNode.name = "LeakyFeeder_" + nodeCount.ToString("00");
                lfNode.transform.parent = lfParent.transform;
                nodeCount++;
            }
        }
    }
}
