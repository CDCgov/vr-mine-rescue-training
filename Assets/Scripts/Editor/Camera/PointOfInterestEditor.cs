using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CustomEditor(typeof(PointOfInterest), editorForChildClasses:true)]
public class PointOfInterestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var poi = (PointOfInterest)target;

        //var sb = new StringBuilder();
        //sb.AppendLine($"POI Name: {poi.Name}");
        //sb.AppendLine($"POI ID: {poi.ID}");

        //EditorGUILayout.LabelField(sb.ToString());

        EditorGUILayout.LabelField($"POI Name: {poi.Name}");
        EditorGUILayout.LabelField($"POI ID: {poi.ID}");

    }
}
