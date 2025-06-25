using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraLogic : MonoBehaviour
{
    public abstract void Activate();
    public abstract void Deactivate();

    public abstract void FocusObject(GameObject go);
}
