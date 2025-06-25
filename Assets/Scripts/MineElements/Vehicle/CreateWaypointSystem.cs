using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Enum handling the three ways we can build a track
/// </summary>
public enum TrackbuildDirection
{
    Forward,
    RightTurn,
    LeftTurn
}

/// <summary>
/// Helper class to house a bunch of waypoints and their direction
/// </summary>
public class Waypoint
{
    public List<Transform> Points;//This is a list in order to handle multiple point turns
    public TrackbuildDirection Direction;	
}

/// <summary>
/// Holds data for the waypoint system editor scripts as well as handling a Reset operation in the Editor
/// </summary>
//[RequireComponent(typeof(UnityStandardAssets.Utility.WaypointCircuit))]
public class CreateWaypointSystem : MonoBehaviour
{
    //public Transform StartingMineSegment;
    //public Direction StartingDirection = Direction.PosZ;

    [HideInInspector]
    public Transform WaypointTransform;

    //This is done to handle Undo
    [HideInInspector]
    public Stack<Waypoint> WaypointStack;

    void OnEnable()
    {
        WaypointStack = new Stack<Waypoint>();
    }

    //Handling reset if it's called in Editor, delete all waypoints and start over
    void Reset()
    {
        Debug.Log("Reset: " + transform.name + ", child count: " + transform.childCount);
        
        var children = new List<GameObject>();
        foreach(Transform child in transform)
        {
            children.Add(child.gameObject);
        }
        children.ForEach(child => DestroyImmediate(child, false));

        WaypointStack = new Stack<Waypoint>();
    }

}