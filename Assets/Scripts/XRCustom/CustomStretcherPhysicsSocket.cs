using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomStretcherPhysicsSocket : MonoBehaviour
{
    public Bounds StretcherBounds;
    public NetworkManager NetworkManager;
    public TeleportManager TeleportManager;

    private int _playerLayer;

    private struct TeleportObjectData
    {
        public Transform ObjectTransform;
        public Vector3 OffsetVector;
        public Quaternion ObjectRotation;
    }

    private List<TeleportObjectData> _teleportObjects;

    private void Awake()
    {
        _teleportObjects = new List<TeleportObjectData>(20);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);

        TeleportManager.BeforeTeleport += OnBeforeTeleport;
        TeleportManager.Teleporting += OnTeleport;

        _playerLayer = LayerMask.NameToLayer("Player");
    }

    private void OnTeleport(Transform dest)
    {
        var currentTarget = TeleportManager.ActiveTeleportTarget;
        if (currentTarget == null)
            return;

        foreach (var data in _teleportObjects)
        {
            //data.ObjectTransform.position = dest.position + data.OffsetVector;
            data.ObjectTransform.position = currentTarget.TransformPoint(data.OffsetVector);
            data.ObjectTransform.rotation = currentTarget.rotation * data.ObjectRotation;
        }

        _teleportObjects.Clear();
    }

    private void OnDestroy()
    {
        TeleportManager.BeforeTeleport -= OnBeforeTeleport;
    }

    private void OnBeforeTeleport(Transform dest)
    {
        _teleportObjects.Clear();

        var currentTarget = TeleportManager.ActiveTeleportTarget;
        if (currentTarget == null)
            return;

        var boundsPos = transform.TransformPoint(StretcherBounds.center);

        //Collider[] cols = Physics.OverlapBox(transform.position + StretcherBounds.center, StretcherBounds.extents);
        Collider[] cols = Physics.OverlapBox(boundsPos, StretcherBounds.extents, transform.rotation);
        if (cols.Length > 0)
        {
            foreach (Collider col in cols)
            {
                if (col.attachedRigidbody == null)
                {
                    //Debug.Log($"Attached col rigidbody was null? {col.name}");
                    continue;
                }

                if (col.attachedRigidbody.gameObject.layer == _playerLayer)
                    continue;

                //CustomXRInteractable interactable = col.attachedRigidbody.GetComponent<CustomXRInteractable>();
                //NetSyncGrabInteractable netGrab = col.attachedRigidbody.GetComponent<NetSyncGrabInteractable>();//This is in place to prevent accidentally grabbing belt items in weird interstitial loading states
                //NetworkedObject networkedObject = col.attachedRigidbody.GetComponent<NetworkedObject

                CustomXRInteractable interactable;
                NetworkedObject networkedObject;

                col.attachedRigidbody.TryGetComponent<CustomXRInteractable>(out interactable);
                col.attachedRigidbody.TryGetComponent<NetworkedObject>(out networkedObject);

                if (networkedObject == null || interactable == null || interactable.CompareTag("Belt"))
                    continue;

                if (!interactable.IsGrabbable)
                    continue;

                if (interactable.CompareTag("SoundingStick") || interactable.CompareTag("Chalk") || interactable.CompareTag("MapBoard"))
                    continue;

                if (interactable.CurrentOwner != null)
                    continue;

                if (!networkedObject.HasAuthority && NetworkManager.IsServer)// && interactable.CurrentOwner == null)
                    networkedObject.RequestOwnership();

                if (!networkedObject.HasAuthority)
                    continue;

                if (col.attachedRigidbody.transform.parent == transform)
                    continue;

                var objTransform = col.attachedRigidbody.transform;
                var offset = currentTarget.InverseTransformPoint(objTransform.position);

                var objectRot = Quaternion.Inverse(currentTarget.rotation) * objTransform.rotation;

                _teleportObjects.Add(new TeleportObjectData
                {
                    ObjectTransform = objTransform,
                    OffsetVector = offset,
                    ObjectRotation = objectRot,
                });

            }
        }
    }

    //private void Update()
    //{
    //    Collider[] cols = Physics.OverlapBox(transform.position + StretcherBounds.center, StretcherBounds.extents);
    //    if (cols.Length > 0)
    //    {
    //        foreach (Collider col in cols)
    //        {
    //            CustomXRInteractable interactable = col.GetComponent<CustomXRInteractable>();
    //            NetSyncGrabInteractable netGrab = col.GetComponent<NetSyncGrabInteractable>();//This is in place to prevent accidentally grabbing belt items in weird interstitial loading states
    //            if (interactable != null)
    //            {
    //                if (col.transform.parent != transform && interactable.CurrentOwner == null && netGrab != null)
    //                {
    //                    StretcherItemOverlapCheck(col);
    //                }
    //            }
    //        }
    //    }
    //}

    private void StretcherItemOverlapCheck(Collider colliderToCheck)
    {

        Collider[] cols = Physics.OverlapBox(colliderToCheck.bounds.center, colliderToCheck.bounds.extents * 0.5f);

        if (cols.Length > 0)
        {
            foreach (Collider col in cols)
            {

                CustomXRInteractable interactable = col.GetComponent<CustomXRInteractable>();
                if (interactable != null && col != colliderToCheck)
                {
                    if (interactable.CurrentOwner == null)
                        return;
                    NetSyncGrabInteractable netGrab = col.GetComponent<NetSyncGrabInteractable>();
                    CustomXRSocket socketOwner = interactable.CurrentOwner.GetComponent<CustomXRSocket>();
                    if (netGrab != null && socketOwner != null)
                        colliderToCheck.transform.Translate(new Vector3(0, 0.3f, 0), Space.World);
                }

            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + StretcherBounds.center, StretcherBounds.size);

    }

}
