using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnColorCalReady : SelectedPlayerControl
{
    public PlayerManager PlayerManager;

    public Color ColorTintReady = Color.green;
    public Color ColorTintNotReady = Color.red;

    private Button _button;

    private ColorBlock _colorsReady;
    private ColorBlock _colorsNotReady;

    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        _button = GetComponent<Button>();

        _colorsReady = TintColorBlock(_button.colors, ColorTintReady);
        _colorsNotReady = TintColorBlock(_button.colors, ColorTintNotReady);

        
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateCalReady());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateCalReady()
    {        
        Debug.Log("Cal Ready Check Coroutine Starting");
        while (true)
        {
            if (PlayerManager == null || _button == null)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                continue;
            }

            if (_player == null)
                _player = PlayerManager.CurrentPlayer;

            if (_player == null)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                continue;
            }

            var calReady = _player.CheckCalibrationReady();
            //Debug.Log($"Cal Ready Check: {calReady}");

            if (calReady)
                _button.colors = _colorsReady;
            else
                _button.colors = _colorsNotReady;

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    private ColorBlock TintColorBlock(ColorBlock original, Color tint)
    {
        ColorBlock colors = new ColorBlock();

        colors.normalColor = TintColor(original.normalColor, tint);
        colors.highlightedColor = TintColor(original.highlightedColor, tint);
        colors.pressedColor = TintColor(original.pressedColor, tint);
        colors.selectedColor = TintColor(original.selectedColor, tint);
        colors.disabledColor = TintColor(original.disabledColor, tint);
        colors.colorMultiplier = 1.0f;

        return colors;
    }

    private Color TintColor(Color c, Color tint)
    {
        return new Color(c.r * tint.r, c.g * tint.g, c.b * tint.b);
    }

}
