using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class VentUIObjStatusText : MonoBehaviour
{
    public VentilationManager VentilationManager;

    private StringBuilder _statusText;
    private TextMeshProUGUI _textMeshUGUI;
    private TextMeshPro _textMesh;

    private VentUIObj _lastObj;

    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _statusText = new StringBuilder(1000);

        _textMesh = GetComponent<TextMeshPro>();
        _textMeshUGUI = GetComponent<TextMeshProUGUI>();

        if (_textMesh == null && _textMeshUGUI == null)
        {
            Debug.LogError("No text mesh on object");
            enabled = false;
            return;
        }

        VentilationManager.VentilationUpdated += OnVentilationUpdated;

        UpdateStatusText();
    }
    private void OnDestroy()
    {
        if (VentilationManager != null)
            VentilationManager.VentilationUpdated -= OnVentilationUpdated;
    }

    private void OnVentilationUpdated()
    {
        UpdateStatusText();
    }

    public void UpdateStatusText()
    {
        try
        {
            _statusText.Clear();


            //_statusText.AppendLine(VentilationManager.SelectedVentUIObj.ToString());
            var selObj = VentilationManager.SelectedVentUIObj;
            if (selObj == null)
                _statusText.AppendLine("No Obj Selected");
            else
                selObj.AppendText(_statusText);

            _lastObj = selObj;

            UpdateTextControls();

        }
        catch (Exception)
        {

        }
    }

    private void UpdateTextControls()
    {
        if (_textMesh != null)
            _textMesh.text = _statusText.ToString();
        if (_textMeshUGUI != null)
            _textMeshUGUI.text = _statusText.ToString();
    }

    private void Update()
    {

        if (_lastObj != VentilationManager.SelectedVentUIObj)
            UpdateStatusText();
    }
}
