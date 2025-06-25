using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class StatusTextDisplay : MonoBehaviour
{

    public float TextUpdateDelay = 0.2f;
    private StringBuilder _statusText;
    private TextMeshProUGUI _textMeshUGUI;
    private TextMeshPro _textMesh;

    private IStatusText[] _statusTextInterfaces;

    // Start is called before the first frame update
    void Start()
    {
        _statusText = new StringBuilder(1000);

        _textMesh = GetComponent<TextMeshPro>();
        _textMeshUGUI = GetComponent<TextMeshProUGUI>();

        if (_textMesh == null && _textMeshUGUI == null)
        {
            Debug.LogError("No text mesh on object");
            enabled = false;
            return;
        }

        _statusTextInterfaces = GetComponents<IStatusText>();
        UpdateStatusText();
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateStatusTextCo());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator UpdateStatusTextCo()
    {
        while (true)
        {
            try
            {
                UpdateStatusText();
            }
            catch (Exception ex)
            {
                Debug.LogError($"StatusTextDisplay: Error updating text {ex.Message} {ex.StackTrace}");
            }
            yield return new WaitForSeconds(TextUpdateDelay);
        }
    }

    public void UpdateStatusText()
    {
        try
        {
            _statusText.Clear();

            if (_statusTextInterfaces == null)
            {
                _statusText.Append("No status text interfaces");
                UpdateTextControls();
                return;
            }

            foreach (var txtInterface in _statusTextInterfaces)
            {
                try
                {
                    txtInterface.AppendStatusText(_statusText);
                }
                catch (Exception ex)
                {
                    _statusText.AppendLine(ex.Message);
                }
            }

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

}
