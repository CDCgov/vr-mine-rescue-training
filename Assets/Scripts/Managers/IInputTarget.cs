using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IInputTarget
{
    void SetMovementVector(Vector3 moveVector);
    Vector3 GetMovementVector();

    /// <summary>
    /// Get the look dir euler angles (pitch, yaw, roll)
    /// </summary>	
    void SetLookEuler(Vector3 eulerAngles);

    /// <summary>
    /// Get the look dir euler angles (pitch, yaw, roll)
    /// </summary>
    Vector3 GetLookEuler();

    InputTargetOptions GetInputTargetOptions();

    void ProcessCustomInput();
}