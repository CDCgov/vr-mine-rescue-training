using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum POIType
{
    Unknown,
    CameraPosition,
    SpawnPoint,
    DMSpawnPoint,
    DebriefOverview
}

public class PointOfInterest : MonoBehaviour
{
    public POIManager POIManager;
    public POIType POIType;

    private string _id = null;
    private bool _poiRegistered = false;

    public string Name
    {
        get { return gameObject.name; }
    }

    public string ID
    {
        get
        {
            if (_id == null)
                return gameObject.name;
            else
                return _id;
        }
        set
        {
            if (_poiRegistered)
            {
                Debug.LogError($"Error - ID on POI changed from {_id} to {value} while POI is registered");
            }

            _id = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        
    }

    private void OnDestroy()
    {
        POIManager.RemovePOI(this);
    }

    void OnEnable()
    {
        if (POIManager == null || !(POIManager is Component))
            POIManager = POIManager.GetDefault(gameObject);


        POIManager.AddPOI(this);
        _poiRegistered = true;
    }

    void OnDisable()
    {
        POIManager.RemovePOI(this);
        _poiRegistered = false;
    }

}
