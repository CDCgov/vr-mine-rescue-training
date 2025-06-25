using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestUnityStopwatch))]
public class TestUnityStopwatchEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TestUnityStopwatch stopwatch = (TestUnityStopwatch)target;

        if (Application.isPlaying)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Start"))
            {
                stopwatch.StartTimer();
            }

            if (GUILayout.Button("Stop"))
            {
                stopwatch.StopTimer();
            }

            if (GUILayout.Button("Reset"))
            {
                stopwatch.ResetTimer();
            }

            var statusText = stopwatch.GetStatusText();
            GUILayout.TextArea(statusText);

            GUILayout.EndVertical();
        }

        DrawDefaultInspector();
    }
}
