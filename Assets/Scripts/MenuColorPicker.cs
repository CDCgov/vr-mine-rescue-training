using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuColorPicker : MonoBehaviour
{
    [SerializeField] SliderField redSliderField;
    [SerializeField] SliderField greenSliderField;
    [SerializeField] SliderField blueSliderField;
    [SerializeField] Image colorImageDisplay;
    [SerializeField] Image colorImageDisplay2;
    Color _savedColor;
    public Action<Color> onColorSaved;

    private void Start()
    {
        redSliderField.onSubmitValue.AddListener(SetRed);
        greenSliderField.onSubmitValue.AddListener(SetGreen);
        blueSliderField.onSubmitValue.AddListener(SetBlue);
    }

    public void OnDestroy()
    {
        redSliderField.onSubmitValue.RemoveListener(SetRed);
        greenSliderField.onSubmitValue.RemoveListener(SetGreen);
        blueSliderField.onSubmitValue.RemoveListener(SetBlue);
    }

    public void SetLightColorPreset(float r, float g, float b)
    {
        redSliderField.ForceValue((int)(r * 255));
        greenSliderField.ForceValue((int)(g * 255));
        blueSliderField.ForceValue((int)(b * 255));
        colorImageDisplay.color = new Color(r, g, b);
        _savedColor = colorImageDisplay.color;
        colorImageDisplay2.color = _savedColor;
    }

    public void SetBlue(float value, bool enabled)
    {
        Color color = _savedColor;
        color.b = value / 255.0f;

        SetColor(color, enabled);

    }

    public void SetRed(float value, bool enabled)
    {
        Color color = _savedColor;
        color.r = value / 255.0f;

        SetColor(color, enabled);
    }
    public void SetGreen(float value, bool enabled)
    {
        Color color = _savedColor;
        color.g = value / 255.0f;

        SetColor(color, enabled);
    }

    public void SetColor(Color color, bool enabled)
    {
        colorImageDisplay.color = color;
        colorImageDisplay2.color = color;
        _savedColor = color;
        onColorSaved?.Invoke(_savedColor);
    }
}
