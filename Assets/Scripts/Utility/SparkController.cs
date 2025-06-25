using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkController : MonoBehaviour
{
    public ParticleSystem SparkSystem;
    public AudioSource AudioSystem;
    public GameObject FlickerLight;
    public float StartDelay = 0;

    private float _triggerTime = 0;
    private bool _lightState = false;
    private bool _enableTrigger = true;
    // Start is called before the first frame update
    void Start()
    {
        StartDelay += Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(!SparkSystem.isPlaying)
        {
            if (Time.time > StartDelay)
            {
                if (Time.time > _triggerTime && _enableTrigger)
                {
                    SparkSystem.Play();
                    AudioSystem.Play();
                    FlickerLight.SetActive(true);
                    _lightState = true;
                    _enableTrigger = false;
                }
                else
                {
                    FlickerLight.SetActive(false);
                    _lightState = false;
                    if (!_enableTrigger)
                    {
                        _triggerTime = Time.time + 1;
                        _enableTrigger = true;
                    }
                }
            }
        }
        else
        {
            _lightState = !_lightState;
            FlickerLight.SetActive(_lightState);
        }
    }
}
