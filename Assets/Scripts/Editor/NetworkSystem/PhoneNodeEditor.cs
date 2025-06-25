using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhoneNode))]
public class PhoneNodeEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PhoneNode node = (PhoneNode)target;
        GameObject[] newSelection = new GameObject[1];
        EditorUtility.SetDirty(node);
        int count = 0;
        if(node.NextPhones != null)
        {
            count = node.NextPhones.Count;
        }
        if (node.PhoneType == PhoneNodeType.Node)
        {            
            if (GUILayout.Button("Add node"))
            {
                newSelection[0] = node.CreateNode();
                Selection.objects = newSelection;
                EditorUtility.SetDirty(newSelection[0]);
            }                
            if (GUILayout.Button("Add phone"))
            {
                newSelection[0] = node.AddNewPhone();
                Selection.objects = newSelection;
                EditorUtility.SetDirty(newSelection[0]);
            }            
        }
        if(GUILayout.Button("Recalculate Paths"))
        {
            node.ComputeLocalConnectivity();
        }
    }
}
