using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DustController : MonoBehaviour
{
    public Rigidbody VehicleRigidbody;
    public VisualEffect WheelVisualEffect;
    public WheelCollider WheelCollider;
    public float TopSpeed = 5;

    private Vector3 _priorPos;
    
    void Start()
    {
        _priorPos = VehicleRigidbody.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPos = VehicleRigidbody.transform.position;
        float vel = Vector3.Magnitude(currentPos - _priorPos) / Time.deltaTime;
        _priorPos = currentPos;
        float spawnRate = Mathf.Lerp(0, 10, VehicleRigidbody.velocity.magnitude / TopSpeed);
        float particleVel = Mathf.Lerp(0, -1, VehicleRigidbody.velocity.magnitude / TopSpeed);
        float rate = 0;
        if (WheelCollider != null)
        {
            rate = Mathf.Lerp(0, 1, Mathf.Abs(WheelCollider.rpm) / 750);
        }
        else
        {
            //Debug.Log($"Pickup truck velocity: {vel}");
            rate = Mathf.Lerp(0, 1, vel / (TopSpeed * 0.44704f));
        }
        
        //WheelVisualEffect.SetFloat("Spawn Rate", spawnRate);
        
        //WheelVisualEffect.playRate = rate;
        WheelVisualEffect.SetFloat("Spawn Rate", rate * 10);
        //WheelVisualEffect.SetVector3("Initial Velocity", new Vector3(0, 0.1f, particleVel));
        //WheelVisualEffect.visualEffectAsset.
    }
}
