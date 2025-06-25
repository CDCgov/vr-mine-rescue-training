using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFightAnimate : MonoBehaviour
{
    public ParticleSystem[] SystemsToFlare;
    public float Duration = 3;

    private List<float> targetEmissions;
    private float time = 0;
    private bool _startFlare = false;
    private bool _tagetsCached = false;


    // Update is called once per frame
    void Update()
    {
        if (_startFlare)
        {
            if (time > Duration)
            {
                _startFlare = false;
                return;
            }
            for (int i = 0; i < SystemsToFlare.Length; i++)
            {
                var em = SystemsToFlare[i].emission;
                float rate = Mathf.Lerp(targetEmissions[i], 0, time / Duration);

                em.rateOverTimeMultiplier = rate;
            }
            time += Time.deltaTime;
            Debug.Log($"Time: " + time);
        }
    }

    private void OnEnable()
    {
        if (!_tagetsCached)
        {
            targetEmissions = new List<float>();
            for (int i = 0; i < SystemsToFlare.Length; i++)
            {
                targetEmissions.Add(SystemsToFlare[i].emission.rateOverTimeMultiplier);
                Debug.Log($"Target Emission {i}: {SystemsToFlare[i].emission.rateOverTimeMultiplier}");
            }
            _tagetsCached = true;
        }
        _startFlare = true;
    }
}
