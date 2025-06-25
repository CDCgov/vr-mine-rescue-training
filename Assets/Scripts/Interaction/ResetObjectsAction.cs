using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetObjectsAction : MonoBehaviour, ISelectableObjectAction
{
    public NetworkManager NetworkManager;
    public SocketManager SocketManager;
    public SceneLoadManager SceneLoadManager;

    public string ActionName = "Reset Objects";

    public List<Transform> ObjectsToReset;

    private List<ObjectResetData> _objectData = new List<ObjectResetData>();

    private struct ObjectResetData
    {
        public Transform Object;
        public Vector3 Position;
        public Quaternion Rotation;
        public Transform Parent;
        public CustomXRSocket Socket;
    }

    public string SelectableActionName
    {
        get
        {
            return ActionName;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (SocketManager == null)
            SocketManager = SocketManager.GetDefault(gameObject);
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);


        if (SceneLoadManager.LoadInProgress)
            StartCoroutine(nameof(StoreObjectDataCoroutine));
        else
            StoreObjectData();
        
    }

    private IEnumerator StoreObjectDataCoroutine()
    {
        if (SceneLoadManager == null)
            yield break;

        while (SceneLoadManager.IsLoadInProgress)
            yield return new WaitForEndOfFrame();

        StoreObjectData();
    }

    void StoreObjectData()
    {
        foreach (var xform in ObjectsToReset)
        {
            var objData = StoreObjectData(xform);
            _objectData.Add(objData);
        }
    }

    public void AddObjectToResetList(Transform obj)
    {
        if (_objectData == null)
            _objectData = new List<ObjectResetData>();

        var objData = StoreObjectData(obj);
        _objectData.Add(objData);
    }

    ObjectResetData StoreObjectData(Transform xform)
    {
        //var netObj = xform.GetComponent<NetworkedObject>();
        //if (netObj != null)
        //{

        //}

        ObjectResetData data = new ObjectResetData
        {
            Object = xform,
            Position = xform.localPosition,
            Rotation = xform.localRotation,
            Parent = xform.parent,
            Socket = null,
        };

        if (xform.TryGetComponent<CustomXRInteractable>(out var interact))
        {
            data.Socket = interact.CurrentOwner as CustomXRSocket;
            if (data.Socket != null)
            {
                Debug.Log($"ResetObjectsAction: Found {xform.name} in socket {data.Socket.name}");
            }
        }

        return data;
    }

    void ResetObject(ObjectResetData data)
    {
        if (!NetworkManager.IsServer)
            return;

        if (data.Object == null || data.Object.gameObject == null)
            return;

        Debug.Log($"Resetting object {data.Object.gameObject.name}");


        var netObj = data.Object.GetComponent<NetworkedObject>();
        if (netObj != null)
        {
            if (!netObj.HasAuthority)
                netObj.RequestOwnership();

            SocketManager.UnsocketObject(netObj.uniqueID);
        }

        var interact = data.Object.GetComponent<CustomXRInteractable>();
        if (interact != null)
        {
            interact.ChangeOwnership(null);
        }

        if (data.Socket != null && interact != null)
        {
            data.Socket.RequestSocketItem(interact);
            data.Object.localPosition = Vector3.zero;
            data.Object.localRotation = data.Rotation;
        }
        else
        {
            data.Object.SetParent(data.Parent, false);
            data.Object.localPosition = data.Position;
            data.Object.localRotation = data.Rotation;
        }

    }

    public void PerformSelectableObjectAction()
    {
        if (_objectData == null || _objectData.Count <= 0)
            return;

        foreach (var data in _objectData)
        {
            ResetObject(data);
        }
    }
}
