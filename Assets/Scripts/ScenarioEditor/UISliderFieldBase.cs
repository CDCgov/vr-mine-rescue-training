using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SliderField))]
public abstract class UISliderFieldBase : MonoBehaviour
{
    protected SliderField _slider;

    protected virtual void Awake()
    {
        _slider = GetComponent<SliderField>();
    }

    protected virtual void Start()
    {
        if (_slider == null)
            return;

        _slider.onSubmitValue.AddListener(OnSubmitValue);
    }

    protected abstract void OnValueChanged(float val);

    protected void SetCurrentValue(float val)
    {
        if (_slider == null)
            return;

        _slider.SetCurrentValue(val);
    }

    private void OnSubmitValue(float val, bool arg1)
    {
        OnValueChanged(val);
    }

    protected virtual void OnDestroy()
    {
        if (_slider != null)
            _slider.onSubmitValue.RemoveListener(OnSubmitValue);
    }
}
