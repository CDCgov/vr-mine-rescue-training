using UnityEngine;
using UnityEditor;

public class HangingGeometryEditor : Editor
{
    protected const float RoofboltSearchSize = 300;
    //Original HookPos 0.0593f, -0.0042f, 0.0722f
    protected readonly Vector3[] RoofBoltHookPosition = { new Vector3(0.07f, -0.0042f, -0.0722f), new Vector3(-0.07f, -0.0042f, 0.0722f), new Vector3(0.07f, -0.0042f, 0) };

    protected bool _showMoveHandles = false;
    //private bool _addCableNodeMode = false;
    private string[] _hangPositions = { "Default", "Secondary", "Lifeline" };
    private int _hangPointSelection;

    protected enum NodeAddMode
    {
        AddModeOff,
        AddHangingNode,
        AddFixedNode,
        AddFloorNode,
    }

    protected static NodeAddMode _addMode;

    
    Tool LastTool = Tool.None;
    protected Vector2 _mouseDownPosition;
    protected HangingGeometry.CableNode _selectedNode;

    private void Awake()
    {
        //Debug.Log("HangingGeometryEditor Awake");
        _addMode = NodeAddMode.AddModeOff;
        EditorUtil.ClearAssetSearchCache();
    }


    [MenuItem("Create Mine/Create Hanging Cable", priority = 500)]
    public static void CreateNewHangingCable()
    {
        GameObject obj = new GameObject("HangingCable");
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>();

        HangingCable cable = obj.AddComponent<HangingCable>();

        var selection = new Object[1];
        selection[0] = cable.gameObject;
        Selection.objects = selection;

    }

    public void SetHangPosition(int position)
    {
        _hangPointSelection = position;
    }

    void OnEnable()
    {
        LastTool = Tools.current;
        Tools.current = Tool.None;
    }

    void OnDisable()
    {
        Tools.current = LastTool;
    }

