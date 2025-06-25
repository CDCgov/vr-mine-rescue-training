using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorField : MonoBehaviour
{
    public TMP_Text LabelText;
    public Image ColorDisplay;
    public GameObject ColorEditorParent;
    public Button AdjustButton;
    public SliderField RedSlider;
    public SliderField GreenSlider;
    public SliderField BlueSlider;

    public event Action<Color> ColorChanged;

    private Color _color;

    public void SetColorWithoutNotify(Color color)
    {
        _color = color;
        ColorDisplay.color = color;
        RedSlider.SetCurrentValue(color.r * 255.0f);
        GreenSlider.SetCurrentValue(color.g * 255.0f);
        BlueSlider.SetCurrentValue(color.b * 255.0f);
    }

    public void SetColor(Color color)
    {
        SetColorWithoutNotify(color);
        ColorChanged?.Invoke(color);
    }

    public void SetLabelText(string label)
    {
        if (LabelText != null)
            LabelText.text = label;
    }

    void Start()
    {
        if (ColorDisplay == null || AdjustButton == null || ColorEditorParent == null ||
            RedSlider == null || GreenSlider == null || BlueSlider == null)
        {
            Debug.LogError($"ColorField missing required controls on {name}");
            enabled = false;
            gameObject.SetActive(false);
            return;
        }

        AdjustButton.onClick.AddListener(OnAdjustClicked);

        RedSlider.onSubmitValue.AddListener(OnSliderChanged);
        GreenSlider.onSubmitValue.AddListener(OnSliderChanged);
        BlueSlider.onSubmitValue.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float arg0, bool arg1)
    {
        var r = RedSlider.GetCurrentValue() / 255.0f;
        var g = GreenSlider.GetCurrentValue() / 255.0f;
        var b = BlueSlider.GetCurrentValue() / 255.0f;

        SetColor(new Color(r, g, b));
    }

    private void OnAdjustClicked()
    {
        ColorEditorParent.SetActive(!ColorEditorParent.activeSelf);
        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        var parent = transform.parent as RectTransform;
        if (parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent); 
            LayoutRebuilder.MarkLayoutForRebuild(parent);
        }
    }
}
