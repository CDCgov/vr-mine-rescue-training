using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeParentOnActivation : MonoBehaviour
{
    public Transform NewParent;

    private Transform _origParent;
        

    private void OnEnable()
    {
        if(_origParent == null)
        {
            _origParent = transform.parent;
        }
        if(NewParent != null)
        {
            transform.parent = NewParent;
        }
        else
        {
            transform.parent = null;
        }
    }

    private void OnDisable()
    {
        transform.parent = _origParent;
    }

}
