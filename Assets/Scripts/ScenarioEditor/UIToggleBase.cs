using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public abstract class UIToggleBase : MonoBehaviour
{

    protected Toggle _toggle;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    protected virtual void OnDestroy()
    {
        if (_toggle != null)
            _toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    protected abstract void OnToggleChanged(bool value);
}
