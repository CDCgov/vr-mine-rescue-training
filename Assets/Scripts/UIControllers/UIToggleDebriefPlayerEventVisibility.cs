using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggleDebriefPlayerEventVisibility : MonoBehaviour, ISelectedPlayerView
{
    public SessionEventManager SessionEventManager;

    private Toggle _toggle;
    private PlayerRepresentation _player;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);

        SessionEventManager.EventVisibilityChanged += OnEventVisibilityChanged;

        _toggle = GetComponent<Toggle>();

        _toggle.onValueChanged.AddListener(OnToggleChanged);

        UpdateToggle();
    }

    void OnDestroy()
    {
        SessionEventManager.EventVisibilityChanged -= OnEventVisibilityChanged;
    }

    private void OnEventVisibilityChanged()
    {
        UpdateToggle();
    }

    private void OnToggleChanged(bool toggleVal)
    {
        if (_player != null)
        {
            SessionEventManager.ShowPlayerEvents(_player.PlayerID, toggleVal);
        }
        else
        {
            SessionEventManager.ShowAllPlayerEvents(toggleVal);
        }
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        _player = player;
    }

    private void UpdateToggle()
    {
        if (_toggle == null)
            return;

        if (_player != null)
        {
            var visData = SessionEventManager.GetPlayerVisibilityData(_player.PlayerID);
            _toggle.SetIsOnWithoutNotify(visData.EventsVisible);
        }
        else
        {
            _toggle.SetIsOnWithoutNotify(SessionEventManager.AreAnyPlayerEventsEnabled());
        }
    }
}
