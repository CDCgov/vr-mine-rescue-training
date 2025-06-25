using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInputDMUIScale : MonoBehaviour
{
    public SystemManager SystemManager;

    private TMP_InputField _input;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (!TryGetComponent<TMP_InputField>(out _input))
        {
            Debug.LogError("UIInputDMUIScale: No input field found");
            return;
        }

        _input.onEndEdit.AddListener(OnEndEdit);

    }

    private void OnEndEdit(string val)
    {
        if (float.TryParse(val, out float uiScale))
        {
            uiScale = Mathf.Clamp(uiScale, 0.5f, 3.0f);

            _input.SetTextWithoutNotify(uiScale.ToString("F2"));

            SystemManager.SystemConfig.UIScale = uiScale;
            SystemManager.SystemConfig.SaveConfig();
        }
    }
}
