using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SegmentGeometry))]
public class SegmentGeometryEditor : Editor 
{

    void OnSceneGUI()
    {
        SegmentGeometry geom = (SegmentGeometry)target;

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < geom.SegmentConnections.Length; i++)
        {
            SegmentConnection conn = geom.SegmentConnections[i];

            //conn.Centroid = Handles.DoPositionHandle(conn.Centroid, Quaternion.identity);
            Vector3 worldCentroid = geom.transform.TransformPoint(conn.Centroid);
            worldCentroid = Handles.PositionHandle(worldCentroid, geom.transform.rotation);
            Handles.Label(worldCentroid, string.Format("ConnPt:{0}", i));
            conn.Centroid = geom.transform.InverseTransformPoint(worldCentroid);

            
            
        }

        EditorGUI.EndChangeCheck();
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        //EditorGUILayout.PropertyField(lookAtPoint);
        serializedObject.ApplyModifiedProperties();


        

        DrawDefaultInspector();
    }
}