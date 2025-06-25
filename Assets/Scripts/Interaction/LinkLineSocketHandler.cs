using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LinkLineSocketHandler : MonoBehaviour
{
    public XRSocketInteractor Socket;
    public CustomXRSocket CustomSocket;
    public TeleportController TeleportCont;

    public LinkLineObject _link;

    private bool _isLinked = false;

    //private void Start()
    //{
    //    //CustomXRSocket.OnSocketAttach += CustomXRSocket_OnSocketAttach;
    //    //CustomXRSocket.OnSocketDetach += CustomXRSocket_OnSocketDetach;
    //}

    //private void Update()
    //{
    //    if (CustomSocket != null)
    //    {
    //        if (CustomSocket.SocketedInteractable != null && !_isLinked)
    //        {
    //            _link = CustomSocket.SocketedInteractable.GetComponent<LinkLineObject>();
    //            if (_link != null)
    //            {
    //                _isLinked = true;                    
    //            }
    //        }
    //    }
    //}

    //private void CustomXRSocket_OnSocketAttach(CustomXRSocket xrSocket)
    //{
    //    //LinkLineObject llObj;
    //    if(xrSocket == null)
    //    {
    //        return;
    //    }
    //    if (CustomSocket != null || xrSocket.gameObject != gameObject)
    //    {
    //        return;
    //    }
    //    if (xrSocket.SocketedInteractable.TryGetComponent<LinkLineObject>(out _link))
    //    {
    //        Debug.Log("On attach was called on " + gameObject.name);
    //        _link.SocketHandler = this;
    //        TeleportCont.LinkLineAttached();
    //        CustomSocket = xrSocket;
    //    }
    //}

    //private void CustomXRSocket_OnSocketDetach(CustomXRSocket xrSocket)
    //{
    //    if(xrSocket == null)
    //    {
    //        return;
    //    }
    //    if (_link == null || CustomSocket == null || xrSocket.gameObject != gameObject)
    //    {
    //        return;
    //    }
    //    if (xrSocket.SocketedInteractable != null)
    //    {
    //        if (xrSocket.SocketedInteractable.TryGetComponent<LinkLineObject>(out _link))
    //        {
    //            _link.SocketHandler = null;
    //            _link = null;
    //            Debug.Log("On detach was called on " + gameObject.name);
    //            TeleportCont.LinkLineDetached();
    //            CustomSocket = null;
    //        }
    //    }
    //}

    //public void OnLink()
    //{        
    //    LinkLineObject link = Socket.selectTarget.GetComponent<LinkLineObject>();
    //    if(link != null)
    //    {
    //        _link = link;
    //        _link.SocketInteract = Socket;
    //        _link.SocketHandler = this;
    //        TeleportCont.LinkLineAttached();
    //    }
        
    //}

    //public void OnDetach()
    //{
    //    if(_link != null)
    //    {
    //        _link.SocketInteract = null;
    //        _link.SocketHandler = null;
    //        _link = null;
    //        Debug.Log("On detach was called");
    //        TeleportCont.LinkLineDetached();
    //    }
        
    //}

    //public void AutoDropLinkLine(LinkLineObject link)
    //{
    //    Debug.Log("In auto drop");
    //    if (link == _link)
    //    {
    //        CustomSocket.RequestRemoveSocketedItem();
            
    //        //_link = null;
    //        OnDetach();
    //        //StartCoroutine(DropLinkLineRoutine());
    //    }
    //}

    //public IEnumerator DropLinkLineRoutine()
    //{
    //    Debug.Log("SHould have dropped");
        
    //    if (Socket != null)
    //    {
    //        Socket.socketActive = false;
    //        Debug.Log(Socket.name);
    //    }
    //    yield return new WaitForSeconds(0.25f);
        
    //    if (Socket != null)
    //    {
    //        Socket.socketActive = true;            
    //    }
    //    if (_link != null)
    //    {
    //        _link.transform.position = _link.LinkLineBasePoint.position;
    //        _link.transform.rotation = _link.LinkLineBasePoint.rotation;
    //        _link = null;
    //    }
    //    TeleportCont.LinkLineDetached();
    //}
}
