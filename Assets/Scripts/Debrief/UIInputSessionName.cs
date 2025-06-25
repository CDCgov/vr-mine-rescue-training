using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(TMP_InputField))]
public class UIInputSessionName : MonoBehaviour
{
    public SystemManager SystemManager;

    private TMP_InputField _inputField;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _inputField = GetComponent<TMP_InputField>();

        _inputField.text = SystemManager.SystemConfig.SessionName;

        _inputField.onValueChanged.AddListener(OnInputFieldChanged);
        _inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
        //_inputField.onDeselect.AddListener(OnInputFieldEndEdit);
        //_inputField.onSubmit.AddListener(OnInputFieldEndEdit);

    }

    private void OnInputFieldEndEdit(string arg0)
    {
        Debug.Log("UIInputSessionName::EndEdit");
        SystemManager.SystemConfig.SaveConfig();
    }

    private void OnInputFieldChanged(string val)
    {
        SystemManager.SystemConfig.SessionName = val;
        //SystemManager.SystemConfig.SaveConfig();
    }
}
