using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_MineCreation;

public class MineDoor : MonoBehaviour
{
    public bool connected = false;
    private Transform connectedTo;
    private PlacablePrefab parentPlacable;

    private void Start()
    {
        transform.parent.GetComponent<PlacablePrefab>().OnPlaced += SetConnected;
        parentPlacable = transform.parent.GetComponent<PlacablePrefab>();
    }

    /*private void OnTriggerEnter(Collider other)
    {
        
        //Debug.Log("trig enter");
        if(other.transform.name == "MineDoor")
        {
            connected = true;
            //connectedTo = other.transform;
            //transform.parent.GetComponent<PlacablePrefab>().SetPlaced();
        }
    }*/

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == connectedTo && connected)
        {
            connected = false;
            connectedTo = null;
            //transform.parent.GetComponent<PlacablePrefab>().UnPlace();
        }
    }

    public void ConnectToDoor(Transform connectedDoor)
    {
        connectedTo = connectedDoor;
        connected = true;
    }

    private void SetConnected(bool isPlaced)
    {
        if(isPlaced && connected)
        {
            GetComponent<MeshRenderer>().enabled = false;
            connectedTo.GetComponent<MeshRenderer>().enabled = false;
            connectedTo.GetComponent<MineDoor>().connected = true;
        }
        else if (!isPlaced && connectedTo != null)
        {
            GetComponent<MeshRenderer>().enabled = true;
            connectedTo.GetComponent<MeshRenderer>().enabled = true;
            connectedTo.GetComponent<MineDoor>().connected = false;
            connectedTo.GetComponent<MineDoor>().connectedTo = null;
            connectedTo = null;
            connected = false;
        }
    }

    public bool IsConnected()
    {
        return connected;
    }
}
