using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TrackingNode : MonoBehaviour {
    public List<ActorHost> ActorsInNode;
    public List<TrackingZone> Zones;
    public List<TrackingZone> ActiveZones;
    public List<ActorHost> ActorsClosestToThisNode;
    public TextMeshPro TmLabel;
    public TrackingManager TManager;
    public TrackingNodeBtn MiniMapButton;

    private string _ActiveActors = "";   
    

    private void Awake()
    {
        ActiveZones = new List<TrackingZone>();
        ActorsInNode = new List<ActorHost>();
        ActorsClosestToThisNode = new List<ActorHost>();

        Zones = new List<TrackingZone>();
        TrackingZone[] gettingZones = gameObject.GetComponentsInChildren<TrackingZone>();
        foreach(TrackingZone zone in gettingZones)
        {
            Zones.Add(zone);
        }
        if (TManager == null)
        {
            TManager = GameObject.FindObjectOfType<TrackingManager>();
        } 
        if(TManager.Nodes == null)
        {
            TManager.Nodes = new List<TrackingNode>();
            TManager.Nodes.Add(this);
        }
        else
        {
            TManager.Nodes.Add(this);
        }

        
        //TmLabel = gameObject.GetComponentInChildren<TextMeshPro>();
        
    }

    private void Start()
    {
        if(TmLabel == null)
        {
            TmLabel = GetComponentInChildren<TextMeshPro>();
        }
    }


    public void UpdateActorList()
    {
        List<ActorHost> ActorsToRemove = new List<ActorHost>();        
        foreach (ActorHost host in ActorsInNode)
        {
            bool foundHost = false;
            foreach (TrackingZone zone in Zones)
            {
                if (zone.ActorsInZone.Contains(host))
                {
                    foundHost = true;
                    if (!ActorsInNode.Contains(host))
                    {
                        ActorsInNode.Add(host);
                    }
                    break;
                }
            }
            if (!foundHost)
            {
                //ActorsInNode.Remove(host);
                ActorsToRemove.Add(host);
            }
        }
        foreach(ActorHost act in ActorsToRemove)
        {
            ActorsInNode.Remove(act);
        }
        //TManager.ActivateClosestNodes();
    }

    public void ClearActiveActors()
    {
        _ActiveActors = "";
        if (TmLabel != null)
        {
            TmLabel.text = "";
        }
        ActorsClosestToThisNode.Clear();
        ActorsClosestToThisNode = new List<ActorHost>();
    }

    public void ActivateActorLabel(ActorHost actor)
    {
        //if (string.IsNullOrEmpty(_ActiveActors))
        //{
        //    _ActiveActors = actor.ActorName + "\n";
        //}
        //else
        //{
        //    _ActiveActors = _ActiveActors + actor.ActorName + "\n";
        //}
        foreach(ActorHost host in ActorsClosestToThisNode)
        {
            _ActiveActors += host.ActorName + "\n";
        }
        //TmLabel.text = _ActiveActors;
        // bug.Log("Is a text mesh? " + (TmLabel is TextMeshProUGUI));
        if(TmLabel == null)
        {
            Debug.Log("It's null");
        }
        //Debug.Log("Type: " + TmLabel.ToString());
        Debug.Log("Is _ActiveActors null? " + _ActiveActors);
        
        TmLabel.text = _ActiveActors;
    }
}