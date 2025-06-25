using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleObjectWhenHeld : MonoBehaviour
{
    public NetworkedObjectManager NetworkedObjectManager;

    public float NormalScale = 1.0f;
    public float HeldScale = 1.5f;

    private NetworkedObject _netObj;
    private Vector3 _scaleCache;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);

        NetworkedObjectManager.ObjectHeldStateChanged += OnObjectHeldStateChanged;

        _netObj = GetComponentInChildren<NetworkedObject>();
        if (_netObj == null)
        {
            Debug.LogError($"Error - ScaleObjectWhenHeld on non-networked object");
        }

        _scaleCache = new Vector3(NormalScale, NormalScale, NormalScale);
    }

    private void OnDestroy()
    {
        if (NetworkedObjectManager != null)
            NetworkedObjectManager.ObjectHeldStateChanged -= OnObjectHeldStateChanged;
    }

    private void OnObjectHeldStateChanged(System.Guid objID)
    {
        if (_netObj == null || objID != _netObj.uniqueID)
            return;

        var objData = NetworkedObjectManager.GetObjectData(objID);
        Vector3 scale;
        
        if (objData.HeldState != null && objData.HeldState.ObjectHeld)
            scale = new Vector3(HeldScale, HeldScale, HeldScale);
        else
            scale = new Vector3(NormalScale, NormalScale, NormalScale);

        transform.localScale = scale;
        _scaleCache = scale;
    }


    //trying this as a quick fix
    private void LateUpdate()
    {
        if(_scaleCache != new Vector3 (NormalScale, NormalScale,NormalScale))
        {
            transform.localScale = _scaleCache;
        }
    }
}
