using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UISliderSetContextValue : UIContextControlBase, IPointerUpHandler, IPointerDownHandler
{
    public bool ApplyOnPointerUp = false;

    private Slider _slider;

    private bool _valueChanged = false;

    protected override void Start()
    {
        base.Start();

        _slider = GetComponent<Slider>();

        _slider.onValueChanged.AddListener(OnSliderChanged);


        //SetContextVariable(_slider.value);
        UpdateFromContext();
    }

    public void OnPointerDown(PointerEventData data)
    {
        Debug.Log($"UISliderSetContextValue: OnPointerDown {data}");
    }

    public void OnPointerUp(PointerEventData data)
    {
        Debug.Log($"UISliderSetContextValue: OnPointerUp {data}");

        if (ApplyOnPointerUp && _valueChanged)
        {
            _valueChanged = false;
            SetContextVariable(_slider.value);
        }
    }

    private void OnSliderChanged(float sliderVal)
    {
        if (ApplyOnPointerUp)
        {
            _valueChanged = true;
            return;
        }

        SetContextVariable(sliderVal);
    }

    protected override void OnVariableChanged(object val)
    {
        UpdateFromContext();
    }

    private void UpdateFromContext()
    {
        float floatVal = GetVariableAsFloat();
        if (float.IsNaN(floatVal))
            return;

        UpdateSlider(floatVal);
    }

    private void UpdateSlider(float val)
    {
        _slider.SetValueWithoutNotify(val);
    }

    
}
