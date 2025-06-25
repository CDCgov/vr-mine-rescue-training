using UnityEngine;
using System;
using UnityEngine.UI;
using System.Drawing.Text;
using System.Collections.Generic;
using NIOSH_MineCreation;

public class ScenarioRequirements : MonoBehaviour
{
    public enum CriteriaName
    {
        PlayerSpawn,
        DMSpawn,
        TeamStop,
        StartJunction,
        Atmosphere,
        NodesConnected,
    }
    
    public Button PlayButton;
    
    
    [Space (20)]
    [Header("Requirements Not Met Color Block")]
    public ColorBlock RequirementsNotMetColorBlock;
    [Space(20)]
    [Header("Requirements Not Met Color Block")]
    public ColorBlock RequirementsMetColorBlock;
    [Space(20)]
    

    public GameObject HeaderMissingRequirementsClosed;
    public GameObject HeaderMissingRequirementsOpen;
    public GameObject HeaderRequirementsMetClosed;
    public GameObject HeaderRequirementsMetOpen;

    public Criteria PlayerSpawn;
    public Criteria DMSpawn;
    public Criteria TeamStop;
    public Criteria StartJunction;
    public Criteria Atmosphere;
    public Criteria NodesConnected;
    private MenuTooltip _playButtonTooltip;

    private bool _sceneChanged = false;
    private bool _ventGraphChanged = false;

    private void Start()
    {
        _playButtonTooltip = PlayButton.GetComponent<MenuTooltip>();
        SetHeader(CheckAllCriteriaAchieved());

        FindObjectOfType<ScenarioSaveLoad>().ScenarioChanged += OnScenarioChanged;
        FindObjectOfType<VentLayerManager>().VentGraphUpdated += OnVentGraphUpdated;
    }

    /// <summary>
    /// Wrapper for testing in Unity UI
    /// </summary>
    /// <param name="index"></param>
    public void SetCriteriaTrue(int index)
    {
        SetCriteria((CriteriaName)index, true);
    }

    /// <summary>
    /// Set a criteria to achieved or not achieved
    /// </summary>
    /// <param name="criteriaName"></param>
    /// <param name="achieved"></param>
    public void SetCriteria(CriteriaName criteriaName, bool achieved)
    {
        
        switch (criteriaName)
        {
            case CriteriaName.PlayerSpawn:
                PlayerSpawn.Set(achieved);
                break;

            case CriteriaName.DMSpawn:
                DMSpawn.Set(achieved);
                break;

            case CriteriaName.TeamStop:
                TeamStop.Set(achieved);
                break;

            case CriteriaName.StartJunction:
                StartJunction.Set(achieved);
                break;

            case CriteriaName.Atmosphere:
                Atmosphere.Set(achieved);
                break;

            case CriteriaName.NodesConnected:
                NodesConnected.Set(achieved);
                break;
        }

        SetHeader(CheckAllCriteriaAchieved());
    }

    private bool CheckAllCriteriaAchieved()
    {
        bool allCriteriaAchieved = true;
        if (!PlayerSpawn.criteriaAchieved)
            allCriteriaAchieved = false;
        if (!DMSpawn.criteriaAchieved)
            allCriteriaAchieved = false;
        if (!TeamStop.criteriaAchieved)
            allCriteriaAchieved = false;
        if (!StartJunction.criteriaAchieved)
            allCriteriaAchieved = false;
        if (!Atmosphere.criteriaAchieved)
            allCriteriaAchieved = false;
        if (!NodesConnected.criteriaAchieved)
            allCriteriaAchieved = false;

        return allCriteriaAchieved;
    }

    private void SetHeader(bool achieved)
    {
        if(HeaderMissingRequirementsClosed != null &&
           HeaderMissingRequirementsOpen != null &&
           HeaderRequirementsMetClosed != null &&
           HeaderRequirementsMetOpen != null)
        {
            HeaderMissingRequirementsClosed.SetActive(!achieved);
            HeaderMissingRequirementsOpen.SetActive(!achieved);
            HeaderRequirementsMetClosed.SetActive(achieved);
            HeaderRequirementsMetOpen.SetActive(achieved);
        }

        //PlayButton.interactable = achieved;
        _playButtonTooltip.HideTooltip = achieved;
        
        
        if(achieved) 
        {
            PlayButton.colors = RequirementsMetColorBlock; 
        }
        else
            PlayButton.colors = RequirementsNotMetColorBlock;
        
    }

    private void OnScenarioChanged()
    {
        _sceneChanged = true;
    }

    private void OnVentGraphUpdated()
    {
        _ventGraphChanged = true;
    }

    private void CheckPOIObjectRequirements()
    {
        VRPointOfInterest[] allPOIs = FindObjectsOfType<VRPointOfInterest>();

        int numSpawnPoints, numDMSpawns, numCameraPositions;
        numSpawnPoints = numDMSpawns = numCameraPositions = 0;
        foreach(VRPointOfInterest poi in allPOIs)
        {
            switch (poi.POIType)
            {
                case POIType.SpawnPoint:
                    numSpawnPoints++;
                    break;
                case POIType.DMSpawnPoint:
                    numDMSpawns++;
                    break;
                case POIType.CameraPosition:
                    numCameraPositions++;
                    break;
            }
        }

        SetCriteria(CriteriaName.PlayerSpawn, numSpawnPoints == 1);
        SetCriteria(CriteriaName.DMSpawn, numDMSpawns == 1);
        SetCriteria(CriteriaName.TeamStop, numCameraPositions >= 1);
    }

    private bool CheckAllNodesConnected()
    {
        List<VentilationLayerNode> allNodes =
            new List<VentilationLayerNode>(GameObject.FindObjectsOfType<VentilationLayerNode>());

        if (allNodes.Count == 0)
            return false;

        foreach (VentilationLayerNode node in allNodes)
        {
            if (node.nodeJunction.LinkedAirways.Count == 0)
            {
                return false;
            }
        }

        return true;
    }

    private void CheckJunctionRequirements()
    {
        var ventControl = FindObjectOfType<VentilationControl>();
        if (ventControl == null)
            return;

        if (ventControl.VentGraph == null)
            ventControl.VentGraph = new VentGraph();

        List<VentJunction> junctions = new List<VentJunction>(ventControl.VentGraph.GetJunctions());

        int numStartJuncs, numInAtmosphereJuncs;
        numStartJuncs = numInAtmosphereJuncs = 0;

        foreach(VentJunction junc in junctions)
        {
            if (junc.IsStartJunction)
                numStartJuncs++;

            if (junc.IsInAtmosphere)
                numInAtmosphereJuncs++;
        }

        SetCriteria(CriteriaName.StartJunction, numStartJuncs == 1);
        SetCriteria(CriteriaName.Atmosphere, numInAtmosphereJuncs > 0);
    }

    private void LateUpdate()
    {
        if (_sceneChanged)
        {
            _sceneChanged = false;

            CheckPOIObjectRequirements();
        }

        if (_ventGraphChanged)
        {
            _ventGraphChanged = false;

            SetCriteria(CriteriaName.NodesConnected, CheckAllNodesConnected());
            CheckJunctionRequirements();
        }
    }
}

[Serializable]
public struct Criteria
{
    public GameObject checkMark;//assign in unity inspector
    public bool criteriaAchieved;  
    public void Set(bool achieved)
    {
        criteriaAchieved = achieved;
        if(checkMark != null)
            checkMark.SetActive(achieved);
    }
}
