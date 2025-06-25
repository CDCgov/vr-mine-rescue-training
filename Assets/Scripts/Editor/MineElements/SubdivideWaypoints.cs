using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to add new waypoints between existing points
/// </summary>
public class SubdivideWaypoints :Editor 
{
    [MenuItem("Create Mine/AI Vehicle Waypoints/Subdivide Waypoints %&w", priority = 500)]//HOTKEY ctrl+w
    public static void DoSubdivide()
    {
        
        foreach (Transform item in Selection.transforms)
        {            
            if (item.tag != "Waypoint")
            {
                Debug.Log("Select waypoints only please.");
                return;
            }
        }
        if (Selection.transforms.Length == 2)//Only a PAIR of waypoints can be subdivided
        {
            if (Mathf.Abs(Selection.transforms[0].GetSiblingIndex() - Selection.transforms[1].GetSiblingIndex()) == 1)//Waypoints need to be sequential (on the heirarchy at least)
            {
                GameObject newWaypoint = new GameObject();
                newWaypoint.transform.position = (Selection.transforms[0].position + Selection.transforms[1].position) / 2;
                newWaypoint.AddComponent<WaypointGizmo>();
                int newWayIndex = 0;
                for (int i = 0; i < 2; i++)
                {
                    if (newWayIndex < Selection.transforms[i].GetSiblingIndex())
                    {
                        newWayIndex = Selection.transforms[i].GetSiblingIndex();
                    }
                }
                newWaypoint.transform.parent = Selection.transforms[0].parent;
                newWaypoint.transform.SetSiblingIndex(newWayIndex);//Sets the position of the subdivision to the index of the latter waypoint, all following waypoints in the heirarchy advance by 1 as a result
                newWaypoint.name = "Waypoint subdivide";
                newWaypoint.tag = "Waypoint";
            }
            else
            {
                Debug.Log("Select sequential waypoints.");
            }
        }
        else
        {
            Debug.Log("Select two waypoints.");
        }
    }
}