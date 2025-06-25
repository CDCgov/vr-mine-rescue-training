using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeLights : MonoBehaviour
{
    public List<Light> LightsToFade;
    public float TargetIntensityNormalized = 0.05f;
    public float FadeDuration = 3;

    private List<float> _startingIntensities;
    private bool _startFade = false;
    private float _targetTime = Mathf.Infinity;
    private float _elapsed = 0;
    // Start is called before the first frame update
    void Start()
    {
        _startingIntensities = new List<float>();
        for (int i = 0; i < LightsToFade.Count; i++)
        {
            _startingIntensities.Add(LightsToFade[i].intensity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_startFade && _elapsed <= 1)
        {
            _elapsed += (Time.deltaTime / FadeDuration);
            for (int i = 0; i < LightsToFade.Count; i++)
            {
                
                LightsToFade[i].intensity = Mathf.Lerp(_startingIntensities[i], _startingIntensities[i] * TargetIntensityNormalized, _elapsed);
                
            }
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < LightsToFade.Count; i++)
        {
            LightsToFade[i].intensity = _startingIntensities[i];
        }
        _startFade = true;
        _targetTime = Time.time + FadeDuration;
    }
}
