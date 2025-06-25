using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
[CustomEditor(typeof(RoofBolt))]
public class RoofboltEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RoofBolt bolt = (RoofBolt)target;

        if(GUILayout.Button("Apply Offset to All Roofbolts"))
        {
            RoofBolt[] bolts = GameObject.FindObjectsOfType<RoofBolt>();
            for(int i = 0; i < bolts.Length; i++)
            {
                bolts[i].Offset = bolt.Offset;
            }
        }
        EditorGUILayout.Space();
        if(GUILayout.Button("Set All Bolts to Offset"))
        {
            RoofBolt[] bolts = GameObject.FindObjectsOfType<RoofBolt>();
            for (int i = 0; i < bolts.Length; i++)
            {
                bolts[i].MoveDownByOffset();
            }
        }
        EditorGUILayout.Space();
        if(GUILayout.Button("Rotate All Bolts 90 Degrees"))
        {
            RoofBolt[] bolts = GameObject.FindObjectsOfType<RoofBolt>();
            foreach(RoofBolt single in bolts)
            {
                Debug.Log(single.name + " Orig: " + single.transform.rotation.eulerAngles);
                single.transform.Rotate(single.transform.up, 90);
                Debug.Log("New: " + single.transform.rotation.eulerAngles);
            }
        }
        EditorGUILayout.Space();
        if(GUILayout.Button("Get Verts"))
        {
            MeshFilter mf = Selection.activeGameObject.GetComponent<MeshFilter>();
            Vector3[] verts = mf.sharedMesh.vertices;
            foreach (var item in verts)
            {
                Debug.Log($"Vertex: {item.y}");
            }
        }
    }
}
*/