using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyToTarget : MonoBehaviour
{
    public Vector3 Target;
    public float Speed = 10.0f;
    public float Lifetime = 3.0f;
    public float RotationSpeed = 45f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 goalDir = (Target - transform.position).normalized;

        var goalRot = Quaternion.FromToRotation(Vector3.forward, goalDir);
        //Vector3.RotateTowards(forward, goalDir, RotationSpeed * Time.deltaTime, 1.0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, goalRot, RotationSpeed * Time.deltaTime);

        transform.position += transform.forward * (Speed * Time.deltaTime);

        Lifetime -= Time.deltaTime;
        if (Lifetime <= 0)
            Destroy(gameObject);
    }
}
