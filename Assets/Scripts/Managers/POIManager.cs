using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

[HasCommandConsoleCommands]
//[CreateAssetMenu(fileName = "POIManager", menuName = "VRMine/Managers/POIManager", order = 0)]
public class POIManager : SceneManagerBase
{
	//public const string DefaultResourcePath = "Managers/POIManager";
	public static POIManager GetDefault(GameObject self) 
    {
        //return Resources.Load<POIManager>(DefaultResourcePath); 
        var manager = Util.GetDefaultManager<POIManager>(self, "POIManager", true);
        //Debug.Log($"Found POI Manager default {manager.gameObject.GetInstanceID()} {manager.POIAdded}");
        return manager;
    }


    public PointOfInterest GetSpawnPoint()
    {
        if (ActivePOIs == null)
            return null;

        foreach (var poi in ActivePOIs)
        {
            if (poi.POIType == POIType.SpawnPoint)
            {
                return poi;
            }
        }

        return null;
    }


    public event Action<PointOfInterest> POIAdded; 
	public event Action<PointOfInterest> POIRemoved;

    private List<PointOfInterest> _pois = new List<PointOfInterest>();
    private Dictionary<string, PointOfInterest> _poiMap = new Dictionary<string, PointOfInterest>();

	public List<PointOfInterest> ActivePOIs
	{
		get
		{
			if (_pois == null)
				_pois = new List<PointOfInterest>();
			return _pois;
		}
	}

	[CommandConsoleCommand("list_pois", "List POIs in the scene")]
	public static void CCListPOIS()
	{
	}

	public void AddPOI(PointOfInterest poi)
	{
		_pois.Add(poi);
        if (_poiMap.ContainsKey(poi.ID))
        {
            Debug.LogWarning($"POIManager: Duplicate POI ID {poi.ID}");
        }
        _poiMap[poi.ID] = poi;
		POIAdded?.Invoke(poi);
	}
	
	public void RemovePOI(PointOfInterest poi)
	{
		_pois.Remove(poi);
        if (_poiMap.ContainsKey(poi.ID))
            _poiMap.Remove(poi.ID);
		POIRemoved?.Invoke(poi);
	}

	public void ClearPOIs()
	{
		foreach (var poi in _pois)
		{
			RemovePOI(poi);
		}
	}


	public void CleanPOIList()
	{
		for (int i = _pois.Count - 1; i >= 0; i-- )
		{
			if (_pois[i] == null || _pois[i].gameObject == null)
			{
				_pois.RemoveAt(i);
			}
		}
	}

	public List<PointOfInterest> GetPOIs()
	{
		return _pois;
	}

	public PointOfInterest GetPOI(string id)
	{
        PointOfInterest poi = null;
        if (_poiMap.TryGetValue(id, out poi))
            return poi;      

		return null;
	}

	public PointOfInterest GetFirstOfType(POIType poiType)
	{
		foreach (var poi in _pois)
		{
			if (poi != null && poi.gameObject != null && poi.POIType == poiType)
				return poi;
		}

		return null;
	}

    private void Awake()
    {
        Util.DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //DontDestroyOnLoad(gameObject);
    }


    private void OnEnable()
	{		
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		//_pois = new List<PointOfInterest>();
		//CleanPOIList();
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}




}