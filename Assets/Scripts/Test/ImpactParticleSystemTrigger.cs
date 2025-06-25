using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactParticleSystemTrigger : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public LayerMask Layers;
    public float SphereDiameter = 1.0f;

    private ParticleSystem _ps;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled)
            return;

        if (other.gameObject.layer != 0)
            return;

        if (ParticleSystem != null)
            ParticleSystem.Play();

        Debug.LogFormat("Impact with {0}", other.name);
    }
}
