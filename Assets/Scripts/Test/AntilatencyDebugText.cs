using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AntilatencyDebugText : MonoBehaviour, IStatusText
{
    public AntilatencyManager AntilatencyManager;
    public bool UpdateTextDirectly = true;

    private StringBuilder _statusText;
    private TextMeshProUGUI _textMeshUGUI;
    private TextMeshPro _textMesh;

    private AltPoseDriver _poseDriver;

    // Start is called before the first frame update
    void Start()
    {
        if (AntilatencyManager == null)
            AntilatencyManager = AntilatencyManager.GetDefault();

        _statusText = new StringBuilder(1000);

        _textMesh = GetComponent<TextMeshPro>();
        _textMeshUGUI = GetComponent<TextMeshProUGUI>();

        //if (_textMesh == null && _textMeshUGUI == null)
        //{
        //    Debug.LogError("No text mesh on object");
        //    enabled = false;
        //    return;
        //}

        AntilatencyManager.NetworkChanged += OnNetworkChanged;

        if (UpdateTextDirectly)
            UpdateStatusText();
        //StartCoroutine(UpdateStatusTextCo());
    }

    private void OnEnable()
    {
        if (UpdateTextDirectly)
            StartCoroutine(UpdateStatusTextCo());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    void OnNetworkChanged()
    {

    }

    IEnumerator UpdateStatusTextCo()
    {
        while (true)
        {
            UpdateStatusText();    

            if (_textMesh != null)
                _textMesh.text = _statusText.ToString();
            if (_textMeshUGUI != null)
                _textMeshUGUI.text = _statusText.ToString();

            yield return new WaitForSeconds(0.2f);
        }
    }

    public void UpdateStatusText()
    {
        if (!UpdateTextDirectly)
            return;

        try
        {            
            _statusText.Clear();
            AppendStatusText(_statusText);
        }
        catch (Exception)
        {

        }
    }

    public void AppendStatusText(StringBuilder statusText)
    {
        try
        {
            if (_poseDriver == null)
            {
                _poseDriver = GameObject.FindObjectOfType<AltPoseDriver>();
            }

            if (AntilatencyManager.Trackers == null || AntilatencyManager.Trackers.Count <= 0)
            {
                statusText.AppendLine("No Trackers Found\n");
                return;
            }

            statusText.AppendLine($"{AntilatencyManager.Trackers.Count} Tracker(s)");

            if (_poseDriver != null)
            {
                statusText.AppendLine(_poseDriver.GetStatusText());
            }
        }
        catch (Exception) { }
    }
}
