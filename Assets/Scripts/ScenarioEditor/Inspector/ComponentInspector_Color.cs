using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComponentInspector_Color : ComponentInspector<ComponentInfo_Color>
{
    public TMP_Text headerText;
    [SerializeField] Image colorImageDisplay;
    [SerializeField] Image colorImageDisplay2;
    [SerializeField] SliderField redSliderField;
    [SerializeField] SliderField greenSliderField;
    [SerializeField] SliderField blueSliderField;
    [SerializeField] SliderField alphaSliderField;
    [SerializeField] GameObject AdvancedColorPanel;


    private Color _savedColor;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        InitializeValues(TargetComponentInfo);
        redSliderField.onSubmitValue.AddListener(SetRed);
        greenSliderField.onSubmitValue.AddListener(SetGreen);
        blueSliderField.onSubmitValue.AddListener(SetBlue);
        alphaSliderField.onSubmitValue.AddListener(SetAlpha);

    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        redSliderField.onSubmitValue.RemoveListener(SetRed);
        greenSliderField.onSubmitValue.RemoveListener(SetGreen);
        blueSliderField.onSubmitValue.RemoveListener(SetBlue);
        alphaSliderField.onSubmitValue.RemoveListener(SetAlpha);
    }

    public void SetRed(float value, bool enabled)
    {
        Color color = TargetComponentInfo.TargetColor;
        color.r = value / 255.0f;

        SetColor(color, true);
        
    }
    public void SetGreen(float value, bool enabled)
    {
        Color color = TargetComponentInfo.TargetColor;
        color.g = value / 255.0f;

        SetColor(color, true);
        
    }
    public void SetBlue(float value, bool enabled)
    {
        Color color = TargetComponentInfo.TargetColor;
        color.b = value / 255.0f;

        SetColor(color, true);
    }

    public void SetAlpha(float value, bool enabled)
    {
        Color color = TargetComponentInfo.TargetColor;
        color.a = value;
        SetColor(color, true);
    }


    public void SetColor(Color color, bool enabled)
    {
        if (TargetComponentInfo == null)
            return;

        TargetComponentInfo.TargetColor = color;
        colorImageDisplay.color = color;
        colorImageDisplay2.color = color;
        _savedColor = color;

        
        redSliderField.SetSliderValues(0, 255, color.r * 255.0f);
        greenSliderField.SetSliderValues(0, 255, color.g * 255.0f);
        blueSliderField.SetSliderValues(0, 255, color.b * 255.0f);


        //TargetComponentInfo.RendererToColor.materials[TargetComponentInfo.MaterialIndexToColor].color = color;
        TargetComponentInfo.TargetColor = color;
        TargetComponentInfo.UpdateColor();
    }

    private void InitializeValues(ComponentInfo_Color colorComponent)
    {
        if (colorComponent == null)
            return;

        //var hdrpLight = light.m_Lights[0].GetComponent<HDAdditionalLightData>();

        AdvancedColorPanel.SetActive(true);

        //Debug.Log($"Light color? {targetLightComponent.color.ToString()}");
        redSliderField.startValue = colorComponent.TargetColor.r * 255;
        redSliderField.ResetValues();

        greenSliderField.startValue = colorComponent.TargetColor.g * 255;
        greenSliderField.ResetValues();

        blueSliderField.startValue = colorComponent.TargetColor.b * 255;
        blueSliderField.ResetValues();

        alphaSliderField.startValue = colorComponent.TargetColor.a;
        alphaSliderField.ResetValues();

        colorImageDisplay.color = colorComponent.TargetColor;
        colorImageDisplay2.color = colorComponent.TargetColor;
        _savedColor = colorComponent.TargetColor;

        AdvancedColorPanel.SetActive(false);

        headerText.text = colorComponent.componentName;

        colorComponent.UpdateColor();

    }
}
