using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ProxSystemController))]
public class ProxSystemControllerEditor : Editor
{
    string _machineStateName;

    public override void OnInspectorGUI()
    {
        ProxSystemController prox = (ProxSystemController)target;

        EditorGUILayout.LabelField("Prox Machine States:");

        if (prox.MachineStates == null || prox.MachineStates.Count <= 0)
        {
            prox.MachineStates = new List<string>();
            prox.MachineStates.Add("Default");
        }

        for (int i = 0; i < prox.MachineStates.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(prox.MachineStates[i]);
            if (GUILayout.Button("X"))
            {
                prox.MachineStates.RemoveAt(i);
                i--;
                prox.RebuildStateMap();
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        _machineStateName = EditorGUILayout.TextField(_machineStateName);
        if (GUILayout.Button("Add") && _machineStateName.Length > 0)
        {
            if (prox.MachineStates.Find((x) => _machineStateName == x) == null)
            {
                prox.MachineStates.Add(_machineStateName);
                _machineStateName = "";
                prox.RebuildStateMap();
                EditorUtility.SetDirty(target);
            }
        }
        EditorGUILayout.EndHorizontal();


        GUILayout.Space(15);

        var mwcUserNames = System.Enum.GetNames(typeof(ProxMWCUserType));
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < mwcUserNames.Length; i++)
        {
            GUILayout.Label(mwcUserNames[i]);
        }
        EditorGUILayout.EndHorizontal();

        if (prox.StateMap != null)
        {

            foreach (ProxMachineStateMap map in prox.StateMap.Values)
            {
                GUILayout.Label(map.MachineState);
                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < map.SystemMap.Length; i++)
                {
                    var newSystem = (ProxSystem)EditorGUILayout.ObjectField((Object)map.SystemMap[i], typeof(ProxSystem), true);
                    if (newSystem != map.SystemMap[i])
                    {
                        map.SystemMap[i] = newSystem;
                        EditorUtility.SetDirty(target);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        DrawDefaultInspector();
    }
}