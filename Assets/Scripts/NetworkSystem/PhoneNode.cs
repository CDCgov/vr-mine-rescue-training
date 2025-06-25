using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum PhoneNodeType
{
    Node,    
    PagePhone,
    DialPhone
}

/// <summary>
/// Extension of Network Node to handle specific behaviors for mine phones
/// </summary>
public class PhoneNode : NetworkNode {

    public PhoneNodeType PhoneType = PhoneNodeType.Node;
    public bool IsSimplePhone = false;

    [Header("GUI Handles for Pager")]
    public GameObject GuiPhone;
    public InputField MessageInput;
    public Text ReceivedMessageLabel;
   

    [HideInInspector]
    public bool PickedUp = false;    
    
    [Header("Connectivity")]
    public List<PhoneNode> PriorPhones;
    public List<PhoneNode> NextPhones;

    //To handle the situation when a non-command phone picks up, a direct line is formed between it and command, others can join the party line
    private List<PhoneNode> _connectedPhones;
    private static List<PhoneNode> _AllPhones;
    private PhoneNode target;
    private bool _BroadcastMode;

    private GameObject _pointLight;
    private AudioSource _auSrc;

    
    // Use this for initialization
    void Start () {		
        
        ComputeLocalConnectivity();

        
        if(PhoneType != PhoneNodeType.Node)
        {
            //_pointLight = GetComponentInChildren<Light>().gameObject;
            //if(_pointLight != null)
            //{
            //    _pointLight.SetActive(false);
            //}
            _auSrc = GetComponent<AudioSource>();
        }     
    }
    
    // Update is called once per frame
    void Update () {
        if (_auSrc != null && _auSrc.isPlaying)
        {
            if (_pointLight != null)
            {
                _pointLight.SetActive(true);
            }
        }
        else
        {
            if (_pointLight != null)
            {
                _pointLight.SetActive(false);
            }
        }
        if (Input.GetKeyUp(KeyCode.Return))
        {
            Broadcast("I hit the enter key!");
        }

        if (Input.GetKeyUp(KeyCode.Alpha2) && gameObject.name == "Phone2")
        {
            AnswerPhone();
            SendCall("Phone 2 responding to command");
            HangUp();
        }

        if (Input.GetKeyUp(KeyCode.Alpha3) && gameObject.name == "Phone3")
        {
            AnswerPhone();
            SendCall("Phone 3 responding to command");
            HangUp();
        }
    }

