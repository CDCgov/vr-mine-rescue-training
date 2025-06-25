using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketParenting : MonoBehaviour
{
    private Transform _oldParent;

    public XRSocketInteractor Socket;

    private void Start()
    {
        if(Socket == null)
        {
            Socket = gameObject.GetComponent<XRSocketInteractor>();
        }
    }
    public void OnSocket()
    {
        _oldParent = Socket.selectTarget.transform.parent;
        Socket.selectTarget.transform.parent = Socket.transform;
    }

    public void OnDetach()
    {        
        Socket.selectTarget.transform.parent = _oldParent;
    }
}
