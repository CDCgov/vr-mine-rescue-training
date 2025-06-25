using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISceneCamera
{
    public Vector3 CameraPosition { get; }
    public void FocusObject(Transform transform, float distance = -1);
    public void FocusTarget(Vector3 pos, float distance = -1);
    public void PositionCamera(Vector3 pos, Quaternion rot);
    

}
