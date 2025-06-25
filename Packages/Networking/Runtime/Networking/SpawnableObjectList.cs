using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class SpawnableObject
{
    public string ObjectName;
    public AssetReference Asset;
    public Vector3 SpawnOffset;
    public Vector3 SpawnOrientation = Vector3.zero;
    public bool SpanEntry = false;
    public bool SpawnNestedObjects = false;

    public void Spawn(Camera camera, LayerMask layerMask, NetworkManager networkManager)
    {
        Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);

        Spawn(mouseRay, layerMask, networkManager);
    }

    public void Spawn(Ray ray, LayerMask layerMask, NetworkManager networkManager)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100.0f, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"Spawning object {ObjectName} at {hit.point.ToString()}");

            var rot = Quaternion.Euler(SpawnOrientation);
            Spawn(hit.point, rot, false, networkManager);
        }

    }


    public void Spawn(Vector3 pos, Quaternion rot, bool ignoreOffset, NetworkManager networkManager)
    {
        networkManager.LogSessionEvent(VRNLogEventType.DmspawnObject, $"DM Spawn Object {ObjectName}",
            pos, rot, ObjectName);

        if (SpawnNestedObjects)
            SpawnNested(pos, rot, ignoreOffset, networkManager);
        else
            SpawnNormal(pos, rot, ignoreOffset, networkManager);
    }

    private async void SpawnNested(Vector3 pos, Quaternion rot, bool ignoreOffset, NetworkManager networkManager)
    {
        if (networkManager == null)
        {
            Debug.LogError("SpawnNested: Network manager is null");
            return;
        }
        //if (networkManager == null)
        //    networkManager = NetworkManager.GetDefault();

        var rootAsset = await Addressables.LoadAssetAsync<GameObject>(Asset).Task;
        rootAsset.transform.localPosition = Vector3.zero;
        rootAsset.transform.localRotation = Quaternion.identity;

        foreach (Transform obj in rootAsset.transform)
        {
            if (obj.TryGetComponent<NetworkedObject>(out var netObj))
            {
                if (netObj.SourcePrefab == null || netObj.SourcePrefab.Length <= 0)
                    continue;

                //Debug.Log($"Spawning nested asset {netObj.SourcePrefab}");

                var worldPos = obj.position + pos;
                var worldRot = rot * obj.rotation;

                _ = networkManager.NetworkedObjectManager.SpawnObject(netObj.SourcePrefab, System.Guid.NewGuid(), worldPos, worldRot, true);
            }
        }

        Addressables.Release<GameObject>(rootAsset);

    }

    private void SpawnNormal(Vector3 pos, Quaternion rot, bool ignoreOffset, NetworkManager networkManager)
    {
        if (networkManager == null)
        {
            Debug.LogError("SpawnNormal: Network manager is null");
            return;
            //networkManager = NetworkManager.GetDefault(gameObject);
        }

        Debug.Log($"Spawn: {Asset.RuntimeKey.ToString()}");

        if (!ignoreOffset)
            pos += SpawnOffset;

        networkManager.SpawnObject(Asset.RuntimeKey.ToString(), System.Guid.NewGuid(), pos, rot, true);

        if (networkManager.IsRecording)
        {
            networkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.Message,
                Position = pos.ToVRNVector3(),
                Rotation = rot.ToVRNQuaternion(),
                PositionMetadata = "UserSelected",
                Message = "Object Spawned",

            });
        }
    }



    public async Task<bool> SpawnSpannedObject(Vector3 pos1, Vector3 pos2, LayerMask layerMask, NetworkManager networkManager = null)
    {
        if (networkManager == null || networkManager.NetworkedObjectManager == null)
            return false;

        //move ray above "ground" plane
        float startY = pos1.y;

        pos1.y = pos1.y + 1.0f;
        pos2.y = pos1.y;

        //check positions are valid
        Vector3 dir = pos2 - pos1;
        float magnitude = dir.magnitude;
        if (magnitude < 0.05f || float.IsNaN(magnitude) || float.IsInfinity(magnitude))
        {
            return false;
        }

        Vector3 startPoint = Vector3.zero;
        Vector3 endPoint = Vector3.zero;

        RaycastHit hit;
        if (!Physics.Raycast(pos1, dir, out hit, 100, layerMask.value, QueryTriggerInteraction.Ignore))
            return false;

        endPoint = hit.point;

        if (!Physics.Raycast(pos1, dir * -1, out hit, 100, layerMask.value, QueryTriggerInteraction.Ignore))
            return false;

        startPoint = hit.point;

        Vector3 pos = (startPoint + endPoint) * 0.5f;
        pos.y = startY;

        var rot = Quaternion.FromToRotation(Vector3.right, dir);

        //calculate length
        float length = Vector3.Distance(startPoint, endPoint);

        var obj = await networkManager.NetworkedObjectManager.SpawnObject(Asset.RuntimeKey.ToString(),
            System.Guid.NewGuid(), pos, rot, true);

        float objBaseLength = 1.0f;
        var collider = obj.GetComponent<BoxCollider>();
        if (collider != null)
            objBaseLength = collider.size.x;

        obj.transform.localScale = new Vector3(length / objBaseLength, 1, 1);

        return true;
    }
}

[CreateAssetMenu(fileName = "SpawnableObjectList", menuName = "VRMine/Spawnable Object List", order = 1)]
public class SpawnableObjectList : ScriptableObject
{
    public bool ListEnabled = true;
    public int ListPriority = 100;
    public List<SpawnableObject> ObjectList;
}
