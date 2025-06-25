using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UISliderLightIntensity : MonoBehaviour
{
    public Light TargetLight;

    private Slider _slider;

    // Start is called before the first frame update
    void Start()
    {
        _slider = GetComponent<Slider>();

        _slider.SetValueWithoutNotify(TargetLight.intensity);
        _slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float val)
    {
        TargetLight.intensity = val;
    }

}
