using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TMPro.TMP_InputField))]
public class UIInputSetContextValue : UIContextControlBase
{

    private TMP_InputField _inputField;

    protected override void Start()
    {
        base.Start();
        _inputField = GetComponent<TMP_InputField>();

        _inputField.onValueChanged.AddListener(OnInputValueChanged);

        UpdateFromContext();
    }

    private void OnEnable()
    {
        if (_inputField == null || _context == null)
            return;

        UpdateFromContext();
    }

    private void OnInputValueChanged(string val)
    {
        SetContextVariable(val);
    }

    protected override void OnVariableChanged(object val)
    {
        UpdateFromContext();
    }

    private void UpdateFromContext()
    {
        var str = GetVariableAsString();
        if (str != null && str.Length > 0)
            _inputField.SetTextWithoutNotify(str);
    }
}
