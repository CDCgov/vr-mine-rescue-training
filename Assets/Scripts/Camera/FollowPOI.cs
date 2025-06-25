using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPOI : MonoBehaviour
{
    public TeleportManager TeleportManager;

    private Transform _parent;

    // Start is called before the first frame update
    void Start()
    {
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);

        TeleportManager.Teleporting += OnTeleporting;

        var obj = new GameObject($"{gameObject.name}_FollowPOI");

        _parent = obj.transform;
        transform.SetParent(_parent);

        if (TeleportManager.ActiveTeleportTarget != null)
            OnTeleporting(TeleportManager.ActiveTeleportTarget);
    }

    private void OnTeleporting(Transform obj)
    {
        //transform.SetParent(obj, false);
        _parent.position = obj.position;
        _parent.rotation = obj.rotation;
    }

    private void OnDestroy()
    {
        if (TeleportManager != null)
        {
            TeleportManager.Teleporting -= OnTeleporting;
        }
    }

}
