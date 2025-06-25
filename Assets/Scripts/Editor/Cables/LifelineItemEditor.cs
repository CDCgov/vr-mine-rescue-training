using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LifelineItem))]
public class LifelineItemEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Flip Direction"))
        {
            LifelineItem marker = (LifelineItem)target;
            Transform geo = marker.transform.Find("Geometry");
            if(geo != null)
            {
                geo.Rotate(new Vector3(0,180,0), Space.Self);
            }
        }
    }
    protected virtual void OnSceneGUI()
    {        
        LifelineItem marker = (LifelineItem)target;

        if (marker == null || marker.LifeLineGenRef == null)
            return;

        Transform markerTransform = marker.transform;

        Handles.color = Handles.zAxisColor;
        Vector3 oldPosition = markerTransform.position;
        Vector3[] points = new Vector3[3];

        if (marker.LifeLineGenRef.ReferencePoints == null || marker.ClosestPointIndex < 1 || marker.ClosestPointIndex >= marker.LifeLineGenRef.ReferencePoints.Count - 1)
            return;

        if (marker.LifeLineGenRef.ReferencePoints == null)
        {
            marker.LifeLineGenRef.CableGeometryRef.RegenerateMesh();
        }

        if (marker.LifeLineGenRef.ReferencePoints != null)
        {
            points[0] = marker.transform.TransformPoint(marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex - 1]);
            points[1] = marker.transform.TransformPoint(marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex]);
            points[2] = marker.transform.TransformPoint(marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex + 1]);

        }
        Vector3 direct = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex + 1] - marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex];
        if (Handles.Button(oldPosition + (markerTransform.forward * 0.5f), Quaternion.LookRotation(direct), 0.1f, 0.2f, Handles.ConeHandleCap))
        {            
            marker.ClosestPointIndex++;
            markerTransform.localPosition = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex];
            Vector3 dir = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex + 1] - marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex];
            markerTransform.rotation = Quaternion.LookRotation(dir);
        }

        if (Handles.Button(oldPosition + (markerTransform.forward * -0.5f), Quaternion.LookRotation(direct * -1), 0.1f, 0.2f, Handles.ConeHandleCap))
        {
            
            marker.ClosestPointIndex--;
            markerTransform.localPosition = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex];
            Vector3 dir = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex] - marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex - 1];
            markerTransform.rotation = Quaternion.LookRotation(dir);
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(marker);
        }
    }
}
