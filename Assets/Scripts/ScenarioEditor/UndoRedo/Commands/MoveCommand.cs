using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand
{
    Vector3 startPosition, endPosition;
    Quaternion startRotation, endRotation;
    Vector3 startScale, endScale;
    Transform movedObject;

    public MoveCommand(Vector3 startPosition, Vector3 endPosition, Quaternion startRotation, Quaternion endRotation, 
                       Vector3 startScale, Vector3 endScale, Transform movedObject)
    {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        this.startRotation = startRotation;
        this.endRotation = endRotation;
        this.startScale = startScale;
        this.endScale = endScale;
        this.movedObject = movedObject;
    }

    public void Execute()
    {
        Debug.Log("EXECUTING END");
        movedObject.localPosition = endPosition;
        movedObject.localRotation = endRotation;
        movedObject.localScale = endScale;
    }

    public void UnExecute()
    {
        movedObject.localPosition = startPosition;
        movedObject.localRotation = startRotation;
        movedObject.localScale = startScale;
    }
}
