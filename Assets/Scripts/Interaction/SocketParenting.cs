using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Obsolete]
public class SocketParenting : MonoBehaviour
{
    private Transform _oldParent;

    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor Socket;

    //private void Start()
    //{
    //    if(Socket == null)
    //    {
    //        Socket = gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
    //    }
    //}
    //public void OnSocket()
    //{
    //    _oldParent = Socket.selectTarget.transform.parent;
    //    Socket.selectTarget.transform.parent = Socket.transform;
    //}

    //public void OnDetach()
    //{        
    //    Socket.selectTarget.transform.parent = _oldParent;
    //}
}
