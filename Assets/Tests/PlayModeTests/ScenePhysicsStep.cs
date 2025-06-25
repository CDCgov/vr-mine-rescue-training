using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePhysicsStep : MonoBehaviour
{
    private PhysicsScene _physics;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        _physics = gameObject.scene.GetPhysicsScene();
    }

    void FixedUpdate()
    {
        _physics.Simulate(Time.fixedDeltaTime);
    }
}
