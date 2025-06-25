using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonToggleGrid : UIButtonBase
{
    public SystemManager SystemManager;

    public Color GridOnColor;
    public Color GridOffColor;
    public GameObject GridObject;

    protected override void Start()
    {
        base.Start();

        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        GridObject.SetActive(SystemManager.SystemConfig.GridEnabled);
        UpdateColor();
    }

    private void UpdateColor()
    {
        Color newColor;

        if (GridObject.activeInHierarchy)
        {
            newColor = GridOnColor;
        }
        else
        {
            newColor = GridOffColor;
        }

        var colors = _button.colors;
        colors.normalColor = newColor;
        colors.pressedColor = newColor;
        colors.highlightedColor = newColor;
        colors.selectedColor = newColor;

        _button.colors = colors;

    }

    protected override void OnButtonClicked()
    {
        var state = GridObject.activeInHierarchy;

        GridObject.SetActive(!state);
        UpdateColor();

        SystemManager.SystemConfig.GridEnabled = !state;
    }
}
