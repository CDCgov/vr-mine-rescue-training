using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Clock : MonoBehaviour
{
    private MeshRenderer _labelMeshRenderer;
    private TMPro.TextMeshPro _tmProLabel;

    private void Start()
    {
        _labelMeshRenderer = GetComponent<MeshRenderer>();
        _tmProLabel = GetComponent<TMPro.TextMeshPro>();
    }
    private void OnEnable()
    {
        InvokeRepeating(nameof(UpdateWatchText), 1.0f, 1.0f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void UpdateWatchText()
    {
        if (!_labelMeshRenderer.enabled)
            return;

        DateTime dateTime = System.DateTime.Now;
        int hour = ((dateTime.Hour + 11) % 12) + 1;
        string minute = dateTime.Minute.ToString("00");
        if (dateTime.Second % 2 == 0)
        {
            _tmProLabel.text = $"{hour}:{minute}";
        }
        else
        {
            _tmProLabel.text = $"{hour} {minute}";
        }

    }
}
