using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrackingManager : MonoBehaviour {

    [HideInInspector]
    public List<TrackingZone> TrackedZones;

    //[HideInInspector]
    public List<TrackingNode> Nodes;

    public ActorHost DebugActor;

    private MasterControl _mControl;
    private TrackingNode _LastGoodNode;
    private Dictionary<ActorHost, TrackingNode> _DLastGoodNode;

    private void Start()
    {
        _DLastGoodNode = new Dictionary<ActorHost, TrackingNode>();
        _mControl = GameObject.FindObjectOfType<MasterControl>();
        MineSegment[] segs = GameObject.FindObjectsOfType<MineSegment>();
        List<MineSegment> segList = new List<MineSegment>();
        for(int i = 0; i < segs.Length; i++)
        {
            segList.Add(segs[i]);
        }
        if (_mControl != null)
        {
            _mControl.CreateBasicMiniMap(segList, Nodes);
        }
    }

    private void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
        {
            //Debug.Log("Closest Node: " + CalculateClosestNode(DebugActor).gameObject.name);
            if (Input.GetKeyDown(KeyCode.M))
            {
                _mControl.ToggleMiniMap();
            }
        }

        if (MasterControl.SceneControl != null && MasterControl.SceneControl.ActiveActors != null)
        {
            ActivateClosestNodes();
        }
    }

    

    TrackingNode CalculateClosestNode(ActorHost actor)
    {
        //float distance = 1000000;        
        TrackingNode closestNode = new TrackingNode();
        foreach(TrackingNode node in Nodes)
        {
            foreach (TrackingZone zone in node.Zones)
            {
                if (zone.ActorsInZone.Contains(actor))
                {                    
                    if (closestNode == null)
                    {
                        closestNode = node;
                        continue;
                    }
                    else
                    {
                        float d1 = Vector3.Distance(closestNode.transform.position, actor.transform.position);
                        float d2 = Vector3.Distance(node.transform.position, actor.transform.position);
                        if (d2 < d1)
                        {
                            closestNode = node;
                        }
                    }
                    break;
                }
            }
        }
        if (closestNode != null)
        {
            closestNode.ActorsClosestToThisNode.Add(actor);
            if (_DLastGoodNode.ContainsKey(actor))
            {
                _DLastGoodNode[actor] = closestNode;
            }
            else
            {
                _DLastGoodNode.Add(actor, closestNode);
            }
        }
        else
        {
            if (_DLastGoodNode.ContainsKey(actor))
            {
                if (!_DLastGoodNode[actor].ActorsClosestToThisNode.Contains(actor))
                {
                    _DLastGoodNode[actor].ActorsClosestToThisNode.Add(actor);
                    //_DLastGoodNode[actor].ActivateActorLabel(actor);
                    //Debug.Log(_DLastGoodNode[actor].name + ":node, " + actor.ActorName + ":act");
                }
            }
        }
             
        return closestNode;
    }

    public void ActivateClosestNodes()
    {
        List<ActorHost> actors = MasterControl.SceneControl.ActiveActors;
        foreach(TrackingNode node in Nodes)
        {
            node.ClearActiveActors();
        }
        foreach(ActorHost actor in actors)
        {
            CalculateClosestNode(actor);
            //Debug.Log("node name? " + node.name);
            //if (node != null)
            //{
            //    node.ActivateActorLabel(actor);
            //    node.ActorsClosestToThisNode.Add(actor);
            //}
        }
        foreach(TrackingNode node in Nodes)
        {
            if(node.MiniMapButton != null)
            {
                node.MiniMapButton.UpdateColor();
            }
        }
    }

    public void ActivateClosestNodesDebug()
    {
        ActorHost actor = DebugActor;
        foreach (TrackingNode nod in Nodes)
        {
            nod.ClearActiveActors();
        }
        
        TrackingNode node = CalculateClosestNode(actor);
        
        node.ActivateActorLabel(actor);
        
    }

    List<TrackingNode> GetActivatedNodes()
    {
        List<TrackingNode> active = new List<TrackingNode>();
        foreach (TrackingNode node in Nodes)
        {
            if (node.ActorsInNode != null)
            {
                if (node.ActorsInNode.Count > 0)
                {
                    active.Add(node);
                }
            }
        }
        return active;
    }
}
