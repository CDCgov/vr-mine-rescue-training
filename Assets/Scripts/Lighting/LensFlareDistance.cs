using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LensFlare))]
public class LensFlareDistance : MonoBehaviour
{
    public float IntensityMultiplier = 3f;
    public float FalloffStartDistance = 10f;
    private LensFlare _lFlare;
    private float _initialBrightness;
    // Start is called before the first frame update
    void Start()
    {        
        _lFlare = GetComponent<LensFlare>();
        _initialBrightness = _lFlare.brightness;
    }

    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;
        float dist = 0;

        float intensityMultiplier = IntensityMultiplier;

        if (cam != null)
        {
            dist = Vector3.Distance(transform.position, cam.transform.position);

            

            //ControlledObject.transform.localScale = new Vector3(scale,scale,scale);

            float intensityReduction = FalloffStartDistance / dist;
            intensityReduction = Mathf.Clamp(intensityReduction, 0.0f, 1.0f);

            intensityMultiplier *= intensityReduction;
            _lFlare.brightness = Mathf.Clamp(_initialBrightness * intensityMultiplier, 0, _initialBrightness);
        }
    }
}
