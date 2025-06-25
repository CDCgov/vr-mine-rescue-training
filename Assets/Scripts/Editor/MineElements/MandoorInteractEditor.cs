using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MandoorInteract))]
public class MandoorInteractEditor : Editor {

    private void OnSceneGUI()
    {
        MandoorInteract manDoor = (MandoorInteract)target;

        Vector3 EntryPointOne = manDoor.transform.TransformPoint(manDoor.DoorEntryPointOne);
        Vector3 EntryPointTwo = manDoor.transform.TransformPoint(manDoor.DoorEntryPointTwo);

        var fmh_16_63_638354911924763409 = Quaternion.identity; EntryPointOne = Handles.FreeMoveHandle(EntryPointOne, 0.1f, new Vector3(1, 1, 1), Handles.SphereHandleCap);
        var fmh_17_63_638354911924789005 = Quaternion.identity; EntryPointTwo = Handles.FreeMoveHandle(EntryPointTwo, 0.1f, new Vector3(1, 1, 1), Handles.SphereHandleCap);
        EntryPointOne.y = manDoor.transform.TransformPoint(manDoor.DoorEntryPointOne).y;
        EntryPointTwo.y = manDoor.transform.TransformPoint(manDoor.DoorEntryPointTwo).y;

        manDoor.DoorEntryPointOne = manDoor.transform.InverseTransformPoint(EntryPointOne);
        manDoor.DoorEntryPointTwo = manDoor.transform.InverseTransformPoint(EntryPointTwo);
    }
}
