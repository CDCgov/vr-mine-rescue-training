using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UITogglePacketStatLog : MonoBehaviour
{
    public SystemManager SystemManager;
    private Toggle _toggle;

    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _toggle = GetComponent<Toggle>();
        _toggle.SetIsOnWithoutNotify(SystemManager.SystemConfig.MPPacketStatLogEnabled);
        _toggle.onValueChanged.AddListener(OnValueChanged);

    }

    private void OnValueChanged(bool val)
    {
        SystemManager.SystemConfig.MPPacketStatLogEnabled = val;
        SystemManager.SystemConfig.SaveConfig();
    }
}
