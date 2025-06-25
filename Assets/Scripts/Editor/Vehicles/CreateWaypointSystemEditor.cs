using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


#pragma warning disable 103
#pragma warning disable 414

/// <summary>
/// Editor script that handles modifying the inspector to create our waypoints.
/// </summary>
[CustomEditor(typeof(CreateWaypointSystem))]
public class CreateWaypointSystemEditor : Editor 
{
    //private bool _FirstPrefabPointCreated = false;
    private Transform _SpawnLocation;
    private int _ChildCount = 0;
    //private List<Vector3> _SpawnedWaypointsPositions;//Made this obsolete due to the desire to have overlapping route sections
        
    public TrackbuildDirection BuildDirection;

    private bool _CtrlHeld = false;
    private bool _AltHeld = false;
    private bool _WHeld = false;

    void Awake()
    {
        //_FirstPrefabPointCreated = false;
        _ChildCount = 0;
        //_SpawnedWaypointsPositions = new List<Vector3>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CreateWaypointSystem waypointSystemRef = (CreateWaypointSystem)target;

        if(waypointSystemRef.WaypointStack.Count == 0)
        {
            _SpawnLocation = waypointSystemRef.transform;
            _ChildCount = 0;
        }

        BuildDirection = (TrackbuildDirection)EditorGUILayout.EnumPopup("Direction to Spawn", BuildDirection);//This handles the dropdown menu for Build Direction
        

        if(GUILayout.Button("Create Waypoint"))
        {			
            //Vector3 pos;
            bool isFirst = false;

            if (waypointSystemRef.transform.childCount == 0)
            {
                _SpawnLocation = waypointSystemRef.transform;
                Collider[] cols = Physics.OverlapSphere(_SpawnLocation.position, 1);
                if(cols.Length > 0)
                {
                    waypointSystemRef.transform.position = cols[0].transform.position;//Centers the waypoint circuit parent onto the center of the floor it's at
                }				
                //pos = cols[0].transform.position;
                isFirst = true;
            }			

            GameObject newWaypoint = new GameObject();
            Waypoint way = new Waypoint();
            way.Points = new List<Transform>();
            way.Points.Add(newWaypoint.transform);
            way.Direction = TrackbuildDirection.Forward;//Default direcion, can get overridden later
            newWaypoint.AddComponent<WaypointGizmo>();
            newWaypoint.transform.position = _SpawnLocation.transform.position;
            if (!isFirst)
            {
                newWaypoint.transform.rotation = _SpawnLocation.rotation;
                switch (BuildDirection)
                {
                    case TrackbuildDirection.Forward:						
                        newWaypoint.transform.Translate(0, 0, 12, _SpawnLocation);
                        if(Physics.OverlapSphere(newWaypoint.transform.position, 1).Length <= 1)
                        {
                            DestroyImmediate(newWaypoint, false);
                            return;
                        }
                        break;
                    case TrackbuildDirection.RightTurn:
                        newWaypoint.transform.Translate(6, 0, 6, _SpawnLocation);
                        newWaypoint.transform.Rotate(0, 90, 0);
                        if (Physics.OverlapSphere(newWaypoint.transform.position, 1).Length <= 1)
                        {
                            DestroyImmediate(newWaypoint, false);
                            return;
                        }
                        
                        GameObject midPointR = new GameObject();//We create the halfway point of the arc of the turn here
                        midPointR.AddComponent<WaypointGizmo>();
                        midPointR.transform.parent = waypointSystemRef.transform;
                        midPointR.transform.position = _SpawnLocation.position;
                        midPointR.transform.rotation = _SpawnLocation.rotation;
                        midPointR.transform.Translate(1.766f, 0, 4.234f, _SpawnLocation);//Values derived from circle equation
                        midPointR.name = "Waypoint " + _ChildCount.ToString();
                        midPointR.tag = "Waypoint";
                        way.Points.Add(midPointR.transform);
                        way.Direction = TrackbuildDirection.RightTurn;
                        _ChildCount++;
                        break;
                    case TrackbuildDirection.LeftTurn:
                        newWaypoint.transform.Translate(-6, 0, 6, _SpawnLocation);
                        newWaypoint.transform.Rotate(0, -90, 0);
                        if (Physics.OverlapSphere(newWaypoint.transform.position, 1).Length <= 1)
                        {
                            DestroyImmediate(newWaypoint, false);
                            return;
                        }						
                        GameObject midPointL = new GameObject();
                        midPointL.AddComponent<WaypointGizmo>();
                        midPointL.transform.parent = waypointSystemRef.transform;
                        midPointL.transform.position = _SpawnLocation.position;
                        midPointL.transform.rotation = _SpawnLocation.rotation;
                        midPointL.transform.Translate(-1.766f, 0, 4.234f, _SpawnLocation);
                        midPointL.name = "Waypoint " + _ChildCount.ToString();
                        midPointL.tag = "Waypoint";
                        way.Points.Add(midPointL.transform);
                        way.Direction = TrackbuildDirection.LeftTurn;
                        _ChildCount++;
                        break;
                    default:
                        break;
                }				

                //newWaypoint.transform.position = pos;
                newWaypoint.transform.parent = waypointSystemRef.transform;
                newWaypoint.name = "Waypoint " + _ChildCount.ToString();
                newWaypoint.tag = "Waypoint";
            }
            else
            {
                
                newWaypoint.transform.position = waypointSystemRef.transform.position;
                newWaypoint.transform.rotation = waypointSystemRef.transform.rotation;
                newWaypoint.transform.parent = waypointSystemRef.transform;
                newWaypoint.transform.Translate(0, 0, 6, _SpawnLocation);
                newWaypoint.name = "Waypoint " + _ChildCount.ToString();
                isFirst = false;
            }
            
            //_SpawnedWaypointsPositions.Add(newWaypoint.transform.position);			
            _SpawnLocation = newWaypoint.transform;
            waypointSystemRef.WaypointStack.Push(way);
            _ChildCount++;
            Debug.Log("New waypoint created.");
            BuildDirection = TrackbuildDirection.Forward;//Resets the direction to Forward (default)
        }
        if (GUILayout.Button("Undo Waypoint"))
        {
            if(waypointSystemRef.WaypointStack.Count > 1)
            {
                Waypoint undo = waypointSystemRef.WaypointStack.Pop();
                foreach(Transform point in undo.Points)
                {
                    DestroyImmediate(point.gameObject, false);
                    _ChildCount--;
                }
                _SpawnLocation = waypointSystemRef.WaypointStack.Peek().Points[0];
            }
        }

        if (GUILayout.Button("Finished"))//Removes the Create Waypoint System script from the parent game object
        {
            //UnityStandardAssets.Utility.WaypointCircuit circuitScript = waypointSystemRef.GetComponent<UnityStandardAssets.Utility.WaypointCircuit>();
            DestroyImmediate(waypointSystemRef, false);
            Debug.Log("Finished circuit. Click \"Assign using all child objects\" and then make sure \"Smooth Route\" is unchecked.");
        }
    }

