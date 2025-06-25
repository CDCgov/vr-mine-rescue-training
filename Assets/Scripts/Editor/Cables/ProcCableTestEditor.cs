using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ProcCableTest))]
public class ProcCableTestEditor : Editor
{
    public void OnSceneGUI()
    {
        ProcCableTest pct = (ProcCableTest)target;

        if (pct.Path == null || pct.Path.Count < 1)
            return;

        for (int i = 0; i < pct.Path.Count; i++)
        {
            Vector3 pos = pct.Path[i];
            pos = pct.transform.TransformPoint(pos);
            var fmh_19_47_638354911924764774 = Quaternion.identity; pos = Handles.FreeMoveHandle(pos, 0.25f, Vector3.zero, Handles.CubeHandleCap);

            pct.Path[i] = pct.transform.InverseTransformPoint(pos);
        }
    }
}
