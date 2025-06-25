using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BH20RaceCoalLoadZone : MonoBehaviour
{

    public enum LoadZoneType
    {
        Loading,
        Unloading,
    }

    public Bounds ZoneBounds;
    public LoadZoneType ZoneType = LoadZoneType.Loading;

    private int _layerMask = 0;

    private HashSet<Rigidbody> _rbs;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position + ZoneBounds.center, ZoneBounds.size);
    }
    // Start is called before the first frame update
    void Start()
    {
        _layerMask = LayerMask.GetMask("Player");
        _rbs = new HashSet<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        _rbs.Clear();
        var colliders = Physics.OverlapBox(transform.position + ZoneBounds.center, ZoneBounds.size, Quaternion.identity, _layerMask);

        foreach (Collider c in colliders)
        {
            _rbs.Add(c.attachedRigidbody);
        }

        //Debug.Log($"Found {colliders.Length} colliders {_rbs.Count} rigidbodies");

        foreach (var rb in _rbs)
        {
            var raceData = rb.GetComponent<BH20RaceData>();
            if (raceData != null)
            {
                ProcessZone(raceData);
            }
        }
    }

    void ProcessZone(BH20RaceData raceData)
    {
        switch (ZoneType)
        {
            case LoadZoneType.Loading:
                raceData.CoalLoad += Time.deltaTime * 0.25f;
                if (raceData.CoalLoad > 1.0f)
                    raceData.CoalLoad = 1.0f;
                break;

            case LoadZoneType.Unloading:
                float unloaded = Time.deltaTime * 0.25f;
                unloaded = Mathf.Clamp(unloaded, 0, raceData.CoalLoad);
                raceData.CoalLoad -= unloaded;
                raceData.CoalMinedLb += unloaded * raceData.MachineCapacity;
                break;
        }

    }
}
