using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkLight : MonoBehaviour
{
    public Light[] LightsToToggle;
    
    public float StartTime = 0;

    private float _triggerTime;
    private float _onDelay = 0.1f;
    private float _offDelay = 1.5f;
    private bool _isOn = true;
    // Start is called before the first frame update
    void Start()
    {
        _triggerTime = Time.time + _onDelay;
        _triggerTime = Time.time + _onDelay + StartTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > _triggerTime)
        {
            if (_isOn)
            {
                foreach(Light lit in LightsToToggle)
                {
                    lit.enabled = false;
                }
                _isOn = false;
                _triggerTime = Time.time + _offDelay;
            }
            else
            {
                foreach (Light lit in LightsToToggle)
                {
                    lit.enabled = true;
                }
                _isOn = true;
                _triggerTime = Time.time + _onDelay;
            }
        }        
    }
}