    protected void ProcessHotkeys()
    {
        if (Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                _addMode = NodeAddMode.AddModeOff;

                //make sure any geometry gets cleared from scene view
                EditorUtility.SetDirty(target);
                HandleUtility.Repaint();
                SceneView.RepaintAll();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        HangingGeometry cable = (HangingGeometry)target;


        ProcessHotkeys();

        if (GUILayout.Button("Clear All Nodes"))
        {
            UnityEditor.Undo.RecordObject(target, "Cleared Cable");
            cable.Clear();
            //EditorUtility.SetDirty(target);
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Reset Slope") && cable.CableNodes != null)
        {
            UnityEditor.Undo.RecordObject(target, "Reset Slope");

            for (int i = 0; i < cable.CableNodes.Count; i++)
            {
                cable.CableNodes[i].Slope = -1;
                cable.RegenerateMesh();
            }
        }

        if (GUILayout.Button("Regenerate Mesh"))
        {
            UnityEditor.Undo.RecordObject(target, "Regenerated Mesh");
            cable.RegenerateMesh();
            //EditorUtility.SetDirty(target);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        bool addHanging = GUILayout.Toggle(_addMode == NodeAddMode.AddHangingNode, "Add Hanging Nodes", GUI.skin.button);
        if (addHanging)
            Debug.Log("Add Hanging");
        if (addHanging && (_addMode != NodeAddMode.AddHangingNode))
        {			
            _addMode = NodeAddMode.AddHangingNode;			
        }

        //if (!addHanging && (_addMode != NodeAddMode.AddHangingNode))
        //{
        //	//make sure any geometry gets cleared from scene view
        //	EditorUtility.SetDirty(target);
        //	HandleUtility.Repaint();
        //	SceneView.RepaintAll();
        //}

        bool addFloor = GUILayout.Toggle(_addMode == NodeAddMode.AddFloorNode, "Add Floor Nodes", GUI.skin.button);
        if (addFloor)
            Debug.Log("Add Floor");
        if (addFloor && (_addMode != NodeAddMode.AddFloorNode))
        {
            _addMode = NodeAddMode.AddFloorNode;
        }

        bool addFixed = GUILayout.Toggle(_addMode == NodeAddMode.AddFixedNode, "Add Fixed Nodes", GUI.skin.button);
        if (addFixed)
            Debug.Log("Add Fixed");
        if (addFixed && (_addMode != NodeAddMode.AddFixedNode))
        {
            _addMode = NodeAddMode.AddFixedNode;
        }

        GUILayout.EndHorizontal();

        /*bool showHandles = EditorGUILayout.Toggle("Show Move Handles", _showMoveHandles);
        if (showHandles != _showMoveHandles)
        {
            _showMoveHandles = showHandles;
            EditorUtility.SetDirty(target);
            HandleUtility.Repaint();
        }
        */

        float slope = EditorGUILayout.Slider("Default Slope", cable.DefaultCableSlope, 0, 1.5f);
        if (slope != cable.DefaultCableSlope)
        {
            UnityEditor.Undo.RecordObject(target, "Changed Default Slope");
            cable.DefaultCableSlope = slope;
            cable.RegenerateMesh();
        }

        float diameter = EditorGUILayout.Slider("Cable Diameter", cable.CableDiameter, 0.01f, 0.5f);
        if (diameter != cable.CableDiameter)
        {
            UnityEditor.Undo.RecordObject(target, "Changed Cable Diameter");
            cable.CableDiameter = diameter;
            cable.RegenerateMesh();
        }

        float hangerWidth = EditorGUILayout.Slider("Cable Hanger Width", cable.CableHangerWidth, 0.01f, 0.25f);
        if (hangerWidth != cable.CableHangerWidth)
        {
            UnityEditor.Undo.RecordObject(target, "Changed Hanger Width");
            cable.CableHangerWidth = hangerWidth;
            cable.RegenerateMesh();
        }

        int segsPerMeter = EditorGUILayout.IntSlider("Segments Per Meter", cable.SegmentsPerMeter, 2, 15);
        if (segsPerMeter != cable.SegmentsPerMeter)
        {
            UnityEditor.Undo.RecordObject(target, "Changed Segments Per Meter");
            cable.SegmentsPerMeter = segsPerMeter;
            cable.RegenerateMesh();
        }

        bool checkInterference = GUILayout.Toggle(cable.CheckPointInterference, "Check Interference");
        if (checkInterference != cable.CheckPointInterference)
        {
            UnityEditor.Undo.RecordObject(target, "Changed check interference");
            cable.CheckPointInterference = checkInterference;
            cable.RegenerateMesh();
        }

        float interferenceDist = EditorGUILayout.Slider("Interference Dist", cable.InterferenceDistance, 0, 0.5f);
        if (interferenceDist != cable.InterferenceDistance)
        {
            UnityEditor.Undo.RecordObject(target, "Changed Interference Dist");
            cable.InterferenceDistance = interferenceDist;
            cable.RegenerateMesh();
        }

        string searchString = "l:CableMaterial";
        LifelineMarkerGenerator LLGenerator = cable.GetComponent<LifelineMarkerGenerator>();
        if (LLGenerator == null)
        {
            if (EditorUtil.MaterialSelector("Cable Material", searchString, ref cable.CableMaterial))
            {
                cable.UpdateMaterial();
            }
            if (EditorUtil.ObjectSelector<GameObject>("Hanger Prefab", "l:CableHanger", ref cable.CableHangerPrefab,
                target, "Changed cable hanger prefab"))
            {

            }
        }
        else
        {
            if (EditorUtil.MaterialSelector("Cable Material", "l:LifelineMaterial", ref cable.CableMaterial))
            {
                cable.UpdateMaterial();
            }
            if (EditorUtil.ObjectSelector<GameObject>("Hanger Prefab", "l:LifelineHanger", ref cable.CableHangerPrefab,
                target, "Changed cable hanger prefab"))
            {

            }            
        }
        _hangPointSelection = EditorGUILayout.Popup("Hanger Position", cable.CableHangerPosition, _hangPositions);
        if(LLGenerator != null)
        {
            _hangPointSelection = 2;
        }
        if(_hangPointSelection != cable.CableHangerPosition)
        {
            UnityEditor.Undo.RecordObject(target, "Changed Hanger Position");
            cable.CableHangerPosition = _hangPointSelection;
        }
        if (GUILayout.Button("Snap to bolts"))
        {
            UnityEditor.Undo.RecordObject(target, "Snapped to roofbolts");

            foreach (HangingGeometry.CableNode node in cable.CableNodes)
            {
                if (node.ShowCableHanger)
                {
                    Transform roofBoltTransform;
                    if (FindRoofBoltWorld(node.Position, out roofBoltTransform))
                    {
                        node.Position = roofBoltTransform.TransformPoint(RoofBoltHookPosition[_hangPointSelection]);
                    }
                }
            }

            cable.RegenerateMesh();
        }

        if (_selectedNode != null)
        {
            EditorGUILayout.LabelField("Selected Node Attributes:");
            
            bool customSlope = EditorGUILayout.Toggle("Custom Slope", _selectedNode.Slope >= 0);
            if (customSlope && _selectedNode.Slope < 0)
            {
                UnityEditor.Undo.RecordObject(target, "Changed Node Slope");
                _selectedNode.Slope = 0;
                cable.RegenerateMesh();

            }
            else if (!customSlope && _selectedNode.Slope >= 0)
            {
                UnityEditor.Undo.RecordObject(target, "Changed Node Slope");
                _selectedNode.Slope = -1;
                cable.RegenerateMesh();
            }

            //if custom slope, show slider
            if (_selectedNode.Slope >= 0)
            {
                slope = EditorGUILayout.Slider("Node Slope", _selectedNode.Slope, 0, 1.5f);
                if (slope != _selectedNode.Slope)
                {
                    UnityEditor.Undo.RecordObject(target, "Changed Node Slope");
                    _selectedNode.Slope = slope;
                    cable.RegenerateMesh();
                }
            }

            bool showHanger = EditorGUILayout.Toggle("Show Hanger", _selectedNode.ShowCableHanger);
            if (showHanger != _selectedNode.ShowCableHanger)
            {
                UnityEditor.Undo.RecordObject(target, "Changed Cable Hanger Visibility");
                _selectedNode.ShowCableHanger = showHanger;
                cable.RegenerateMesh();
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Snapping:");

            if (GUILayout.Button("Snap to Bolt"))
            {
                Transform roofBoltTransform;
                if (FindRoofBoltWorld(_selectedNode.Position, out roofBoltTransform))
                {
                    UnityEditor.Undo.RecordObject(target, "Snapped node to bolt");
                    _selectedNode.Position = roofBoltTransform.TransformPoint(RoofBoltHookPosition[_hangPointSelection]);
                    cable.RegenerateMesh();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Delete Node"))
            {
                UnityEditor.Undo.RecordObject(target, "Deleted Node");
                cable.CableNodes.Remove(_selectedNode);
                _selectedNode = null;
                cable.RegenerateMesh();
                Repaint();
            }

        }

        //DrawDefaultInspector();
    }

    /*
    private void FindRoofbolt(Vector2 screenPos)
    {
        Rect searchRect = new Rect(screenPos.x - RoofboltSearchSize / 2, screenPos.y - RoofboltSearchSize / 2,
            RoofboltSearchSize, RoofboltSearchSize);

        GameObject[] objects = HandleUtility.PickRectObjects(searchRect, false);

        if (objects.Length > 0)
            Debug.LogFormat("Found {0} Objects", objects.Length);

        var go = HandleUtility.PickGameObject(screenPos, false);
        if (go != null)
            Debug.Log(go.name);
    }*/

    protected bool FindRoofBolt(Vector2 screenPos, out Transform roofBoltTransform)
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(screenPos);

        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 100, LayerMask.GetMask("Floor", "Roof", "Walls"),
            QueryTriggerInteraction.Ignore))
        {
            return FindRoofBoltWorld(hit.point, out roofBoltTransform);
        }

        roofBoltTransform = null;
        return false;
    }

    protected bool FindRoofBoltWorld(Vector3 worldPos, out Transform roofBoltTransform)
    {
        var roofbolts = FindObjectsOfType<RoofBolt>();

        float minDist = float.MaxValue;
        GameObject roofbolt = null;

        for (int i = 0; i < roofbolts.Length; i++)
        {
            float dist = Vector3.Distance(worldPos, roofbolts[i].transform.position);
            if (dist < minDist)
            {
                roofbolt = roofbolts[i].gameObject;
                minDist = dist;
            }
        }

        if (roofbolt != null)
        {
            roofBoltTransform = roofbolt.transform;
            return true;
        }
        else
        {
            roofBoltTransform = null;
            return false;
        }
    }

    protected Mesh _sphereMesh;
    protected Material _sphereMat;

    protected void OnSceneGUI()
    {
        ProcessHotkeys();

        HangingGeometry cable = (HangingGeometry)target;
        Tools.current = Tool.None;

        if (_sphereMesh == null)
        {
            _sphereMesh = ProcSphere.GenSphere(0.3f);
            _sphereMat = new Material(Shader.Find("Unlit/Color"));
            _sphereMat.color = Color.green;
        }

        /*
        if (Event.current.isMouse)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            Vector3 roofboltPos;
            if (FindRoofBolt(Event.current.mousePosition, out roofboltPos))
            {
                //Debug.Log(roofboltPos);
            }

            //sw.Stop();
            //Debug.LogFormat("Roofbolt search took {0} ms", sw.ElapsedMilliseconds);
        } */
        //Debug.Log(_addMode.ToString());
        //Debug.Log(Event.current.type.ToString());
        if (_addMode != NodeAddMode.AddModeOff)
        {
            
            //intercept scene clicks if we are in add-node mode
            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(0);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                _mouseDownPosition = Event.current.mousePosition;
            }

            Vector2 mousePos = Event.current.mousePosition;
            Transform roofBoltTransform;

            //show roofbolt snap position in hanging node add mode
            if (_addMode == NodeAddMode.AddHangingNode)
            {
                if (FindRoofBolt(mousePos, out roofBoltTransform))
                {
                    //Handles.SphereHandleCap(0, roofBoltPos, Quaternion.identity, 1.0f, EventType.MouseDown);

                    Graphics.DrawMesh(_sphereMesh, roofBoltTransform.position, Quaternion.identity, _sphereMat, 0);
                    //Debug.DrawLine(Vector3.zero, roofBoltPos);
                    HandleUtility.Repaint();

                    //for some reason this clears the previously drawn spheres
                    EditorUtility.SetDirty(target);
                }
            }

            //add nodes on left click 
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                Vector2 mouseDelta = _mouseDownPosition - mousePos;

                if (mouseDelta.magnitude < 10)
                {
                    
                    switch (_addMode)
                    {
                        case NodeAddMode.AddHangingNode:
                            AddCableNode(cable, mousePos, true, true);
                            break;

                        case NodeAddMode.AddFixedNode:
                            AddCableNode(cable, mousePos, true, false);
                            break;

                        case NodeAddMode.AddFloorNode:
                            AddCableNode(cable, mousePos, false, false);
                            break;

                    }
                    
                    
                }
            } 
        }

        if (cable.CableNodes != null && cable.CableNodes.Count > 0)
        {
            for (int i = 0; i < cable.CableNodes.Count; i++)
            {
                HangingGeometry.CableNode node = cable.CableNodes[i];

                if (_showMoveHandles)
                {
                    Vector3 newPos = Handles.PositionHandle(node.Position, Quaternion.identity);
                    if (newPos != node.Position)
                    {
                        UnityEditor.Undo.RecordObject(target, "Moved Cable Node");
                        node.Position = newPos;
                        cable.RegenerateMesh();
                        //EditorUtility.SetDirty(target);
                    }
                }
                else
                {
                    if (_selectedNode == node)
                    {
                        Vector3 newPos = Handles.PositionHandle(node.Position, Quaternion.identity);
                        if (newPos != node.Position)
                        {
                            UnityEditor.Undo.RecordObject(target, "Moved Cable Node");
                            node.Position = newPos;
                            cable.RegenerateMesh();
                        }
                    }
                    else
                    {
                        Handles.color = Color.white;

                        if (Handles.Button(node.Position, Quaternion.identity, 0.15f, 0.15f, Handles.SphereHandleCap))
                        {
                            _selectedNode = node;
                            Repaint();
                        }
                    }
                }
            }
        }

    }

    protected void AddCableNode(HangingGeometry cable, Vector2 screenPos, bool showHanger, bool snapToBolt)
    {
        Debug.Log($"adding cable node to {cable.name}");
        Vector3 nodePos = Vector3.zero;
        bool positionFound = false;

        if (snapToBolt)
        {
            Transform roofBoltTransform;
            HangingGeometry.CableNode lastNode = null;
            if (cable.CableNodes != null)
            {
                if (cable.CableNodes.Count > 0)
                {                    
                    lastNode = cable.CableNodes[cable.CableNodes.Count-1];
                }
            }
            if (FindRoofBolt(screenPos, out roofBoltTransform))
            {
                Vector3 hangerPos = RoofBoltHookPosition[_hangPointSelection];
                Vector3 savePos = hangerPos;
                if (cable.CableNodes != null && _hangPointSelection == 2)
                {
                    if (cable.CableNodes.Count > 0)
                    {
                        Vector3 dir = (roofBoltTransform.position - lastNode.Position).normalized;
                        Vector3 HangerRotation;
                        hangerPos = RoofBoltHookPosition[_hangPointSelection];
                        if (Mathf.Abs(Vector3.Angle(Vector3.forward, dir)) <= 45)
                        {
                            HangerRotation = new Vector3(0, 0, 1);
                        }
                        else if (Mathf.Abs(Vector3.Angle(Vector3.right, dir)) <= 45)
                        {
                            HangerRotation = new Vector3(1, 0, 0);                            
                            hangerPos.z = -savePos.x;
                            hangerPos.x = 0;
                            //roofBoltTransform.Rotate(roofBoltTransform.up, 90);
                        }
                        else if (Mathf.Abs(Vector3.Angle(-Vector3.forward, dir)) <= 45)
                        {
                            HangerRotation = new Vector3(0, 0, -1);
                            //roofBoltTransform.Rotate(roofBoltTransform.up, 180);
                            hangerPos.x = -savePos.x;
                        }
                        else
                        {
                            HangerRotation = new Vector3(-1, 0, 0);
                            hangerPos.z = savePos.x;
                            hangerPos.x = 0;
                            //roofBoltTransform.Rotate(roofBoltTransform.up, -90);
                        }
                    }
                }
                //nodePos = roofBoltTransform.TransformPoint(RoofBoltHookPosition[_hangPointSelection]);
                nodePos = roofBoltTransform.TransformPoint(hangerPos);
                positionFound = true;
            }
        }
        else
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(screenPos);

            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit, 100, LayerMask.GetMask("Floor", "Roof", "Walls"),
                QueryTriggerInteraction.Ignore))
            {
                nodePos = hit.point;
                nodePos += (cable.CableDiameter * 0.7f) * hit.normal;
                positionFound = true;
            }
        }

        if (positionFound)
        {
            UnityEditor.Undo.RecordObject(target, "Added Cable Node");
            cable.AppendNode(nodePos, showHanger);
            cable.RegenerateMesh();
        }
    }
}