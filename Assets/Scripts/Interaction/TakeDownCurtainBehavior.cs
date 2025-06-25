using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using UnityEngine;

[RequireComponent(typeof(CustomXRInteractable))]
[RequireComponent(typeof(NetworkedObject))]
public class TakeDownCurtainBehavior : MonoBehaviour, IInteractableObject
{
    public float DetectionRadius = 1f;//meters

    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public string HalfHungAddress = "Curtain_HalfHung_MP";
    public string BlownDownAddress = "Ventilation_Curtain/Curtain_BlownDown";
    public string EventLogMessage = "";

    public bool IsBarricade = false;
    public Transform WoodBlocks;
    public Vector3 BlownCurtainOffset;
    public float BlownOffset = 1;

    private NetworkedObject _netObj;
    private CustomXRInteractable _xrInteractable;
    private Collider[] _targets = new Collider[20];

    private void Start()
    {
        if (NetworkManager == null)
        {
            NetworkManager = NetworkManager.GetDefault(gameObject);
        }
        if (PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }

        _netObj = GetComponent<NetworkedObject>();
        if (_xrInteractable == null)
        {
            _xrInteractable = GetComponent<CustomXRInteractable>();
        }

        if (IsBarricade)
            ReparentBarricadeBlocks();
    }

    public void HandleCurtainActivation(Transform interactor)
    {
        Debug.Log("Curtain: TakeDownCurtainBehavior::OnActivate");

        /* LayerMask layerMask = LayerMask.GetMask("Floor");
         float distance = Mathf.Infinity;
         float distanceCheck2 = Mathf.Infinity;
         Vector3 detectionPoint = Vector3.zero;
         Vector3 direction1 = transform.TransformDirection(Vector3.right);
         Vector3 direction2 = transform.TransformDirection(Vector3.left);
         Vector3 localPoint = transform.InverseTransformPoint(interactor.position);

         localPoint.z = 0;
         localPoint.y = 1.7f;

         bool foundHit1 = false;
         bool foundHit2 = false;
         Vector3 worldPoint = transform.TransformPoint(localPoint);
         Debug.Log($"World point where we check for raycast: {worldPoint}");
         if (Physics.Raycast(worldPoint, direction1, out RaycastHit hit1, 3.5f, layerMask))
         {
             distance = Vector3.Distance(worldPoint, hit1.point);
             foundHit1 = true;
             Debug.Log($"Found hit 1! {hit1.point}");
         }
         if (Physics.Raycast(worldPoint, direction2, out RaycastHit hit2, 3.5f, layerMask))
         {
             distanceCheck2 = Vector3.Distance(worldPoint, hit2.point);
             foundHit2 = true;
             Debug.Log($"Found hit 2! {hit2.point}");
         }


         if (foundHit1 && foundHit2)
         {
             if (distance < distanceCheck2)
             {
                 detectionPoint = hit2.point;

             }
             else
             {
                 detectionPoint = hit1.point;
             }
         }
         else
         {
             if (foundHit1)
             {
                 detectionPoint = hit1.point;
             }
             else if (foundHit2)
             {
                 detectionPoint = hit2.point;
             }
             else
             {
                 detectionPoint = interactor.position;
                 Debug.Log("Curtain fallback?");
             }
         }

         Debug.Log($"Detection point: {detectionPoint}"); */

        var detectionPoint = transform.position;
        if (interactor != null)
            detectionPoint = interactor.transform.position;

        Collider[] cols = Physics.OverlapSphere(detectionPoint, 10.0f); //should change to a value based on bolt distance
        CurtainReceiver closestReceiver = null;
        float minDistance = Mathf.Infinity;
        foreach (Collider col in cols)
        {
            CurtainReceiver cr = col.GetComponent<CurtainReceiver>();
            if (cr != null)
            {
                if (cr.enabled)
                {
                    float dist = Vector3.Distance(col.transform.position, detectionPoint);
                    if (dist < minDistance)
                    {
                        closestReceiver = cr;
                        minDistance = dist;
                    }
                }
            }
        }

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (closestReceiver != null)
        {
            CurtainReceiver target = closestReceiver.PairedReceiver;
            if(target != null)
            {
                spawnPos = target.transform.position;
                Vector3 eul = new Vector3(0, target.transform.eulerAngles.y, 0);
                spawnRot = Quaternion.Euler(eul);
            }
            else
            {
                spawnPos = closestReceiver.transform.position;
                Vector3 eul = new Vector3(0, closestReceiver.transform.eulerAngles.y, 0);
                spawnRot = Quaternion.Euler(eul);
            }
        }
        else
        {
            spawnPos = transform.position;
            spawnRot = Quaternion.identity;
        }
        //TryGetComponent<BoxCollider>(out var item);
        ////Collider[] targets = Physics.OverlapBox(item.bounds.center, item.bounds.extents);
        ////Bounds bound = item.bounds;
        ////bound.extents = new Vector3(bound.extents.x, bound.extents.y, bound.extents.z*4);
        //Physics.OverlapBoxNonAlloc(item.bounds.center, new Vector3(3 * transform.localScale.x, 2 * transform.localScale.y, 2 * transform.localScale.z), _targets);
        //if(_targets.Length <= 0)
        //{
        //    Debug.Log($"Found no targets?");
        //}
        //foreach (Collider target in _targets)
        //{
        //    if (target.TryGetComponent<ObjectInfo>(out var info))
        //    {
        //        Debug.Log($"Found target: {target.name}");
        //        if (info.AssetID.Contains("SPRAY_PAINT"))
        //        {
        //            Debug.Log($"Destroying {info.name}");
        //            Destroy(target.gameObject);
        //        }
        //    }
        //}

        if (!IsBarricade)
        {
            NetworkManager.SpawnObject(HalfHungAddress, System.Guid.NewGuid(), spawnPos, spawnRot, true);
        }
        else
        {
            Vector3 pos = transform.position + transform.forward * BlownOffset;
            NetworkManager.SpawnObject(BlownDownAddress, System.Guid.NewGuid(), pos, transform.rotation, true);

            //ReparentBarricadeBlocks();
            //ClearNearbySpraypaint();
        }

        if (EventLogMessage == null)
            EventLogMessage = "";

        NetworkManager.LogSessionEvent(new VRNLogEvent
        {
            EventType = VRNLogEventType.CurtainRemove,
            ObjectType = VRNLogObjectType.Curtain,
            ObjectName = "Curtain Removed",
            Message = EventLogMessage,
            Position = transform.position.ToVRNVector3(),
            Rotation = transform.rotation.ToVRNQuaternion(),
            SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
        });

        if (_netObj != null)
        {
            Debug.Log($"Curtain: Destroying curtain {_netObj.gameObject.name} - {_netObj.uniqueID}");
            NetworkManager.DestroyObject(_netObj.uniqueID);
        }
        else
        {
            Debug.LogError($"Curtain: No networked objct present on curtain {gameObject.name}!");
            Destroy(gameObject);
        }
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {

    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {

    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {

    }

    public void OnPickedUp(Transform interactor)
    {

    }

    public void OnDropped(Transform interactor)
    {

    }

    public ActivationState CanActivate => ActivationState.Ready;

    public void OnActivated(Transform interactor)
    {
        Debug.Log("Curtain: TakeDownCurtainBehavior::OnActivate");

        HandleCurtainActivation(interactor);
    }

    public void OnDeactivated(Transform interactor)
    {

    }

    private void ClearNearbySpraypaint()
    {
        if (!IsBarricade)
            return;

        if (!TryGetComponent<BoxCollider>(out var item))
            return;

        //Collider[] targets = Physics.OverlapBox(item.bounds.center, item.bounds.extents);
        //Bounds bound = item.bounds;
        //bound.extents = new Vector3(bound.extents.x, bound.extents.y, bound.extents.z*4);
        _targets = Physics.OverlapBox(item.bounds.center, new Vector3(item.size.x / 2, item.size.y / 2, item.size.z / 2), transform.rotation);
        if (_targets.Length <= 0)
        {
            Debug.Log($"Found no targets?");
        }
        foreach (Collider target in _targets)
        {

            if (target != null && target.TryGetComponent<ObjectInfo>(out var info))
            {
                Debug.Log($"Found target: {target.name}");
                if (info.AssetID.Contains("SPRAY_PAINT"))
                {
                    Debug.Log($"Destroying {info.name}");
                    Destroy(target.gameObject);
                }
            }


        }

    }

    private void ReparentBarricadeBlocks()
    {

        if (!IsBarricade)
            return;

        if (WoodBlocks == null)
            return;

        if (!transform.parent.gameObject.activeInHierarchy)
            return;
        
        GameObject newBlocksParent = new GameObject();
        newBlocksParent.transform.position = transform.position;
        newBlocksParent.transform.parent = transform.parent;
        WoodBlocks.parent = newBlocksParent.transform;
        
        
    }

    private void EnableNPCResponse(bool enabled)
    {
        if (WoodBlocks == null)
            return;

        var knockComponents = WoodBlocks.GetComponentsInChildren<Knock>();
        foreach (var knock in knockComponents)
        {
            knock.AllowNPCResponse = enabled;
        }
        //foreach (Transform child in WoodBlocks.transform)
        //{
        //    if (child.TryGetComponent<Knock>(out var knock))
        //    {
        //        knock.AllowNPCResponse = false;
        //    }
        //}
    }

    private void OnDestroy()
    {
        EnableNPCResponse(false);
        ClearNearbySpraypaint();
    }

    private void OnDrawGizmos()
    {
        if (IsBarricade)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(transform.localPosition + BlownCurtainOffset, new Vector3(0.1f, 0.1f, 0.1f));
        }
        //if (TryGetComponent<BoxCollider>(out var item))
        //{
        //    Collider[] targets = Physics.OverlapBox(item.bounds.center, item.bounds.extents);
        //    Bounds bound = item.bounds;
        //    Vector3 size = bound.size;
        //    Vector3 dir = transform.forward;
        //    dir.y = 1;
        //    dir.z *= 2;
        //    dir.x *= 2;
        //    size = size + dir;
        //    bound.extents = new Vector3(bound.extents.x * 2, bound.extents.y, bound.extents.z);
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawCube(bound.center, bound.size);

        //}
    }
}
