using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class UITextTeamstopStatus : MonoBehaviour
{
    public SessionPlaybackControl SessionPlaybackControl;
    private TextMeshProUGUI _text;

    SessionTeamstopState _currentTeamstop;
    StringBuilder _sb;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);

        _sb = new StringBuilder();
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void AppendTeamstopInfo(SessionTeamstopState ts)
    {
        _sb.AppendLine($"TS{ts.TeamstopIndex} Location: {ts.TeleportTarget} (from {ts.TeamstopStartTime:F2} to {ts.TeamstopEndTime:F2}");
    }

    void Update()
    {
        if (SessionPlaybackControl.CurrentSessionLog == null)
            return;

        if (SessionPlaybackControl.CurrentSessionLog.CurrentTeamstopState == _currentTeamstop)
            return;

        _currentTeamstop = SessionPlaybackControl.CurrentSessionLog.CurrentTeamstopState;
        _sb.Clear();

        _sb.AppendLine("Current Teamstop:");
        AppendTeamstopInfo(_currentTeamstop);

        _sb.AppendLine("Session Teamstops:");
        foreach (var teamstop in SessionPlaybackControl.CurrentSessionLog.GetTeamstops())
        {
            AppendTeamstopInfo(teamstop);
        }
        _text.text = _sb.ToString();
    }
}