    /// <summary>
    /// Temporary behavior to demo phone pickup behaviors
    /// </summary>
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!IsSimplePhone)
            {
                if (Input.GetKey(KeyCode.LeftControl) && PickedUp)
                {
                    SendCall("Sending a message");
                }
                else
                {
                    PickedUp = !PickedUp;
                    if (GuiPhone != null)
                    {
                        GuiPhone.SetActive(PickedUp);
                    }
                    Renderer ren = GetComponent<Renderer>();
                    if (ren != null)
                    {
                        if (PickedUp)
                        {
                            ren.material.color = Color.blue;
                        }
                        else
                        {
                            ren.material.color = Color.white;
                            if (_auSrc != null)
                            {
                                _auSrc.Stop();
                            }
                        }
                    }
                }
            }
            else
            {
                if(_auSrc != null)
                {
                    if (!_auSrc.isPlaying)
                    {
                        _auSrc.Play();
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (Input.GetKey(KeyCode.LeftControl) && PickedUp)
            {
                Broadcast(gameObject.name + " is Broadcasting!");
            }
            else
            {
                PickedUp = !PickedUp;
                Renderer ren = GetComponent<Renderer>();
                //IsCommandPhone = !IsCommandPhone;
                if (ren != null)
                {
                    if (PickedUp)
                    {
                        ren.material.color = Color.red;
                    }
                    else
                    {
                        ren.material.color = Color.white;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(3))
        {
            if (Input.GetKey(KeyCode.LeftControl) && PickedUp)
            {
                SendCall("Sending a message");
            }
            else
            {
                PickedUp = !PickedUp;
                Renderer ren = GetComponent<Renderer>();
                if (ren != null)
                {
                    if (PickedUp)
                    {
                        ren.material.color = Color.yellow;
                    }
                    else
                    {
                        ren.material.color = Color.white;
                        if (_auSrc != null)
                        {
                            _auSrc.Stop();
                        }
                    }
                }
            }
        }
    }

    public void AnswerPhone()
    {
        //foreach(NetworkPath path in NetworkPaths)
        //{
        //    PhoneNode commandPhone = (PhoneNode)path.Destination;
        //    if(commandPhone != null)
        //    {
        //        if (commandPhone.IsCommandPhone)
        //        {
        //            if(commandPhone._connectedPhones == null)
        //            {
        //                commandPhone._connectedPhones = new List<PhoneNode>();
        //            }
        //            if (!commandPhone._connectedPhones.Contains(this))
        //            {
        //                commandPhone._connectedPhones.Add(this);
        //                _connectedPhones.Add(commandPhone);
        //            }
        //        }

        //    }
        //}
        PickedUp = true;
    }

    public void HangUp()
    {
        //foreach (NetworkPath path in NetworkPaths)
        //{
        //    PhoneNode commandPhone = (PhoneNode)path.Destination;
        //    if (commandPhone != null)
        //    {
        //        if (commandPhone.IsCommandPhone)
        //        {
        //            if (commandPhone._connectedPhones == null)
        //            {
        //                commandPhone._connectedPhones = new List<PhoneNode>();
        //            }
        //            commandPhone._connectedPhones.Remove(this);
        //        }
        //    }
        //}
        //_connectedPhones.Clear();
        if(GuiPhone != null)
        {
            GuiPhone.SetActive(false);
        }
        PickedUp = false;
    }
    
    /// <summary>
    /// Method called at the end point of a call. For now pass string data. Final methods can potentially send information about audio streams?
    /// </summary>
    /// <param name="info"></param>
    public void ReceiveCall(string info)
    {
        Debug.Log(gameObject.name + ", Message received: " + info);
        //AudioSource auSrc = GetComponent<AudioSource>();
        if(_auSrc != null)
        {
            _auSrc.Play();
        }
        if(ReceivedMessageLabel != null)
        {
            ReceivedMessageLabel.text = info;
        }
    }

    /// <summary>
    /// Old method called for sending information
    /// </summary>
    /// <param name="info"></param>
    public void SendCall(string info)
    {
        List<PhoneNode> connectedPhones = new List<PhoneNode>();
        connectedPhones.Add(this);
        SendToAll(this, info, ref connectedPhones, false);
    }

    /// <summary>
    /// UI handle for sending a message
    /// </summary>
    public void SendBtn()
    {
        if (_BroadcastMode)
        {
            Broadcast(MessageInput.text);
            MessageInput.text = "";
        }
        else
        {
            SendCall(MessageInput.text);
            MessageInput.text = "";        
        }
    }

    public void OnBroadcastToggleChanged(bool toggle)
    {
        _BroadcastMode = toggle;
    }

    /// <summary>
    /// Sends call message to all node paths
    /// </summary>
    /// <param name="info"></param>
    public void Broadcast(string info)
    {
        
        if (_BroadcastMode)
        {
            //foreach (NetworkPath path in this.NetworkPaths)
            //{
            //    SendCall(info);
            //}
            //SendCall(info);
            List<PhoneNode> connectedPhones = new List<PhoneNode>();
            connectedPhones.Add(this);

            SendToAll(this, info, ref connectedPhones, true);
        }

        //List<PhoneNode> connectedPhones = new List<PhoneNode>();
        //connectedPhones.Add(this);

        //SendToAll(this, info, ref connectedPhones);
    }

    /// <summary>
    /// Sends a message to all connected phones in the network. Goes through the phone nodes recursively
    /// </summary>
    /// <param name="current">Start point of the call</param>
    /// <param name="info">String information sent out to the network</param>
    /// <param name="connectedPhones">List of phones that have been visited (i.e. connected) in the call</param>
    /// <param name="Broadcast">Is this a broadcasted message?</param>
    private void SendToAll(PhoneNode current, string info, ref List<PhoneNode> connectedPhones, bool Broadcast = true)
    {
        foreach(NetworkPath path in current.NetworkPaths)
        {
            if (!connectedPhones.Contains((PhoneNode)path.Destination))
            {
                PhoneNode dest = (PhoneNode)path.Destination;
                connectedPhones.Add(dest);
                if(dest.PhoneType == PhoneNodeType.PagePhone || dest.PhoneType == PhoneNodeType.DialPhone)
                {
                    if (Broadcast)
                    {
                        dest.ReceiveCall(info);
                    }
                    else if (dest.PickedUp)
                    {
                        dest.ReceiveCall(info);
                    }
                }
                //Continue down the graph, passing in the visited phones
                SendToAll(dest, info, ref connectedPhones, Broadcast);
            }
        }
    }

    /// <summary>
    /// Future method for handling Dial phones
    /// </summary>
    /// <param name="target"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool DialPhone(PhoneNode target, string info)
    {
        bool success = false;
        float imp = 0;
        float cap = 1;
        if(NetworkSys.AreConnected(this, target, out imp, out cap))
        {
            //Replace with a Ringing logic to have someone pick up the phone
            target.ReceiveCall(info);
            success = true;
        }
        else
        {
            success = false;
        }
        return success;
    }

    /// <summary>
    /// Method to add a new Phone to the network. At this time only called from the editor, could be used at runtime later
    /// </summary>
    /// <returns></returns>
    public GameObject AddNewPhone()
    {
        GameObject phoneResource = (GameObject)Resources.Load("MineElements/PagePhone");
        GameObject phone = Instantiate(phoneResource);
        PhoneNode pnScript = phone.GetComponent<PhoneNode>();
        //PhoneNode pnScript = phone.AddComponent<PhoneNode>();
        phone.transform.position = transform.position;
            
        pnScript.NetworkID = NetworkID;
        pnScript.NetworkSys = NetworkSys;
        phone.transform.parent = transform.parent;
        phone.name = "Spawned Phone " + transform.parent.childCount;
        //pnScript.PriorPhone = this;
        //pnScript.PhoneType = PhoneNodeType.PagePhone;
        //NextPhone = pnScript;
        if(pnScript.PriorPhones == null)
        {
            pnScript.PriorPhones = new List<PhoneNode>();
        }
        pnScript.PriorPhones.Add(this);
        if (NextPhones == null)
        {
            NextPhones = new List<PhoneNode>();
        }
        NextPhones.Add(pnScript);

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
            NetworkPaths = new List<NetworkPath>();
        }
        //Adds new node to the network system
        pnScript.NetworkSys = networkManager.NetworkSys;
        NetworkSys.AddToNetwork(pnScript);
        if (pnScript.NetworkPaths == null)
        {
            pnScript.NetworkPaths = new List<NetworkPath>();
        }
        else
        {
            pnScript.NetworkPaths.Clear();
        }
        //Adds a path to the newly created node
        NetworkPath newPath = new NetworkPath();
        newPath.Destination = pnScript;
        newPath.Source = this;
        newPath.Impedance = 0;
        newPath.Capacity = 1;
        NetworkPaths.Add(newPath);

        //Create a return path (in this case, phone is bi-directional)
        newPath.Source = pnScript;
        newPath.Destination = this;
        newPath.Impedance = 0;
        newPath.Capacity = 1;
        NetworkPaths.Add(newPath);
        //phone.AddComponent<AudioSource>();

        return phone;
    }    

    /// <summary>
    /// Creates a new node in network
    /// </summary>
    /// <returns></returns>
    public override GameObject CreateNode()
    {
        GameObject nodeGameObject = new GameObject();
        PhoneNode node = nodeGameObject.AddComponent<PhoneNode>();
        nodeGameObject.transform.position = transform.position;
        
        node.NetworkID = NetworkID;
        node.NetworkSys = NetworkSys;
        //node.PriorPhone = this;
        node.PhoneType = PhoneNodeType.Node;
        //NextPhone = node;
        nodeGameObject.transform.parent = transform.root;
        nodeGameObject.name = "Phone Node " + transform.root.childCount;

        if(node.PriorPhones == null)
        {
            node.PriorPhones = new List<PhoneNode>();
        }
        node.PriorPhones.Add(this);
        if (NextPhones == null)
        {
            NextPhones = new List<PhoneNode>();
        }
        NextPhones.Add(node);

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
        if(NetworkPaths == null)
        {
            NetworkPaths = new List<NetworkPath>();
        }
        //Adds new node to the network system
        node.NetworkSys = networkManager.NetworkSys;
        node.NetworkSys.AddToNetwork(node);
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
    /// Recalculates the node path network. Call this on start, or if a node becomes destroyed.
    /// </summary>
    /// <returns></returns>
    public override List<NetworkPath> ComputeLocalConnectivity()
    {
        //foreach (PhoneNode dest in Destination)
        //{
        //    NetworkPath path = new NetworkPath();
        //    path.Source = this;
        //    path.Destination = dest;
        //    path.Impedance = 0;
        //    path.Capacity = 1;
        //    if(NetworkPaths == null)
        //    {
        //        NetworkPaths = new List<NetworkPath>();
        //    }
        //    this.NetworkPaths.Add(path);
        //}

        if(NetworkSys != null)
        {
            NetworkSys.AddToNetwork(this);
        }
        
        if (NetworkPaths == null)
        {
            NetworkPaths = new List<NetworkPath>();
        }
        else
        {
            NetworkPaths.Clear();
        }
        //if (PriorPhone != null)
        //{
        //    NetworkPath priorPhonePath = new NetworkPath();
        //    priorPhonePath.Source = this;
        //    priorPhonePath.Destination = PriorPhone;
        //    priorPhonePath.Impedance = 0;
        //    priorPhonePath.Capacity = 1;            
        //    NetworkPaths.Add(priorPhonePath);
        //    //Bidirectional
        //    priorPhonePath.Source = PriorPhone;
        //    priorPhonePath.Destination = this;
        //    priorPhonePath.Impedance = 0;
        //    priorPhonePath.Capacity = 1;
        //    NetworkPaths.Add(priorPhonePath);
            
        //}

        if(PriorPhones != null)
        {
            foreach(PhoneNode node in PriorPhones)
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
        if(NextPhones != null)
        {
            foreach (PhoneNode node in NextPhones)
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.25f, 0.25f, 0.25f));
        float colCalc = (NetworkID * 10) / 360f;
        if(colCalc > 1)
        {
            colCalc = 0;
        }
        Color custCol = Color.HSVToRGB(colCalc, 1, 1);
        Gizmos.color = custCol;
        //if (NetworkPaths != null)
        //{
        //    foreach (NetworkPath path in NetworkPaths)
        //    {
        //        if(path.Source == null)
        //        {
        //            Debug.Log("Source null: " + path.Source.name);
        //        }
        //        if(path.Destination == null)
        //        {
        //            Debug.Log("Dest null: " + path.Destination.name);
        //        }
        //        Gizmos.DrawLine(path.Source.transform.position, path.Destination.transform.position);
        //    }
        //}
    }
    public void PlaySimpleMessage()
    {
        if (_auSrc != null)
        {
            if (!_auSrc.isPlaying)
            {
                _auSrc.Play();
            }
        }
    }
}
