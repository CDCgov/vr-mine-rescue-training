using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Text;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(VentilationControl))]
public class VentilationControlEditor : Editor
{
    private SerializedProperty _networkManager;
    private SerializedProperty _ventilationManager;
    private SerializedProperty _staticVentilationManager;

    //private SerializedProperty _defaultAtmosphere;
    private SerializedProperty _ventilationProvider;

    private SerializedProperty _defaultVectorFieldFile;
    private SerializedProperty _defaultGasFieldField;

    private SerializedProperty _vfxBounds;
    private SerializedProperty _autoInitializeVentilation;

    private VentJunction _selectedJunction = null;
    private VentJunction _lastJunction = null;
    private VentAirway _selectedAirway = null;

    private bool _linkJunctions = false;
    private bool _addJunction = false;

    private StringBuilder _sb;

    private void OnEnable()
    {
        _networkManager = serializedObject.FindProperty(nameof(VentilationControl.NetworkManager));
        _ventilationManager = serializedObject.FindProperty(nameof(VentilationControl.VentilationManager));
        _staticVentilationManager = serializedObject.FindProperty(nameof(VentilationControl.StaticVentilationManager));

        //_defaultAtmosphere = serializedObject.FindProperty(nameof(VentilationControl.DefaultAtmosphere));
        _ventilationProvider = serializedObject.FindProperty(nameof(VentilationControl.VentilationProvider));
        _vfxBounds = serializedObject.FindProperty(nameof(VentilationControl.VFXBounds));

        _autoInitializeVentilation = serializedObject.FindProperty(nameof(VentilationControl.AutoInitializeVentilation));

        _defaultVectorFieldFile = serializedObject.FindProperty(nameof(VentilationControl.DefaultVectorFieldFile));
        _defaultGasFieldField = serializedObject.FindProperty(nameof(VentilationControl.DefaultGasFieldFile));

        _sb = new StringBuilder();
    }

    public override void OnInspectorGUI()
    {
        var ventControl = (VentilationControl)target;

        EditorGUILayout.PropertyField(_networkManager);
        EditorGUILayout.PropertyField(_ventilationManager);
        EditorGUILayout.PropertyField(_staticVentilationManager);

        EditorGUILayout.PropertyField(_ventilationProvider);

        EditorGUILayout.PropertyField(_defaultVectorFieldFile);
        EditorGUILayout.PropertyField(_defaultGasFieldField);

        //EditorGUILayout.PropertyField(_defaultAtmosphere);
        EditorGUILayout.PropertyField(_vfxBounds);

        EditorGUILayout.PropertyField(_autoInitializeVentilation);

        serializedObject.ApplyModifiedProperties();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Test");
        //GUILayout.Button("Test Button");
        //GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();

        _addJunction = GUILayout.Toggle(_addJunction, "Add new junction", EditorStyles.miniButton);

        _linkJunctions = GUILayout.Toggle(_linkJunctions, "Link selected junctions", EditorStyles.miniButton);

        GUILayout.EndHorizontal();

        if (_selectedJunction != null)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            _sb.Clear();
            _sb.AppendFormat("Selected Junction: {0}\n", _selectedJunction.JunctionID);
            _sb.AppendFormat("Position: {0:F1},{1:F1},{2:F1}", _selectedJunction.WorldPosition.x, _selectedJunction.WorldPosition.y, _selectedJunction.WorldPosition.z);

            GUILayout.Label(_sb.ToString());

            _selectedJunction.IsStartJunction = GUILayout.Toggle(_selectedJunction.IsStartJunction, "Start junction");
            _selectedJunction.IsInAtmosphere = GUILayout.Toggle(_selectedJunction.IsInAtmosphere, "Is in atmosphere");

            if (GUILayout.Button("Delete junction"))
            {
                if (ventControl.VentGraph != null)
                {
                    Undo.RecordObject(ventControl, "Delete junction");
                    ventControl.VentGraph.RemoveJunction(_selectedJunction.JunctionID);
                    _selectedJunction = null;
                    _lastJunction = null;

                    EditorUtility.SetDirty(target);
                    EditorSceneManager.MarkAllScenesDirty();

                    //EditorUtility.SetDirty(target);
                    Repaint();
                }
            }

            GUILayout.EndVertical();
        }

