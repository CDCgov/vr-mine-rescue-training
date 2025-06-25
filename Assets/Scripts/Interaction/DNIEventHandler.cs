using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNIEventHandler : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public ChalkDNITextHandle ChalkDNITextHandle;
    // Start is called before the first frame update
    void Start()
    {
        if(NetworkManager == null)
        {
            NetworkManager = NetworkManager.GetDefault(gameObject);
        }

        if(ChalkDNITextHandle == null)
        {
            ChalkDNITextHandle = GetComponent<ChalkDNITextHandle>();
        }

        if (NetworkManager.IsServer)
            NetworkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.DateAndInitial,
                ObjectType = VRNLogObjectType.Chalk,
                ObjectName = "DNI",
                Position = transform.position.ToVRNVector3(),
                Rotation = transform.rotation.ToVRNQuaternion(),
            });
    }
}
