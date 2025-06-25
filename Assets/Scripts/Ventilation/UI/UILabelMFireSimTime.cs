using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UILabelMFireSimTime : MonoBehaviour
{
    public VentilationManager VentilationManager;
    public string FormatString = "{time}";


    private PlayerRepresentation _player;
    private TextMeshProUGUI _guiText;
    private TextMeshPro _text;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;

        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _text = GetComponent<TextMeshPro>();
        _guiText = GetComponent<TextMeshProUGUI>();

        //if (VentilationManager != null)
        //    VentilationManager.SimulationChanged += OnSimulationChanged;


        UpdateText();

        InvokeRepeating("UpdateText", 0, 0.3f);
    }

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        //if (VentilationManager != null)
        //    VentilationManager.SimulationChanged -= OnSimulationChanged;

        
        VentilationManager = VentilationManager.GetDefault(gameObject);

        //if (VentilationManager != null)
        //    VentilationManager.SimulationChanged += OnSimulationChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;

        //if (VentilationManager != null)
        //    VentilationManager.SimulationChanged -= OnSimulationChanged;
    }
    private void OnSimulationChanged()
    {
        UpdateText();
    }

    void UpdateText()
    {
        if (VentilationManager == null)
            return;

        string txt;

        var ts = new System.TimeSpan(0, 0, 0, 0, (int)VentilationManager.GetSimulationTime());
        var time = VentilationManager.GetSimulationTime().ToString("F1");
        time = ts.ToString();
        txt = FormatString.Replace("{time}", time);

        if (_guiText != null)
            _guiText.text = txt;
        if (_text != null)
            _text.text = txt;
    }

}
