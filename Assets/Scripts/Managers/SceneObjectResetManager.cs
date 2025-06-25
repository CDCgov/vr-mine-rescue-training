using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;


public enum ObjectResetCategory
{
    All,
    General,
    Curtains,
    StretcherItems,
    NPC,
    Tools,
}

[System.Serializable]
public class ObjectResetData
{
    public ObjectResetCategory RespawnCategory;

    //should the object be destroyed if it still exists when its reset?
    public bool DestroyBeforeRespawn;
    public bool ResetPosition;
    public bool ResetRotation;
    public bool ResetScale;

    public AssetReference RespawnAsset;

    [HideInInspector]
    [System.NonSerialized]
    public Vector3 LocalPos;
    [HideInInspector]
    [System.NonSerialized]
    public Quaternion LocalRot;
    [HideInInspector]
    [System.NonSerialized]
    public Vector3 LocalScale;
    [HideInInspector]
    [System.NonSerialized]
    public GameObject ObjectReference;
    [HideInInspector]
    [System.NonSerialized]
    public Transform ParentTransform;

}

public class SceneObjectResetManager : SceneManagerBase
{
    public NetworkManager NetworkManager;
    public NetworkedObjectManager NetworkedObjectManager;

    private List<ObjectResetData> _objectData;

    public static SceneObjectResetManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<SceneObjectResetManager>("SceneObjectResetManager");
    }

    public void RegisterObject(ObjectResetCategory category, GameObject obj, 
        AssetReference respawnAsset, bool destroyBeforeRespawn,
        bool resetPosition = true, bool resetRotation = true, bool resetScale = true)
    {
        ObjectResetData data = new ObjectResetData
        {
            DestroyBeforeRespawn = destroyBeforeRespawn,
            ResetPosition = resetPosition,
            ResetRotation = resetRotation,
            ResetScale = resetScale,
            LocalPos = obj.transform.localPosition,
            LocalRot = obj.transform.localRotation,
            LocalScale = obj.transform.localScale,
            RespawnAsset = respawnAsset,
            ObjectReference = obj,
            ParentTransform = obj.transform.parent
        };

        RegisterObject(data);
    }

    public void RegisterObject(ObjectResetData data)
    {
        _objectData.Add(data);
    }

    public void ResetObjects(ObjectResetCategory category)
    {
        foreach (var data in _objectData)
        {
            if (category == ObjectResetCategory.All || 
                category == data.RespawnCategory)
            {
                ResetObject(data);
            }
        }
    }

    private async void ResetObject(ObjectResetData data)
    {
        NetworkedObject netObj = null;

        if (data.ObjectReference == null || data.ObjectReference.transform == null)
        {

            if (data.RespawnAsset == null)
                return; //can't respawn

            //object has been destroyed, respawn
            data.ObjectReference = await RespawnObject(data.RespawnAsset, data.LocalPos, data.LocalRot);
            if (data.ObjectReference == null)
            {
                Debug.LogError($"Failed to respawn object {data.ObjectReference}");
                return;
            }
        }
        else if (data.DestroyBeforeRespawn)
        {
            netObj = data.ObjectReference.GetComponent<NetworkedObject>();
            if (netObj != null)
                NetworkedObjectManager.DestroyObject(netObj.uniqueID);
            else
                Destroy(data.ObjectReference);

            data.ObjectReference = await RespawnObject(data.RespawnAsset, data.LocalPos, data.LocalRot);
        }

        if (data.ObjectReference.TryGetComponent<NetworkedObject>(out netObj))
        {
            var objData = NetworkedObjectManager.GetObjectData(netObj.uniqueID);
            if (objData.IsObjectHeld)
                return; //don't reset objects that are currently held
        }

        data.ObjectReference.transform.SetParent(data.ParentTransform, true);

        if (data.ResetPosition)
            data.ObjectReference.transform.localPosition = data.LocalPos;
        if (data.ResetRotation)
            data.ObjectReference.transform.localRotation = data.LocalRot;
        if (data.ResetScale)
            data.ObjectReference.transform.localScale = data.LocalScale;

        if (data.ObjectReference.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (data.ObjectReference.TryGetComponent<IResetableObject>(out var resetObj))
        {
            resetObj.ResetObject();
        }
    }

    private async Task<GameObject> RespawnObject(AssetReference asset, 
        Vector3 pos, Quaternion rot)
    {
        var netObj = await NetworkedObjectManager.SpawnObject(asset.RuntimeKey.ToString(), System.Guid.NewGuid(),
            pos, rot, true);

        var rso = netObj.GetComponent<RespawnableSceneObject>();
        if (rso != null)
            Destroy(rso);

        return netObj.gameObject;
    }

    private void Awake()
    {
        _objectData = new List<ObjectResetData>();
    }

    // Start is called before the first frame update
    void Start()
    {
       
        if (NetworkedObjectManager == null)
            NetworkedObjectManager = NetworkedObjectManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        NetworkManager.SceneIDChanged += OnSceneIDChanged;
    }

    private void OnSceneIDChanged(int obj)
    {
        _objectData.Clear();
    }
}
