using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    public float Speed = 1;
    public float MinBrightness = 1;
    public float MaxBrightness = 50;


    private Light _light;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_light == null)
            return;

        var noise = Mathf.PerlinNoise(Time.time * Speed, 0);

        var light = noise * (MaxBrightness - MinBrightness);
        light += MinBrightness;

        _light.intensity = light;
    }
}
