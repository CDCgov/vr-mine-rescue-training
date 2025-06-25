using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomXRInteractable))]
public class CustomXRInteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CustomXRInteractable interactable = (CustomXRInteractable)target;

        if (Application.isPlaying)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Show Highlight"))
            {
                interactable.ShowSelectHighlight();
            }

            if (GUILayout.Button("Clear Highlight"))
            {
                
            }
            

            GUILayout.EndVertical();
        }

        DrawDefaultInspector();
    }
}
