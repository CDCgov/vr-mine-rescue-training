using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityStopwatch
{

    private float _startTime;
    private float _offsetTime;
    private bool _running;

    public float ElapsedSeconds
    {
        get
        {
            if (_running)
                return (Time.realtimeSinceStartup - _startTime) + _offsetTime;
            else
                return _offsetTime;
        }
    }
    
    public UnityStopwatch()
    {
        _startTime = 0;
        _offsetTime = 0;
        _running = false;
    }

    public void Start()
    {
        if (_running)
            return;

        _startTime = Time.realtimeSinceStartup;
        _running = true;
    }

    public void Stop()
    {
        _running = false;
        var elapsed = Time.realtimeSinceStartup - _startTime;
        _offsetTime += elapsed;
    }

    public void Reset()
    {
        _startTime = Time.realtimeSinceStartup;
        _offsetTime = 0;
    }

    public void SetState(float elapsedTime, bool running)
    {
        _startTime = Time.realtimeSinceStartup;
        _running = running;
        _offsetTime = elapsedTime;
    }

}
