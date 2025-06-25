using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainOrientation : MonoBehaviour
{
    private Quaternion _orientation;

    void Awake()
    {
        _orientation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = _orientation;
    }
}
