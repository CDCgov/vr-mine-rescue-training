using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TMP_InputField))]
public abstract class UIInspectorInputFieldBase : MonoBehaviour
{
    protected TMP_InputField _inputField;

    public virtual void Start()
    {
        if (!TryGetComponent<TMP_InputField>(out _inputField))
            return;

        _inputField.onEndEdit.AddListener(OnEndEdit);

        SetValue(GetComponentValue());
    }

    public virtual void OnDestroy()
    {
        if (_inputField != null)
            _inputField.onEndEdit.RemoveListener(OnEndEdit);
    }

    protected abstract bool UpdateComponentValue(string value);
    protected abstract string GetComponentValue();

    protected void SetValue(string value)
    {
        _inputField.SetTextWithoutNotify(value);
    }

    private void OnEndEdit(string value)
    {
        if (!UpdateComponentValue(value))
        {
            //revert to last value
            SetValue(GetComponentValue());
        }
    }
}
