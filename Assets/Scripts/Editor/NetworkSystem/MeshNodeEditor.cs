using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshNode))]
public class MeshNodeEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MeshNode node = (MeshNode)target;
        EditorUtility.SetDirty(node);
        if(GUILayout.Button("Add nodes to Network"))
        {
            node.AddAllNodesToNetwork();
        }
    }
}
