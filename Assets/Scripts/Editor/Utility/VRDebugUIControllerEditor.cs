using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(VRDebugUIController))]
public class VRDebugUIControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var debugUI = (VRDebugUIController)target;

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Show GUI") && debugUI != null)
            {
                debugUI.EnableUI(true);
            }

            if (GUILayout.Button("Hide GUI") && debugUI != null)
            {
                debugUI.EnableUI(false);
            }
        }

        DrawDefaultInspector();
    }
}
