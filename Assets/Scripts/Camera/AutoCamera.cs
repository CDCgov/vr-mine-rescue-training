using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCamera : MonoBehaviour
{
    public SystemManager SystemManager;

    void Awake()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
    }

    // Use this for initialization
    void Start()
    {
        var cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.enabled = false;
        }

        var camObj = SystemManager.CreateCamera();
        camObj.transform.SetParent(transform, false);

    }

}
