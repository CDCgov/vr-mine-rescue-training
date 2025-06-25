using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestChangeTimescale : MonoBehaviour
{
    [Range(0.1f, 10)]
    public float Timescale = 1;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = Timescale;
    }
}
