using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockFall : MonoBehaviour {

    public ParticleSystem RockFallSystem;
    // Use this for initialization
    private void OnCollisionEnter(Collision collision)
    {
        if(RockFallSystem != null && RockFallSystem.isStopped)
        {           
            RockFallSystem.Play();
        }
    }
}
