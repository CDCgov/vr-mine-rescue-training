using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(TMP_Text))]
[RequireComponent(typeof(LayoutElement))]
public class TextMeshAutoSize : MonoBehaviour
{
    public float MaxWidth = 300;

    public event Action OnAutoSizeComplete;

    private TMP_Text _text;
    private LayoutElement _layout;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _layout = GetComponent<LayoutElement>();
    }

    void Start()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        OnTextChanged(_text);
    }

    private void OnEnable()
    {

        if (_text != null)
            OnTextChanged(_text);
    }

    void OnTextChanged(UnityEngine.Object obj)
    {
        if (obj != _text)
            return;

        var prefSize = _text.GetPreferredValues();
        if (prefSize.x > MaxWidth)
            _layout.preferredWidth = MaxWidth;
        else
            _layout.preferredWidth = -1;

        if(OnAutoSizeComplete != null)
            OnAutoSizeComplete.Invoke();
    }

    public void ManualSizeAdjust()
    {
        OnTextChanged(_text);
    }
}
