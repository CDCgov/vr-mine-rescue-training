using BAHDOL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ComponentInfo_MineSegment : ModularComponentInfo, ISaveableComponent
{
    public string componentName = "MineSegment";
    //protected ObjectInfo objectInfo;
    public Inspector.ExposureLevel volumeExposureLevel;

    public GameObject TeamstopObject;
    public GameObject TeamstopPrefab;
    public bool IsTeamstop = true;
    public bool IsMapped = true;
    public float BoltSpacing = 1.22f;

    public MineSegment MineSegment_Component;
    public POIManager POIManager;
    private List<RoofBoltGeneratorZone> _boltGeneratorZones;
    private RoofBoltGenerator _boltGenerator;
    private Guid _teamstopGUID;
    private string _teamstopName;
    private bool _teamstopConfigured = false;

    private void Awake()
    {
        _teamstopGUID = Guid.NewGuid();
        //objectInfo = GetComponent<ObjectInfo>();
        //if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
        //if (objectInfo != null)
        //{
        //    if (!objectInfo.componentInfo_MineSegments.Contains(this)) objectInfo.componentInfo_MineSegments.Add(this);
        //}

        MineSegment_Component = GetComponent<MineSegment>();
        //_boltGeneratorZones = new List<RoofBoltGeneratorZone>();
        //RoofBoltGeneratorZone[] zones = GetComponentsInChildren<RoofBoltGeneratorZone>();
        //foreach (var zone in zones)
        //{
        //    _boltGeneratorZones.Add(zone);
        //}
        _boltGenerator = GetComponent<RoofBoltGenerator>();
        //if (TeamstopObject != null)
        //{
        //    VRPointOfInterest vrPov = TeamstopObject.GetComponent<VRPointOfInterest>();
        //    if (TeamstopObject.GetComponent<VRPointOfInterest>())
        //    {
                
        //        Destroy(vrPov);
        //    }
        //}
    }

    public void ResetTeamstopGUID()
    {
        _teamstopGUID = Guid.NewGuid();
    }

    private void Start()
    {
        if(TeamstopPrefab == null)
        {
            return;
        }
        if (POIManager == null || !(POIManager is Component))
        {
            POIManager = POIManager.GetDefault(gameObject);
        }

        if(ScenarioSaveLoad.Instance != null)
        {
            ScenarioSaveLoad.Instance.onLoadComplete += SpawnBolts;

            //if (ScenarioSaveLoad.IsScenarioEditor)
            //{
            //    ConfigureTeamstop(IsTeamstop);
            //}
        }

        if (!_teamstopConfigured)
            ConfigureTeamstop(IsTeamstop);
    }

    private void OnDestroy()
    {
        if (ScenarioSaveLoad.Instance != null)
        {
            ScenarioSaveLoad.Instance.onLoadComplete -= SpawnBolts;
        }
    }

    public string[] SaveInfo()
    {
        if (_teamstopName == null)
            _teamstopName = "";

        return new string[] { "IsTeamstop|" + IsTeamstop,"IsMapped|" + IsMapped, "BoltSpacing|" +  BoltSpacing, "TSID|" + _teamstopGUID.ToString(), "TSName|" + _teamstopName};
    }

    public string SaveName()
    {
        return componentName;
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
        {
            Debug.Log("Failed to load NPC component info. Saved component is null for " + gameObject.name); return;
        }
        componentName = component.GetComponentName();

        _teamstopName = component.GetParamValueString("TSName", "");
        var tsIDString = component.GetParamValueString("TSID", null);
        if (tsIDString == null || !Guid.TryParse(tsIDString, out _teamstopGUID))
        {
            _teamstopGUID = Guid.NewGuid();
        }

        bool.TryParse(component.GetParamValueAsStringByName("IsTeamstop"), out IsTeamstop);
        ConfigureTeamstop(IsTeamstop);

        bool.TryParse(component.GetParamValueAsStringByName("IsMapped"), out IsMapped);
        SetMapped(IsMapped);

        float.TryParse(component.GetParamValueAsStringByName("BoltSpacing"), out BoltSpacing);
        SpawnBolts();
        
    }

    public void SpawnBolts()
    {
        //if(_boltGeneratorZones.Count <= 0)
        //{
        //    return;
        //}
        //foreach (var zone in _boltGeneratorZones)
        //{
        //    zone.BoltSpacingDistance = BoltSpacing;
        //    zone.GenerateBolts();
        //}
        Debug.Log("In MineSegment spawn bolts");
        if(_boltGenerator != null)
        {
            
            _boltGenerator.RoofBoltSpacing = BoltSpacing;
            //_boltGenerator.GenerateBolts();
            StartCoroutine(DelayedSpawn(_boltGenerator));
        }
    }

    IEnumerator DelayedSpawn(RoofBoltGenerator roofBoltGenerator)
    {
        yield return new WaitForFixedUpdate();
        _boltGenerator.GenerateBolts();
    }

    public void SetMapped(bool isMapped)
    {
        if (MineSegment_Component != null)
        {
            MineSegment_Component.IncludeInMap = isMapped;
        }
    }

    public void ConfigureTeamstop(bool isTeamstop)
    {
        if (TeamstopObject == null)
            return;

        _teamstopConfigured = true;

        //TeamstopObject.SetActive(isTeamstop);
        var loadableManager = LoadableAssetManager.GetDefault(gameObject);

        if (!isTeamstop)
        {
            foreach (Transform child in TeamstopObject.transform)
            {
                Destroy(child.gameObject);
            }
            return;
        }

        if (loadableManager == null)
            return;

        if(TeamstopObject.transform.childCount != 0)
        {
            return;
        }

        //GameObject prefab = AssetLoader.Instance.GetPlaceableAsset("Team Stop");
        //var prefabIsActive = prefab.activeSelf;
        //prefab.SetActive(false);

        //GameObject ts = Instantiate(prefab);
        //prefab.SetActive(prefabIsActive);

        GameObject ts;
        if (ScenarioSaveLoad.IsScenarioEditor)
            ts = loadableManager.InstantiateEditorAsset("TEAMSTOP");
        else
            ts = loadableManager.InstantiateSceneAsset("TEAMSTOP", Vector3.zero, Quaternion.identity, null);

        ts.transform.localScale = Vector3.one;

        if (ScenarioSaveLoad.IsScenarioEditor)
        {
            //in scenario editor parent to mine segment so that the teamstop is destroyed if the segment is destroyed
            ts.transform.parent = TeamstopObject.transform;
            ts.transform.localPosition = Vector3.zero;
            ts.transform.localRotation = Quaternion.identity;
        }
        else
        {
            ts.transform.parent = transform.parent;
            ts.transform.position = TeamstopObject.transform.position;
            ts.transform.rotation = TeamstopObject.transform.rotation;
        }

        ObjectInfo info = ts.GetComponent<ObjectInfo>();
        PointOfInterest poi = ts.GetComponent<PointOfInterest>();

        info.InstanceID = _teamstopGUID;
        poi.ID = _teamstopGUID.ToString();

        ts.gameObject.SetActive(true);


        if (ScenarioSaveLoad.IsScenarioEditor)
        {
            if (!ts.TryGetComponent<PlacablePrefab>(out var placeable))
            {
                placeable = ts.AddComponent<PlacablePrefab>();
            }

            //don't save generated teamstops as separate objects
            placeable.SetIgnoreSave(true);

            placeable.SetPlaced();
            TeamstopMoveHandler moveHandler = ts.GetComponent<TeamstopMoveHandler>();
            moveHandler.mineSegment = this;
        }

        //if(gameObject.name != objectInfo.displayName)
        //{
        //    return;
        //}



        //VRPointOfInterest[] pois = FindObjectsOfType<VRPointOfInterest>();
        //if (pois != null)
        //{
        //    foreach (VRPointOfInterest poi in pois)
        //    {                    
        //        int poiIndex = 0;
        //        string[] split = poi.name.Split('_');
        //        if (split.Length > 1)
        //        {
        //            if (int.TryParse(split[1], out poiIndex))
        //            {
        //                if (index < poiIndex)
        //                {
        //                    index = poiIndex;
        //                }
        //            }
        //        }                    
        //    }                
        //}
        //index++;
        //ts.name = $"Teamstop_{index}";

        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);

        if (POIManager == null)
        {
            Debug.LogError("Couldn't find POI Manager");
            return;
        }

        if (string.IsNullOrEmpty(_teamstopName))
            _teamstopName = $"Teamstop_{POIManager.ActivePOIs.Count}";

        ts.name = _teamstopName;
        info.InstanceName = _teamstopName;

        Debug.Log($"Created Teamstop_{POIManager.ActivePOIs.Count}");
        ts.SetActive(true);
    }    
}
