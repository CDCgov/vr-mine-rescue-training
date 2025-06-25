using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script attached to all waypoints, visualizes the waypoints in editor only
/// </summary>
public class WaypointGizmo : MonoBehaviour 
{
    void OnDrawGizmos()//May change this to OnDrawGizmosSelected?
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
        //Gizmos.DrawIcon(transform.position, transform.name);//TODO: Create a waypoint icon?
    }
}