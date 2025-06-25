using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMoveTest : MonoBehaviour
{
    public Vector3 Offset;

    private float _multiplier = 1.0f;

    void Update()
    {
        transform.position += Offset * _multiplier;
        _multiplier *= -1.0f;
    }
}
