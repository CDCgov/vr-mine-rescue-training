using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0219

/// <summary>
/// Base class for all proximity systems
/// </summary>
public abstract class ProxSystem : MonoBehaviour
{

    public struct VisOptions
    {		
        public bool ShowRedShell;
        public bool ShowYellowShell;

        public VisOptions(bool showRed, bool showYellow)
        {
            ShowRedShell = showRed;
            ShowYellowShell = showYellow;
        }
    }

    /// <summary>
    /// Enable display of visualation for the specified zone, attaching the visualation
    /// to the provided parent game object
    /// </summary>
    public abstract void EnableZoneVisualization(VisOptions opt);

    /// <summary>
    /// Disable all zone visualizations
    /// </summary>
    public abstract void DisableZoneVisualization();

    /// <summary>
    /// return what zone is active (red, green, etc.)
    /// </summary>	
    public virtual ProxZone GetActiveProxZone()
    {
        return _activeProxZone;
    }

    /// <summary>
    /// enumerate all detected prox objects in the specified zone
    /// </summary>
    public abstract IEnumerator<GameObject> GetObjectsInZone(ProxZone zone);

    public abstract Bounds ComputeProxSystemBounds();

    public abstract ProxZone TestPoint(Vector3 position);

    private Dictionary<Collider, int> _closeObjects;
    private ProxZone _activeProxZone = ProxZone.GreenZone;

    private int _mask;
    private int _playerLayer;

    protected virtual void Start()
    {/*
        _closeObjects = new Dictionary<Collider, int>();

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        Bounds b = ComputeProxSystemBounds();
        GameObject triggerObj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("BoxTrigger"));
        triggerObj.transform.SetParent(transform, false);
        BoxCollider col = triggerObj.GetComponent<BoxCollider>();
        col.center = b.center;
        col.size = b.size;

        _mask = LayerMask.GetMask("Player");
        _playerLayer = LayerMask.NameToLayer("Player");*/
    }
    protected virtual void Update()
    {

    }

    /*
    protected virtual void Update()
    {
        ProxZone curZone = ProxZone.GreenZone;
        int colliderCount = _closeObjects.Keys.Count;
        foreach (Collider col in _closeObjects.Keys)
        //for (int i = 0; i < colliderCount; i++)
        {
            //Collider col = _closeObjects.Keys.;
            //if ((col.gameObject.layer & mask) == 0)
            //continue;
            if (col.gameObject.layer != _playerLayer)
                continue;

            //Vector3 testPos = col.transform.position;
            Vector3 testPos = col.bounds.center;

            ProxZone colZone = TestPoint(testPos);

            ProxAudioAlert alert = col.gameObject.GetComponent<ProxAudioAlert>();

            if (colZone > curZone)
            {
                curZone = colZone;
            }

            if (alert != null)
            {
                alert.SetProxZone(colZone);
            }

            //if (curZone == ProxZone.RedZone)
                //break; // don't need to keep testing
        }

        _activeProxZone = curZone;
    }
    */

    void OnTriggerEnter(Collider other)
    {
        int count = 0;
        _closeObjects.TryGetValue(other, out count);
        _closeObjects[other] = count + 1;
    }

    void OnTriggerExit(Collider other)
    {
        int count = 1;
        _closeObjects.TryGetValue(other, out count);
        count--;
        if (count <= 0)
            _closeObjects.Remove(other);
    }
}