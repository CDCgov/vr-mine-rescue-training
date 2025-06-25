using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//TODO Weird bug
//Transform parent

[CustomEditor(typeof(MineSegment))]
[CanEditMultipleObjects()]
public class MineSegmentEditor : Editor
{
    bool _showWindow = false;
    static bool _showHandles = false;
    Vector2 _windowPos;
    List<GenerateConnectionTable.TableInfoContainer> matchedList = new List<GenerateConnectionTable.TableInfoContainer>();
    Vector3 _lastHandlePos = new Vector3();
    Vector3 _lastRotationPos = new Vector3();
    Vector3 _lastUpVector = new Vector3();
    int _conIndex = 0;


    [MenuItem("Create Mine/Mine Segments/Show \u2215 Hide Add Segment Cubes", priority = 200)]
    public static void ToggleButton()
    {
        //if (_showHandles)
        //{
        //    _showHandles = false;


        //} else
        //{
        //    _showHandles = true;            
        //}
        _showHandles = !_showHandles;//simplified -bdm

        if (_showHandles)
            Debug.Log("Showing add segment interface");
        else
            Debug.Log("Hiding add segment interface");
    }

    [MenuItem("Create Mine/Mine Segments/Swap Selected Tiles", priority = 200)]
    public static void SwapTiles()
    {
        GameObject[] selectedGOs = Selection.gameObjects;
        List<GameObject> filteredGOs = new List<GameObject>();
        for (int i = 0; i < selectedGOs.Length; i++)
        {
            if (selectedGOs[i].GetComponent<MineSegment>() != null)
            {
                filteredGOs.Add(selectedGOs[i]);
            }
        }
        SwapMineTileWindow window = new SwapMineTileWindow();
        window.SelectedTiles = filteredGOs;


        //string path = Path.Combine(Application.dataPath, "/Tilesets/");
        DirectoryInfo dirInfo = new DirectoryInfo("Assets/Tilesets");
        DirectoryInfo[] directs = dirInfo.GetDirectories();
        string[] paths = new string[directs.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i] = directs[i].ToString();
        }
        //CreateMineWindow.Tilesets = Directory.GetDirectories(path);
        window._tilesets = paths;
        window._tilesetsTruncated = new string[window._tilesets.Length];
        for (int i = 0; i < window._tilesetsTruncated.Length; i++)
        {
            string stopAt = "\\";
            int lastIndex = window._tilesets[i].Length - 1;
            while (!window._tilesets[i].Substring(lastIndex, 1).Equals(stopAt))
            {
                lastIndex--;
            }
            int location = lastIndex;
            window._tilesetsTruncated[i] = window._tilesets[i].Substring(location + 1);
        }

        window.paths = Directory.GetFiles(paths[0]);

