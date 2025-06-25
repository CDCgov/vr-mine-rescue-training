using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UITextLoadedSessionInfo : MonoBehaviour
{
    public SessionPlaybackControl SessionPlaybackControl;

    private TMP_Text _text;

    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);

        SessionPlaybackControl.SessionLoaded += OnSessionLoaded;

        _text = GetComponent<TMP_Text>();

        UpdateText();
    }

    private void OnSessionLoaded()
    {
        UpdateText();
    }

    void UpdateText()
    {
        var currentLog = SessionPlaybackControl.CurrentSessionLog;

        if (currentLog == null)
        {
            _text.text = "No session loaded";
            return;
        }

        string datestr;
        if (currentLog.SessionDateTime == null)
            datestr = "Unknown Date";
        else
            datestr = ((System.DateTime)currentLog.SessionDateTime).ToString("f");

        _text.text = $"{currentLog.SceneName}\n{currentLog.SessionName}\n{datestr}";
            

    }
}
