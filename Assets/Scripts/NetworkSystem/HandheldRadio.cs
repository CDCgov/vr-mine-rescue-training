using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandheldRadio : MonoBehaviour {

    public int NetworkID = 0;
    public float Range = 20;
    public Text LeakyFeederLabel;
    public bool IsSending = false;
    public LeakyFeederNode LeakyFeedRef;

    private LeakyFeederNode _lastLFNode;
    private NetworkSystem _netSys;

    private string[] _message = { "Get", "to", "the", "mine", "portal!" };
    private float _delay = 0;
    private int _messageIndex = 0;
    private NetworkNode _lastNodeInRange;
    public bool _ShowTrackingLocation = false;
    private float _trackingDelay = 0;
    // Use this for initialization
    void Start () {
        if(_netSys == null)
        {
            _netSys = GameObject.FindObjectOfType<NetworkSystemManager>().NetworkSys;
        }
    }
    
    // Update is called once per frame
    void Update () {
        if (IsSending)
        {
            if (Time.time > _delay)
            {                
                if (_lastNodeInRange != null)
                {
                    if (_lastNodeInRange.InRange(transform.position))
                    {                        
                        _lastNodeInRange.SendToReceivers(Time.time.ToString("F2") + ": " + _message[_messageIndex]);
                        MessagePacket();
                    }
                    else
                    {
                        foreach(NetworkNode node in _netSys.GetAllNodesOfID(NetworkID))
                        {
                            if (node.InRange(transform.position) && node.GetType() != typeof(MeshToLeakyFeeder))
                            {
                                _lastNodeInRange = node;
                                node.SendToReceivers(Time.time.ToString("F2") + ": " + _message[_messageIndex]);
                                MessagePacket();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (NetworkNode node in _netSys.GetAllNodesOfID(NetworkID))
                    {
                        if (node.InRange(transform.position) && node.GetType() != typeof(MeshToLeakyFeeder))
                        {
                            _lastNodeInRange = node;
                            node.SendToReceivers(Time.time.ToString("F2") + ": " + _message[_messageIndex]);
                            MessagePacket();
                            break;
                        }
                    }
                }

                //LeakyFeedRef.SendToReceivers(Time.time.ToString() + ": " + _message[_messageIndex]);
                //_messageIndex++;
                //if (_messageIndex >= _message.Length)
                //{
                //    _messageIndex = 0;
                //}
                //_delay = Time.time + 1;
            }
        }

        if (_lastNodeInRange != null && _ShowTrackingLocation)
        {
            if (Time.time > _trackingDelay)
            {
                _lastNodeInRange.ShowTracking(gameObject);
                _trackingDelay = Time.time + 1;
            }
        }
        if (Input.GetKeyUp(KeyCode.Alpha9))
        {
            Debug.Log(_netSys.GetNodeType(0));
        }
        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            if (_lastNodeInRange != null)
            {
                if (_lastNodeInRange.InRange(transform.position))
                {

                }
                else
                {
                    foreach (NetworkNode node in _netSys.GetAllNodesOfID(NetworkID))
                    {

                    }
                }
            }
            else
            {
                foreach (NetworkNode node in _netSys.GetAllNodesOfID(NetworkID))
                {
                    Debug.Log(node.GetType());
                }
            }
        }
    }

    void MessagePacket()
    {
        _messageIndex++;
        if (_messageIndex >= _message.Length)
        {
            _messageIndex = 0;
        }
        _delay = Time.time + 1;
    }

    void CheckLFConnection()
    {
        if (_netSys == null)
            return;
        //float d1 = 0;
        //float d2 = 0;
        //bool nodesNotInRange = false;

        //Get all nodes connected to the last node connected
        List<NetworkPath> lfConnections = _netSys.GetConnections(_lastLFNode);
        List<LeakyFeederNode> lfNodes = new List<LeakyFeederNode>();
        lfNodes.Add(_lastLFNode);
        foreach(NetworkPath path in lfConnections)
        {
            if (!lfNodes.Contains((LeakyFeederNode)path.Destination))
            {
                lfNodes.Add((LeakyFeederNode)path.Destination);
            }
        }
        //Find the node that is closest to the radio
        float distance = Vector3.Distance(transform.position, _lastLFNode.transform.position);
        foreach(LeakyFeederNode node in lfNodes)
        {
            float d = Vector3.Distance(transform.position, node.transform.position);
            if(d < distance)
            {
                distance = d;
                _lastLFNode = node;
            }
        }

        //if(distance > Range)
        //{
        //    nodesNotInRange = true;
        //}
    }



    public void ReceiveMessage(string message)
    {
        if (!IsSending)
        {
            Debug.Log(gameObject.name + " message received: " + message);
        }
        if (LeakyFeederLabel != null && !IsSending)
        {
            LeakyFeederLabel.text = message;
        }
    }

    public void ChangeNetworkID(int id)
    {
        ConnectToNetworkID(id);
    }

    private void ConnectToNetworkID(int networkID)
    {
        
    }

   
}
