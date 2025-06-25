using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class UIBtnSessionRec : MonoBehaviour
{
    public SystemManager SystemManager;
    public NetworkManager NetworkManager;
    public TeleportManager TeleportManager;

    public string RecordText = "Start Recording";
    public string StopText = "Stop Recording";

    private Button _button;
    private TextMeshProUGUI _buttonText;

    // Use this for initialization
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);

        _button = GetComponent<Button>();
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();

        _button.onClick.AddListener(OnButtonClicked);

        NetworkManager.SessionRecordingStarted += OnSessionRecordingChanged;
        NetworkManager.SessionRecordingStopped += OnSessionRecordingChanged;

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        UpdateButtonText();
    }

    private void OnSessionRecordingChanged()
    {
        UpdateButtonText();
    }

    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        UpdateButtonText();
    }


    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        if (NetworkManager != null)
        {
            NetworkManager.SessionRecordingStarted -= OnSessionRecordingChanged;
            NetworkManager.SessionRecordingStopped -= OnSessionRecordingChanged;
        }
    }

    private void OnButtonClicked()
    {
        var sessionRec = NetworkManager.GetActiveSessionRec();
        if (sessionRec == null)
        {
            NetworkManager.StartSessionRecording(TeleportManager.ActivePOIName, SystemManager.SystemConfig.SessionName);
            UpdateButtonText(true);
        }
        else
        {
            NetworkManager.StopSessionRecording();
            UpdateButtonText(false);
        }
    }

    private void UpdateButtonText()
    {
        if (NetworkManager == null)
            return;

        var sessionRec = NetworkManager.GetActiveSessionRec();
        if (sessionRec != null)
            UpdateButtonText(true);
        else
            UpdateButtonText(false);

    }

    private void UpdateButtonText(bool recording)
    {
        if (_buttonText == null)
            return;

        if (recording)
        {
            _buttonText.text = StopText;
        }
        else
        {
            _buttonText.text = RecordText;
        }
    }
}
