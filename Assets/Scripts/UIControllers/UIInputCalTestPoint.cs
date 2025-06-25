using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class UIInputCalTestPoint : MonoBehaviour
{
    public SystemManager SystemManager;

    public ColorBlock NormalColor;
    public ColorBlock ErrorColor;

    private TMP_InputField _input;
    private Regex _parseRegex;
    private bool _valueChanged = false;
    private Vector3 _newValue = Vector3.zero;

    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _parseRegex = new Regex(@"([\d.]+)\s*,\s*([\d.]+)\s*,\s*([\d.]+)");

        SystemManager.SystemConfig.CalibrationTestPointChanged += OnCalTestPointChanged;

        _input = GetComponent<TMP_InputField>();
        _input.onValueChanged.AddListener(OnInputChanged);
        _input.onEndEdit.AddListener(OnInputEndEdit);

        UpdateText();
    }

    private void OnInputEndEdit(string arg0)
    {
        if (!_valueChanged)
            return;

        SystemManager.SystemConfig.CalibrationTestPointVec3 = _newValue;
        _valueChanged = false;

        Debug.Log($"UIInputCalTestPoint: Saving config, new test pt {SystemManager.SystemConfig.CalibrationTestPoint}");
        SystemManager.SystemConfig.SaveConfig();
    }

    private void OnDestroy()
    {
        if (_input != null)
            _input.onValueChanged.RemoveListener(OnInputChanged);

        if (SystemManager != null && SystemManager.SystemConfig != null)
            SystemManager.SystemConfig.CalibrationTestPointChanged -= OnCalTestPointChanged;
    }

    private void OnCalTestPointChanged()
    {
        UpdateText();
    }

    void UpdateText()
    {
        if (SystemManager == null || SystemManager.SystemConfig == null)
            return;

        var pt = SystemManager.SystemConfig.CalibrationTestPoint;

        var text = $"{pt.x:F3}, {pt.y:F3}, {pt.z:F3}";
        _input.SetTextWithoutNotify(text);
    }

    private void OnInputChanged(string text)
    {
        _input.colors = ErrorColor;

        var match = _parseRegex.Match(text);
        if (!match.Success || match.Groups.Count != 4)
            return;

        float x, y, z;
        if (!float.TryParse(match.Groups[1].Value, out x))
            return;
        if (!float.TryParse(match.Groups[2].Value, out y))
            return;
        if (!float.TryParse(match.Groups[3].Value, out z))
            return;

        //SystemManager.SystemConfig.CalibrationTestPoint = new YAMLVec3(x, y, z);
        _newValue = new Vector3(x, y, z);
        _valueChanged = true;
        _input.colors = NormalColor;
    }

}
