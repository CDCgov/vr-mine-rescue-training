using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverrideTransformParent : MonoBehaviour
{
    public Transform NewParent = null;
    public bool ResetScale = true;
    
    
    void Start()
    {
        transform.SetParent(null, true);
        if (ResetScale)
            transform.localScale = Vector3.one;

    }

}
