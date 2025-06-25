using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class UIButtonBase : MonoBehaviour
{
    protected Button _button;
    protected UIContextData _context;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);

        _context = transform.GetComponentInParent<UIContextData>();
    }

    protected virtual void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnButtonClicked);
    }

    protected abstract void OnButtonClicked();
}
