using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LinkLinePlayerHandler : MonoBehaviour
{
    public LinkLineObject _HeldLinkLine;
    public XRDirectInteractor _DirInt;
    public XRSocketInteractor _SockInt;
    public TeleportController TeleportCont;
    
    
    //public void OnPickupLifeline(XRDirectInteractor directInteractor)
    //{
    //    _HeldLinkLine = directInteractor.selectTarget.GetComponent<LinkLineObject>();
    //    if(_HeldLinkLine != null)
    //    {
    //        //_HeldLinkLine.LinkPlayerHandle = this;
    //        _DirInt = directInteractor;
    //        TeleportCont.LinkLineAttached();
    //    }        
    //}

    //public void SetSocket(XRSocketInteractor socket)
    //{
    //    _SockInt = socket;
    //    _HeldLinkLine = socket.selectTarget.GetComponent<LinkLineObject>();
    //    if(_DirInt != null)
    //    {
    //        RemoveLinkFromInteractor(_DirInt);
    //        TeleportCont.LinkLineAttached();
    //    }
    //}

    //public void RemoveLinkFromInteractor(XRDirectInteractor directInteractor)
    //{        
    //    if (_DirInt != null)
    //    {
    //        _DirInt.allowSelect = true;
    //        _DirInt = null;
    //    }
    //}

    //public void AutoDropLinkLine(LinkLineObject link)
    //{
    //    Debug.Log("In auto drop");
    //    if(link == _HeldLinkLine)
    //        StartCoroutine(DropLinkLineRoutine());
    //}

    //public IEnumerator DropLinkLineRoutine()
    //{
    //    Debug.Log("SHould have dropped");
    //    if (_DirInt != null)
    //    {
    //        _DirInt.allowSelect = false;
    //        Debug.Log(_DirInt.name);
    //    }
    //    if (_SockInt != null)
    //    {
    //        _SockInt.socketActive = false;
    //        Debug.Log(_SockInt.name);
    //    }
    //    yield return new WaitForSeconds(0.1f);
    //    if (_DirInt != null)
    //    {
    //        _DirInt.allowSelect = true;
    //        _DirInt = null;
    //    }
    //    if (_SockInt != null)
    //    {
    //        _SockInt.socketActive = true;
    //        _SockInt = null;
    //    }
    //    if (_HeldLinkLine != null)
    //    {
    //        _HeldLinkLine.transform.position = _HeldLinkLine.LinkLineBasePoint.position;
    //        _HeldLinkLine.transform.rotation = _HeldLinkLine.LinkLineBasePoint.rotation;
    //        _HeldLinkLine = null;
    //    }
    //    TeleportCont.LinkLineDetached();
    //}

    //public void DropLinkLine(LinkLineObject link)
    //{
    //    if(_HeldLinkLine == link)
    //    {
    //        _HeldLinkLine = null;
    //        TeleportCont.LinkLineDetached();
    //        _DirInt = null;
    //        _SockInt = null;            
    //    }
    //}
}
