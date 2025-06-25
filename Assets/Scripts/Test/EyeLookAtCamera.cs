using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeLookAtCamera : MonoBehaviour
{
    Transform _mainCameraTransform;

    Vector3 _startDir;
    // Start is called before the first frame update
    void Start()
    {
        _mainCameraTransform = Camera.main.transform;
        _startDir = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Angle(_startDir, _mainCameraTransform.position - transform.position) <= 30)
        {
            transform.LookAt(_mainCameraTransform);
        }
    }
}
