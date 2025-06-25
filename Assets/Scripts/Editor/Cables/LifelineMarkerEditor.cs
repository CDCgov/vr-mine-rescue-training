using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LifelineMarker))]
public class LifelineMarkerEditor : Editor {

    protected virtual void OnSceneGUI()
    {
        return;

        LifelineMarker marker = (LifelineMarker)target;
        Transform markerTransform = marker.transform;
        
        Handles.color = Handles.zAxisColor;
        Vector3 oldPosition = markerTransform.position;
        //Vector3 newPosition = Handles.FreeMoveHandle((oldPosition), Quaternion.identity, 0.1f, new Vector3(1, 1, 1), Handles.RectangleHandleCap);
        Vector3[] points = new Vector3[3];
        if (marker.LifeLineGenRef.ReferencePoints != null)
        {
            points[0] = marker.transform.TransformPoint(marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex - 1]);
            points[1] = marker.transform.TransformPoint(marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex]);
            points[2] = marker.transform.TransformPoint(marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex + 1]);

            //if(markerTransform.InverseTransformPoint(newPosition).z > marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex].z)
            //{
            //    newPosition = HandleUtility.ProjectPointLine(newPosition, points[1], points[2]);
            //    Debug.Log(newPosition + ": nPos, " + points[1] + ": p1, " + points[2] + ": p2");
            //}
        }
        Vector3 direct = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex + 1] - marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex];
        if (Handles.Button(oldPosition + (markerTransform.forward * 0.5f), Quaternion.LookRotation(direct), 0.1f, 0.2f, Handles.ConeHandleCap))
        {
            //Undo.RecordObject(marker, "Change handle positions");
            //if (marker.LifeLineGenRef.ReferencePoints != null)
            //{
            //    markerTransform.position = newPosition;
            //    int closestIndex = 0;
            //    float minDistance = Vector3.Distance(markerTransform.position, points[0]);
            //    Debug.Log(minDistance);
            //    if (Vector3.Distance(markerTransform.position, points[2]) < minDistance)
            //    {
            //        closestIndex = 2;
            //        marker.ClosestPointIndex = marker.ClosestPointIndex + 1;
            //    }
            //    else
            //    {
            //        marker.ClosestPointIndex = marker.ClosestPointIndex - 1;
            //    }
            //    Debug.Log(points[closestIndex]);
            //    markerTransform.position = points[closestIndex];
            //} 
            marker.ClosestPointIndex++;
            markerTransform.localPosition = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex];
            Vector3 dir = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex+1] - marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex];
            markerTransform.rotation = Quaternion.LookRotation(dir);
        }

        if (Handles.Button(oldPosition + (markerTransform.forward * -0.5f), Quaternion.LookRotation(direct * -1), 0.1f, 0.2f, Handles.ConeHandleCap))
        {
            //Undo.RecordObject(marker, "Change handle positions");
            //if (marker.LifeLineGenRef.ReferencePoints != null)
            //{
            //    markerTransform.position = newPosition;
            //    int closestIndex = 0;
            //    float minDistance = Vector3.Distance(markerTransform.position, points[0]);
            //    Debug.Log(minDistance);
            //    if (Vector3.Distance(markerTransform.position, points[2]) < minDistance)
            //    {
            //        closestIndex = 2;
            //        marker.ClosestPointIndex = marker.ClosestPointIndex + 1;
            //    }
            //    else
            //    {
            //        marker.ClosestPointIndex = marker.ClosestPointIndex - 1;
            //    }
            //    Debug.Log(points[closestIndex]);
            //    markerTransform.position = points[closestIndex];
            //}            
            marker.ClosestPointIndex--;
            markerTransform.localPosition = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex];
            Vector3 dir = marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex] - marker.LifeLineGenRef.ReferencePoints[marker.ClosestPointIndex-1];
            markerTransform.rotation = Quaternion.LookRotation(dir);
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(marker);
        }
    }
}
