using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(MineNetwork))]
[CanEditMultipleObjects()]
public class MineNetworkEditor : Editor
{
    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        //EditorGUILayout.PropertyField(lookAtPoint);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Build ventilation graph"))
        {
            var ventControl = FindObjectOfType<VentilationControl>();
            if (ventControl == null)
                throw new System.Exception("Couldn't find VentilationControl");

            ventControl.VentGraph = new VentGraph();

            MineNetwork mineNetwork = (MineNetwork)target;
            mineNetwork.BuildBaseVentGraph(ventControl.VentGraph);

            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkAllScenesDirty();
        }

        if (targets.Length == 1)
        {
            MineNetwork mineNetwork = (MineNetwork)target;

            SerializedProperty xSlopeProp = serializedObject.FindProperty("XSlope");
            SerializedProperty zSlopeProp = serializedObject.FindProperty("ZSlope");
            SerializedProperty yScaleProp = serializedObject.FindProperty("ZSlope");

            float xSlope = EditorGUILayout.Slider("X Slope", mineNetwork.XSlope, -1, 1);
            float zSlope = EditorGUILayout.Slider("Z Slope", mineNetwork.ZSlope, -1, 1);
            float yScale = EditorGUILayout.Slider("Y Scale", mineNetwork.YScale, 0.5f, 3.0f);

            if (xSlope != mineNetwork.XSlope || zSlope != mineNetwork.ZSlope || yScale != mineNetwork.YScale)
            {
                UnityEditor.Undo.RecordObject(target, "World Tilted");

                xSlopeProp.floatValue = xSlope;
                zSlopeProp.floatValue = zSlope;
                yScaleProp.floatValue = yScale;

                mineNetwork.XSlope = xSlope;
                mineNetwork.ZSlope = zSlope;
                mineNetwork.YScale = yScale;

                MineNetworkCommands.RebuildAllGeometry();
            }
        }

        DrawDefaultInspector();
    }

}
