using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackingZone))]
public class TrackingZoneEditor : Editor {

    float size = 0.1f;
    private void OnSceneGUI()
    {
        TrackingZone commZone = (TrackingZone)target;
        Transform transform = commZone.transform;

        Handles.color = Handles.xAxisColor;
        EditorGUI.BeginChangeCheck();
        var fmh_17_98_638354911924757708 = Quaternion.identity; Vector3 newPosition = Handles.FreeMoveHandle(transform.position + commZone.xPosPosition, size, new Vector3(1, 1, 1), Handles.RectangleHandleCap);
        var fmh_18_97_638354911924789330 = Quaternion.identity; Vector3 newNegXPos = Handles.FreeMoveHandle(transform.position + commZone.xNegPosition, size, new Vector3(1, 1, 1), Handles.RectangleHandleCap);
        Handles.color = Handles.zAxisColor;
        var fmh_20_97_638354911924791954 = Quaternion.identity; Vector3 newPosZPos = Handles.FreeMoveHandle(transform.position + commZone.zPosPosition, size, new Vector3(1, 1, 1), Handles.RectangleHandleCap);
        var fmh_21_97_638354911924794900 = Quaternion.identity; Vector3 newNegZPos = Handles.FreeMoveHandle(transform.position + commZone.zNegPosition, size, new Vector3(1, 1, 1), Handles.RectangleHandleCap);
        if (newPosition.x < transform.position.x)
        {
            newPosition.x = transform.position.x + 0.1f;
        }
        newPosition.y = transform.position.y;
        newPosition.z = transform.position.z;
        if (newNegXPos.x > transform.position.x)
        {
            newNegXPos.x = transform.position.x - 0.1f;
        }
        newNegXPos.y = transform.position.y;
        newNegXPos.z = transform.position.z;
        newPosZPos.x = transform.position.x;
        newPosZPos.y = transform.position.y;
        if (newPosZPos.z < transform.position.z)
        {
            newPosZPos.z = transform.position.z + 0.1f;
        }
        newNegZPos.x = transform.position.x;
        newNegZPos.y = transform.position.y;
        if (newNegZPos.z > transform.position.z)
        {
            newNegZPos.z = transform.position.z - 0.1f;
        }
        if (EditorGUI.EndChangeCheck())
        {
            UnityEditor.Undo.RecordObject(commZone, "Change handle positions");
            //Debug.Log(newPosition);
            commZone.xPosPosition = newPosition - transform.position;
            commZone.xNegPosition = newNegXPos - transform.position;
            commZone.zPosPosition = newPosZPos - transform.position;
            commZone.zNegPosition = newNegZPos - transform.position;
        }
    }
}
