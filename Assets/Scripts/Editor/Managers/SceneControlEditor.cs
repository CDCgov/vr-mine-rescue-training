using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SceneControl))]
public class SceneControlEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Rebuild Scene Object List"))
        {
            BuildSceneObjectList();
        }

        DrawDefaultInspector();
    }

    private void BuildSceneObjectList()
    {
        SceneControl sc = (SceneControl)target;

        //List<NetworkedObject> objList = new List<NetworkedObject>();


        UnityEditor.Undo.RecordObject(target, "Rebuild Scene Object List");
        NetworkedObject[] netObjs = GameObject.FindObjectsOfType<NetworkedObject>();
        sc.SceneNetworkedObjects = netObjs;
        //EditorSceneManager.MarkAllScenesDirty();
        //EditorSceneManager.MarkSceneDirty();
        //serializedObject.Update();

        UnityEditor.Undo.RecordObjects(netObjs, "Set scene id on networked objects");
        for (int i = 0; i < netObjs.Length; i++)
        {
            netObjs[i].SceneID = i;
        }

        //sc.SceneNetworkedObjects = objList.ToArray();
    }
}