        if (_selectedAirway != null)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.EndVertical();

            _sb.Clear();
            _sb.AppendFormat("Selected airway: {0}\n", _selectedAirway.AirwayID);
            _sb.AppendFormat("Start: {0}\n", _selectedAirway.Start.JunctionID);
            _sb.AppendFormat("End: {0}", _selectedAirway.End.JunctionID);

            GUILayout.Label(_sb.ToString());

            GUINumberField("Added resistance:", ref _selectedAirway.AddedResistance);
            GUINumberField("Override resistance:", ref _selectedAirway.OverrideResistance);
            GUINumberField("CH4 Emission Rate:", ref _selectedAirway.MFAirway.CH4EmissionRateAirway);

            if (GUILayout.Button("Delete airway"))
            {
                if (ventControl.VentGraph != null)
                {
                    Undo.RecordObject(ventControl, "Delete airway");
                    ventControl.VentGraph.RemoveAirway(_selectedAirway.AirwayID);
                    _selectedAirway = null;

                    EditorUtility.SetDirty(target);
                    EditorSceneManager.MarkAllScenesDirty();

                    //EditorUtility.SetDirty(target);
                    Repaint();
                }
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rebuild vent network"))
        {
            BuildSampleNetwork();
        }

        GUILayout.EndHorizontal();

        //EditorGUILayout.PropertyField()

        //GUILayout.BeginVertical();

        //DrawDefaultInspector();


        //GUILayout.EndVertical();
    }

    bool GUINumberField(string label, ref double value)
    {
        float newValue;
        bool changed = false;

        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        string strVal = value.ToString();
        string newStrVal = GUILayout.TextField(strVal);
        if (strVal != newStrVal)
        {
            if (float.TryParse(newStrVal, out newValue))
            {
                changed = true;
                value = newValue;
            }
        }
        GUILayout.EndHorizontal();

        if (changed)
        {
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkAllScenesDirty();
        }
        return changed;
    }