        window.Show();
    }

    void OnEnable()
    {
        //lookAtPoint = serializedObject.FindProperty("lookAtPoint");
    }

    public override void OnInspectorGUI()
    {
        MineNetwork.InitializeLinks();
        serializedObject.Update();
        //EditorGUILayout.PropertyField(lookAtPoint);
        serializedObject.ApplyModifiedProperties();

        //if (targets.Length == 1)
        //{
        //	MineSegment seg = (MineSegment)target;
        //	if (GUILayout.Button("Apply Changes to Prefab"))
        //	{

        //		seg.ClearSegmentGeometry();
        //		//PrefabUtility.ReconnectToLastPrefab(seg.gameObject);
        //		PrefabUtility.ReplacePrefab(seg.gameObject, PrefabUtility.GetCorrespondingObjectFromSource(seg.gameObject), ReplacePrefabOptions.ConnectToPrefab);
        //	}
        //}

        //if (GUILayout.Button("Rebuild/Reload Segment Geometry"))
        //{
        //	foreach (Object obj in targets)
        //	{
        //		((MineSegment)obj).RebuildGeometry();
        //	}
        //}

        if (targets.Length == 2)
        {
            if (GUILayout.Button("Link Selected Segments"))
            {

                MineSegment seg1 = (MineSegment)targets[0];
                MineSegment seg2 = (MineSegment)targets[1];


                MineNetwork.LinkSegments(seg1, seg2);
            }
        }

        if (targets.Length == 1)
        {
            var segment = ((MineSegment)targets[0]);

            if (GUILayout.Button("Add Teamstop"))
            {
                var teamstopParentObj = GameObject.Find("TeamStops");
                Transform teamstopParent = null;
                if (teamstopParentObj != null)
                    teamstopParent = teamstopParentObj.transform;

                var teamstopObj = new GameObject("Teamstop");
                teamstopObj.transform.SetParent(teamstopParent);
                teamstopObj.transform.position = segment.transform.position;
                teamstopObj.transform.rotation = segment.transform.rotation;
                var poi = teamstopObj.AddComponent<VRPointOfInterest>();
                poi.POIType = POIType.CameraPosition;
                poi.WorldScaleMultiplier = 1;
            }

            GUILayout.TextField(string.Format("Linked to {0} MineSegments", segment.MineSegmentLinks.Count));
        }

        DrawDefaultInspector();

        if (targets.Length == 1)
        {
            GUILayout.TextArea(((MineSegment)target).DebugText);
        }
    }
    private void OnSceneGUI()
    {
        MineSegment segment = (MineSegment)target;

        MineNetwork.InitializeLinks();

        //draw MFire Debug Info
        foreach (MineSegmentLink link in segment.MineSegmentLinks)
        {
            Handles.Label(link.GetWorldSpaceCenter(), string.Format("Airway {0}", link.MFireAirway));
        }

        // Debug.LogFormat("SegmentConnections: {0}", segment.SegmentConnections);
        if (_showHandles)
        {
            segment.InitializeSegmentConnections();

            if (segment.SegmentConnections != null)
            {

                for (int i = 0; i < segment.SegmentConnections.Length; i++)
                {
                    SegmentConnectionInfo connInfo = segment.SegmentConnections[i];
                    //Debug.LogFormat("Connection: {0}", connInfo);

                    if (connInfo != null)
                        continue;

                    if (segment.SegmentGeometry != null)
                    {
                        var conn = segment.SegmentGeometry.SegmentConnections[i];

                        Vector3 worldSpacePos = segment.transform.TransformPoint(conn.Centroid);
                        Vector3 rotationWorldSpacePos = segment.transform.TransformDirection(conn.Normal * -1);
                        _lastUpVector = segment.transform.TransformDirection(Vector3.up);

                        if (Handles.Button(worldSpacePos, segment.transform.rotation, 1.0f, 1.0f, Handles.CubeHandleCap))
                        {
                            if (GenerateConnectionTable.MatchMap == null)
                            {
                                GenerateConnectionTable.CreateMatchMap();
                            }
                            matchedList = GenerateConnectionTable.MatchMap[conn.ConnectionID];
                            _lastHandlePos = worldSpacePos;
                            _lastRotationPos = rotationWorldSpacePos;

                            _windowPos = Event.current.mousePosition;
                            _conIndex = i;
                            _showWindow = true;

                        }
                    }
                }
            }
        }
        if (_showWindow)
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(_windowPos, new Vector2(300, 300)));

            GUILayout.BeginVertical();
            foreach (GenerateConnectionTable.TableInfoContainer cont in matchedList)
            {
                if (GUILayout.Button(cont.GameObj.GetComponent<MineSegment>().name))
                {
                    _showWindow = false;
                    GameObject newSection = PrefabUtility.InstantiatePrefab(cont.GameObj) as GameObject;

                    Vector3 newUpVector = newSection.transform.TransformDirection(Vector3.up);
                    Vector3 newRotation = newSection.transform.TransformDirection(cont.SegmentConnection.Normal);

                    Quaternion targetQ = Quaternion.LookRotation(_lastRotationPos, _lastUpVector);
                    Quaternion newQ = Quaternion.LookRotation(newRotation, newUpVector);

                    newSection.transform.rotation = (Quaternion.Inverse(newQ)) * targetQ;


                    Vector3 newCentroidWorldSpacePos = newSection.transform.TransformPoint(cont.SegmentConnection.Centroid);
                    newSection.transform.position = _lastHandlePos - newCentroidWorldSpacePos;

                    newSection.transform.parent = segment.transform.parent;
                }
            }

            if (GUILayout.Button("Create Branch"))//TODO: Make branch sections compatible with the connection table utiltiy
            {
                Debug.Log("Make a branch!");
                CreateMineContextWindow window = EditorWindow.CreateInstance<CreateMineContextWindow>();
                Vector3 centroid = segment.SegmentGeometry.SegmentConnections[_conIndex].Centroid;
                window.Position = segment.transform.position + segment.SegmentGeometry.SegmentConnections[_conIndex].Centroid + segment.SegmentGeometry.SegmentConnections[_conIndex].Centroid;
                CreateMineContextWindow.Root = segment.transform.root.gameObject;
                float x = 0;
                float z = 0;
                if (centroid.x > 0)
                {
                    x = 1;
                }
                if (centroid.x < 0)
                {
                    x = -1;
                }
                if (centroid.z > 0)
                {
                    z = 1;
                }
                if (centroid.z < 0)
                {
                    z = -1;
                }
                window.Direction = new Vector3(x, 0, z);
                Debug.Log("Segment spawnloc: " + window.Position + ", " + window.Direction);
                window.Show();
                _showWindow = false;
            }
            GUILayout.EndVertical();
            //GUILayout.Window(0, new Rect(Event.current.mousePosition, new Vector2(100, 100)),

            GUILayout.EndArea();
            HandleUtility.Repaint();

            Handles.EndGUI();
        }
    }
}