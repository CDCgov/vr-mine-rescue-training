using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StringField : MonoBehaviour
{
    public TMP_InputField InputField;
    public TMP_Text LabelField;

    public event Action<string> ValueChanged;

    public void SetLabelText(string label)
    {
        if (LabelField == null)
            return;

        LabelField.text = label;
    }

    public void SetValueWithoutNotify(string val)
    {
        if (InputField == null)
            return;

        InputField.SetTextWithoutNotify(val);
    }

    private void Start()
    {
        if (InputField == null)
        {
            Debug.LogError($"Missing InputField on {name}");
            return;
        }

        //InputField.onSubmit.AddListener(OnValueChanged);
        //InputField.onEndEdit.AddListener(OnValueChanged);
        InputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(string val)
    {
        ValueChanged?.Invoke(val);
    }
}
