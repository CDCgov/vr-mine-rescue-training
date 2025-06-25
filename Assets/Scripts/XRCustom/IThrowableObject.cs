using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IThrowableObject
{
    public void ThrowObject(Transform thrownBy, Vector3 velocity, Vector3 angularVelocity);
}
