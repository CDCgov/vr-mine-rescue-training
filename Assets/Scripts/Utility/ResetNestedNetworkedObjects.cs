using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetNestedNetworkedObjects : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public float ResetInterval = 20;

    private struct ResetObjectData
    {
        public NetworkedObject NetworkedObject;
        public Transform transform;
        public Rigidbody rigidbody;
        public Vector3 StartPos;
        public Quaternion StartRot;
    }

    private List<ResetObjectData> _resetObjData;
    private float _nextReset = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        _resetObjData = new List<ResetObjectData>();

        var objects = transform.GetComponentsInChildren<NetworkedObject>();

        foreach (var obj in objects)
        {
            ResetObjectData data = new ResetObjectData();
            data.NetworkedObject = obj;
            data.transform = obj.transform;
            data.rigidbody = obj.GetComponent<Rigidbody>();
            data.StartPos = obj.transform.position;
            data.StartRot = obj.transform.rotation;

            _resetObjData.Add(data);
        }

        _nextReset = Time.time + ResetInterval;
    }

    void ResetObjects()
    {
        if (_resetObjData == null || !NetworkManager.IsServer)
            return;

        foreach (var data in _resetObjData)
        {
            if (data.transform == null || data.NetworkedObject == null)
                continue;

            if (!data.NetworkedObject.HasAuthority)
                data.NetworkedObject.RequestOwnership();

            if (data.rigidbody != null && !data.rigidbody.isKinematic) 
            {
                data.rigidbody.velocity = Vector3.zero;
                data.rigidbody.angularVelocity = Vector3.zero;
            }

            data.transform.position = data.StartPos;
            data.transform.rotation = data.StartRot;
        }
    }

    void Update()
    {
        if (!NetworkManager.IsServer)
        {
            this.enabled = false;
            return;
        }

        if (Time.time > _nextReset)
        {
            _nextReset = Time.time + ResetInterval;

            ResetObjects();
        }
    }
}
