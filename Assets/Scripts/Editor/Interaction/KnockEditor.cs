using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Knock))]
public class KnockEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var knock = (Knock)target;

        if (GUILayout.Button("Test Knock"))
        {
            knock.PlayKnock();
        }

        if (GUILayout.Button("Test Sounding Stick Knock"))
        {
            knock.SoundingStickKnock();
        }

        base.OnInspectorGUI();
    }
}
