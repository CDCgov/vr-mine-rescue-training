using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0219

[CustomEditor(typeof(FieldBasedProxSystem))]
public class FieldBasedProxSystemEditor : Editor 
{
    private int _displayVis = 0;
    private readonly string[] _visOptions = { "None", "Yellow Zone", "Red Zone" };
    
    static FieldBasedProxSystemEditor()
    {
        //EditorApplication.update += editorUpdate;
    }

    static void editorUpdate()
    {
        Debug.Log("EditorUpdate");
    }

    static void editorDelayCall()
    {

    }

    public override void OnInspectorGUI()
    {
        FieldBasedProxSystem fieldSystem = (FieldBasedProxSystem)target;
        serializedObject.Update();

        fieldSystem.UnitScale = EditorGUILayout.FloatField("Unit Scale", fieldSystem.UnitScale);

        fieldSystem.YellowColor = EditorGUILayout.ColorField("Yellow Zone Color", fieldSystem.YellowColor);
        fieldSystem.RedColor = EditorGUILayout.ColorField("Red Zone Color", fieldSystem.RedColor);

        fieldSystem.YellowColor.a = 1.0f;
        fieldSystem.RedColor.a = 1.0f;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Display Zone Visualization:");
        int newVis = EditorGUILayout.Popup(_displayVis, _visOptions);

        if (newVis != _displayVis)
        {
            _displayVis = newVis;

            switch (_displayVis)
            {
                case 0:
                    fieldSystem.DisableZoneVisualization();
                    break;

                case 1:
                    fieldSystem.EnableZoneVisualization(new ProxSystem.VisOptions(false, true));
                    break;

                case 2:
                    fieldSystem.EnableZoneVisualization(new ProxSystem.VisOptions(true, false));
                    break;
            }

            EditorUtility.SetDirty(target);
        }

        

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        if (GUILayout.Button("Add Generator"))
        {
            FieldGenerator gen = new FieldGenerator();
            if (fieldSystem.FieldGenerators == null)
                fieldSystem.FieldGenerators = new List<FieldGenerator>();


            gen.Ca_ShellBaseShapeConst = 138.2f;
            gen.Da_ShellShapeChangeConst = 0.215f;
            gen.Cb_ShellBaseSizeConst = 1690.4f;
            gen.Db_ShellSizeChangeConst = 0.333f;

            fieldSystem.FieldGenerators.Add(gen);
        }

        var fieldGens = serializedObject.FindProperty("FieldGenerators");
        for (int i = 0; i < fieldGens.arraySize; i++)
        {			
            DrawGeneratorInspector(fieldGens, i);
        }

        //foreach (FieldGenerator gen in fieldSystem.FieldGenerators)
        //{

        //}

        //DrawDefaultInspector();

        fieldSystem.GridSpacing = EditorGUILayout.FloatField("Grid Spacing (ft)", fieldSystem.GridSpacing);
        fieldSystem.GridLineCount = EditorGUILayout.IntField("Grid Line Count", fieldSystem.GridLineCount);	
        //fieldSystem.GridSecondDivisor = EditorGUILayout.IntField("Grid Second Divisor", fieldSystem.GridSecondDivisor);
        fieldSystem.GridColor = EditorGUILayout.ColorField("Grid Color", fieldSystem.GridColor);
        //fieldSystem.GridSecondColor = EditorGUILayout.ColorField("Grid Second Color", fieldSystem.GridSecondColor);


        Vector2 gridShift = EditorGUILayout.Vector2Field("Grid Shift", fieldSystem.GridShift);
        if (gridShift != fieldSystem.GridShift)
        {
            fieldSystem.GridShift = gridShift;
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }

    int _lastRenderedFrame;

    private Vector3[] _gridLines;

    private void OnSceneGUI()
    {
        FieldBasedProxSystem fieldSystem = (FieldBasedProxSystem)target;

        //Debug.Log(Event.current.type.ToString());

        EditorUtility.SetDirty(fieldSystem.gameObject);

        if (Event.current.type == EventType.Layout && _lastRenderedFrame != Time.renderedFrameCount)
        {
            _lastRenderedFrame = Time.renderedFrameCount;
            fieldSystem.DrawVisShell();
        }

        int numLinePairs = fieldSystem.GridLineCount * 2;
        if (_gridLines == null || _gridLines.Length != numLinePairs * 2)
        {
            _gridLines = new Vector3[numLinePairs * 2];
        }

        int i = 0;

        Vector3 origin = fieldSystem.transform.position;
        origin.x += fieldSystem.GridShift.x;
        origin.z += fieldSystem.GridShift.y;

        float spacing = fieldSystem.GridSpacing * 0.3048f;
        float size = (float)fieldSystem.GridLineCount * spacing;

        float startX = origin.x - (size / 2);
        float endX = origin.x + (size / 2);
        float startZ = origin.z - (size / 2);
        float endZ = origin.z + (size / 2);
        
        for (int x = 0; x < fieldSystem.GridLineCount; x++)
        {
            Vector3 p1 = new Vector3(startX + x * spacing + spacing/2, origin.y, startZ);
            Vector3 p2 = new Vector3(startX + x * spacing + spacing/2, origin.y, endZ);

            _gridLines[i * 2] = p1;
            _gridLines[(i * 2) + 1] = p2;

            i++;
        }

        for (int z = 0; z < fieldSystem.GridLineCount; z++)
        {
            Vector3 p1 = new Vector3(startX, origin.y, startZ + z * spacing + spacing/2);
            Vector3 p2 = new Vector3(endX, origin.y, startZ + z * spacing + spacing/2);

            _gridLines[i * 2] = p1;
            _gridLines[(i * 2) + 1] = p2;

            i++;
        }

        Handles.color = fieldSystem.GridColor;
        Handles.DrawLines(_gridLines);

        //fieldSystem.DrawVisShell();
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
    public static void OnDrawFieldBasedProxGizmos(FieldBasedProxSystem prox, GizmoType gizmoType)
    {
        //prox.DrawVisShell(1.5f);
        //fieldSystem.DrawVisShell();
    }

    private void DrawGeneratorInspector(SerializedProperty generatorArray, int index)
    {
        FieldBasedProxSystem fieldSystem = (FieldBasedProxSystem)target;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        var generator = generatorArray.GetArrayElementAtIndex(index);

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(generator, true);
        EditorGUILayout.Slider(generator.FindPropertyRelative("B_Yellow"), 0.00001f, 1.8f);
        EditorGUILayout.Slider(generator.FindPropertyRelative("B_Red"), 0.00001f, 1.8f);
        EditorGUI.indentLevel--;
        if (GUILayout.Button("Delete"))
        {
            generatorArray.DeleteArrayElementAtIndex(index);
        }

        FieldGenerator gen = fieldSystem.FieldGenerators[index];

        //gen.B_Yellow = EditorGUILayout.Slider(gen.B_Yellow, 0.00001f, 0.5f);
        //gen.B_Red = EditorGUILayout.Slider(gen.B_Red, 0.00001f, 0.5f);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("No Plate"))
        {
            generator.FindPropertyRelative("Ca_ShellBaseShapeConst").floatValue = 138.2f;
            generator.FindPropertyRelative("Da_ShellShapeChangeConst").floatValue = 0.215f;
            generator.FindPropertyRelative("Cb_ShellBaseSizeConst").floatValue = 1690.4f;
            generator.FindPropertyRelative("Db_ShellSizeChangeConst").floatValue = 0.333f;
        }

        if (GUILayout.Button("With Plate"))
        {
            generator.FindPropertyRelative("Ca_ShellBaseShapeConst").floatValue = 204.87f;
            generator.FindPropertyRelative("Da_ShellShapeChangeConst").floatValue = 0.307f;
            generator.FindPropertyRelative("Cb_ShellBaseSizeConst").floatValue = 2201.0f;
            generator.FindPropertyRelative("Db_ShellSizeChangeConst").floatValue = 0.366f;
        }
        EditorGUILayout.EndHorizontal();


        float distX = FieldBasedProxSystem.ComputeShellSurfaceDistance(new Vector3(1, 0, 0), 
            gen.B_Yellow, gen.Ca_ShellBaseShapeConst, gen.Da_ShellShapeChangeConst, 
            gen.Cb_ShellBaseSizeConst, gen.Db_ShellSizeChangeConst, 
            fieldSystem.UnitScale);
        //Debug.LogFormat("Distance: {0}", currentDist);

        float distZ = FieldBasedProxSystem.ComputeShellSurfaceDistance(new Vector3(0, 0, 1),
            gen.B_Yellow, gen.Ca_ShellBaseShapeConst, gen.Da_ShellShapeChangeConst,
            gen.Cb_ShellBaseSizeConst, gen.Db_ShellSizeChangeConst,
            fieldSystem.UnitScale);

        //Debug.LogFormat("Shell Dist X: {0:F3} Z: {1:F3}", currentDist, distZ);

        if (gen.Position != null)
        {
            Vector3 wpos = gen.Position.position;
            EditorGUILayout.LabelField(string.Format("DistX (ft): {0:F2}", distX * 3.28084f));
            EditorGUILayout.LabelField(string.Format("DistZ (ft): {0:F2}", distZ * 3.28084f));
        }

        GUILayout.EndVertical();
    }
}