using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class TestUnityStopwatch : MonoBehaviour
{
    private Stopwatch _stopwatch;
    private UnityStopwatch _unityStopwatch;

    public void StartTimer()
    {
        _stopwatch.Start();
        _unityStopwatch.Start();
    }

    public void StopTimer()
    {
        _stopwatch.Stop();
        _unityStopwatch.Stop();
    }

    public void ResetTimer()
    {
        _stopwatch.Reset();
        _unityStopwatch.Reset();
    }

    public string GetStatusText()
    {
        float unity = _unityStopwatch.ElapsedSeconds;
        float system = _stopwatch.ElapsedMilliseconds;
        system = system / 1000.0f;

        return string.Format("Unity: {0:F2}\nSystem: {1:F2}", unity, system);
    }

    // Start is called before the first frame update
    void Start()
    {
        _stopwatch = new Stopwatch();
        _unityStopwatch = new UnityStopwatch();
    }

}
