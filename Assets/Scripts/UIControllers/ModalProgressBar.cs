using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModalProgressBar : MonoBehaviour
{
    public TMP_Text StatusText;
    public RectTransform ProgressBar;

    private static ModalProgressBar _instance;

    public static void ShowProgressBar(string message, float progress)
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<ModalProgressBar>(true);
        }

        if (_instance == null)
            return;

        _instance.SetStatusText(message);
        _instance.SetProgress(progress);

        _instance.gameObject.SetActive(true);
    }

    public static void HideProgressBar()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<ModalProgressBar>(true);
        }

        if (_instance == null)
            return;

        _instance.gameObject.SetActive(false);
    }

    public void SetStatusText(string statusText)
    {
        if (StatusText == null)
            return;

        StatusText.text = statusText;
    }

    public void SetProgress(float progress)
    {
        if (ProgressBar == null)
            return;

        var anchor = ProgressBar.anchorMax;
        anchor.x = progress;
        ProgressBar.anchorMax = anchor;
    }
}
