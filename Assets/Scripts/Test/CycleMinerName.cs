using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleMinerName : MonoBehaviour
{
    private TextTexture[] _textTextures;
    private MinerColorChanger _colorChanger;
    private string[] _names;

    private int _index = 0;

    // Start is called before the first frame update
    void Start()
    {
        _names = new string[]
        {
            "Han",
            "Luke",
            "One",
            "Two",
            "Three",
            "Very Long Name Test",
        };

        _textTextures = GetComponentsInChildren<TextTexture>();
        _colorChanger = GetComponent<MinerColorChanger>();

        InvokeRepeating(nameof(CycleName), 0, 2.0f);
    }

    private void CycleName()
    {
        if (_index >= _names.Length)
            _index = 0;

        string text = _names[_index];
        _index++;

        if (_textTextures != null)
        {
            foreach (var textTexture in _textTextures)
            {
                textTexture.Text = text;
                textTexture.UpdateTexture();
            }
        }

        if (_colorChanger != null)
        {
            Color color = Color.HSVToRGB(Random.value, 1.0f, 1.0f);
            _colorChanger.MinerColor = color;
            _colorChanger.UpdateMiner();
        }

    }
}