    void BuildSampleNetwork()
    {
        var ventControl = (VentilationControl)target;

        var ventGraph = new VentGraph();
        var diagonalList = new List<int>();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Vector3 pos = new Vector3(i * 3, 0, j * 3);

                var junc = new VentJunction();
                junc.MFireID = -1;
                junc.JunctionID = i;
                junc.WorldPosition = pos;

                int juncID = ventGraph.AddJunction(junc);

                if (i == j)
                    diagonalList.Add(juncID);
            }
        }

        for (int i = 1; i < diagonalList.Count; i++)
        {
            var j1 = ventGraph.FindJunction(diagonalList[i - 1]);
            var j2 = ventGraph.FindJunction(diagonalList[i]);

            var airway = new VentAirway();

            airway.MFireID = -1;
            airway.Start = j1;
            airway.End = j2;
            airway.CH4EmissionRate = 0;

            ventGraph.AddAirway(airway);
        }

        ventControl.VentGraph = ventGraph;
    }

    private void AddNewJunction(VentGraph graph)
    {
        var worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        
        float rayDist;
        Plane groundPlane = new Plane(Vector3.up, 0);
        if (!groundPlane.Raycast(worldRay, out rayDist))
            return;        

        var worldPt = worldRay.GetPoint(rayDist);

        //Debug.Log($"Ray hit {worldPt}");

        var junc = new VentJunction();
        junc.MFireID = -1;
        junc.WorldPosition = worldPt;

        int juncID = graph.AddJunction(junc);

    }

    private void LinkJunctions(VentGraph graph, VentJunction start, VentJunction end)
    {
        if (start == end || start == null || end == null)
            return;

        if (graph.AreJunctionsConnected(start, end))
            return;

        var airway = new VentAirway();

        airway.MFireID = -1;
        airway.Start = start;
        airway.End = end;
        airway.CH4EmissionRate = 0;

        graph.AddAirway(airway);

        EditorUtility.SetDirty(target);
        EditorSceneManager.MarkAllScenesDirty();
    }

    protected virtual void OnSceneGUI()
    {
        var ventControl = (VentilationControl)target;
        var graph = ventControl.VentGraph;

        if (ventControl.VentGraph == null)
            return;

        if (Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseDrag)
        {
            //DrawGUI(ventControl);
        }
        //else
        //    Debug.Log(Event.current.type.ToString());
        DrawGUI(ventControl);

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (_addJunction)
            {
                AddNewJunction(graph);
                //_addJunction = false;
            }
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
        

            Debug.Log($"VentControlEditor: Mouse Click {GUIUtility.hotControl} {Event.current.mousePosition}");
            Event.current.Use();

            _lastJunction = _selectedJunction;
            _selectedJunction = null;
            _selectedAirway = null;
            foreach (var j in graph.GetJunctions())
            {
                //var screenPos = Handles.matrix.MultiplyPoint(j.WorldPosition);
                //var dist = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), Event.current.mousePosition);
                var screenPos = HandleUtility.WorldToGUIPoint(j.WorldPosition);
                var dist = Vector2.Distance(screenPos, Event.current.mousePosition);
                //Debug.Log($"{dist}");

                if (dist < 10)
                {
                    _selectedJunction = j;
                    break;
                }
            }

            if (_linkJunctions && _lastJunction != null && _selectedJunction != null)
            {
                LinkJunctions(graph, _lastJunction, _selectedJunction);
            }

            if (_selectedJunction == null)
            {
                foreach (var a in graph.GetAirways())
                {
                    var sp1 = HandleUtility.WorldToGUIPoint(a.Start.WorldPosition);
                    var sp2 = HandleUtility.WorldToGUIPoint(a.End.WorldPosition);
                    var dist = HandleUtility.DistancePointToLineSegment(Event.current.mousePosition, sp1, sp2);

                    //Debug.Log($"Dist: {dist} ({sp1} -> {sp2})");

                    if (dist < 10)
                    {
                        _selectedAirway = a;
                        break;
                    }
                }
            }

            Repaint();
        }

        //if (Event.current.type == EventType.MouseUp)
        //{
        //    Event.current.Use();
        //    Selection.activeGameObject = ventControl.gameObject;
        //}

        Selection.activeGameObject = ventControl.gameObject;

    }

    void DrawGUI(VentilationControl ventControl)
    {
        var graph = ventControl.VentGraph;
        Vector3 newPos = Vector3.zero;

        EditorGUI.BeginChangeCheck();

        foreach (var j in graph.GetJunctions())
        {
            //Handles.PositionHandle(j.WorldPosition, Quaternion.identity);
            if (_selectedJunction == j)
            {
                Handles.color = Color.green;
                newPos = Handles.PositionHandle(j.WorldPosition, Quaternion.identity);
                //if (j.WorldPosition != newPos)
                //    Debug.Log(newPos);

                _selectedJunction.WorldPosition = newPos;
                
            }
            else
                Handles.color = Color.white;

            Handles.DrawSolidDisc(j.WorldPosition, Vector3.up, 0.33f);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(ventControl, "Moved Vent Node");
            _selectedJunction.WorldPosition = newPos;

            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkAllScenesDirty();
        }

        foreach (var a in graph.GetAirways())
        {
            if (_selectedAirway == a)
                Handles.color = Color.green;
            else
                Handles.color = Color.yellow;
            Handles.DrawDottedLine(a.Start.WorldPosition, a.End.WorldPosition, 5);
        }

        

        Transform transform = ventControl.transform;

        Handles.color = Color.magenta;
        Handles.ArrowHandleCap(0, transform.position, Quaternion.identity, 3.0f, EventType.Repaint);
    }
}