    void OnSceneGUI()
    {
        Event e = Event.current;
        if (EventType.KeyDown == e.type)
        {
            if (e.keyCode == KeyCode.LeftControl)
            {
                _CtrlHeld = true;
                
            }
            if (e.keyCode == KeyCode.LeftAlt)
            {
                _AltHeld = true;
            }
            if (e.keyCode == KeyCode.W)
            {
                _WHeld = true;                
            }
        }
        if (EventType.KeyUp == e.type)
        {
            if (e.keyCode == KeyCode.LeftControl)
            {
                _CtrlHeld = false;
            }
            if (e.keyCode == KeyCode.LeftAlt)
            {
                _AltHeld = false;
            }
            if (e.keyCode == KeyCode.W)
            {
                _WHeld = false;
            }
        }

        if (_CtrlHeld && EventType.MouseUp == e.type)
        {
            Debug.Log("Do stuff");
            RaycastHit[] hits = Physics.RaycastAll(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition));
            if(hits.Length >= 1)
            {
                CreateWaypointSystem waypointSystemRef = (CreateWaypointSystem)target;
                Debug.Log(hits[hits.Length - 1].transform.name);
                GameObject way = new GameObject();
                way.transform.position = hits[hits.Length - 1].point;
                way.AddComponent<WaypointGizmo>();
                way.transform.parent = waypointSystemRef.transform;
                way.name = "Waypoint";
            }
        }
    }
}