using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Timer : MonoBehaviour 
{
    //public static double Time;
    private static float _FloatStartTime;
    private static Stopwatch _stopwatch;
    public static void Init () 
    {
        _stopwatch = new Stopwatch();
    }
    
    public static void StartTimer()
    {
        _stopwatch.Start(); //TODO: IMPORTANT: Establish when timers should be started
        _FloatStartTime = Time.time;
    }

    public static void StopTimer()
    {
        _stopwatch.Stop();
    }

    public static void ResetTimer()
    {
        _stopwatch.Reset();
        _FloatStartTime = Time.time;//may have to change to Time.time
    }

    public static double GetTime()
    {
        if (_stopwatch != null)
        {
            //return (double)_stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
            return Time.time - _FloatStartTime;
        }
        else
        {
            return (double)Time.time; //If the timer was never started, defaults to Unity's time class for time since application started
        }
    }
}