using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UIToggleRoleEventVisibility : MonoBehaviour
{
    public SessionEventManager SessionEventManager;
    public VRNPlayerRole PlayerRole;

    private Toggle _toggle;    

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
        SessionEventManager.ShowRoleEvents(PlayerRole, toggleVal);
    }

    private void UpdateToggle()
    {
        if (_toggle == null)
            return;

        var visData = SessionEventManager.GetRoleVisibilityData(PlayerRole);
        _toggle.SetIsOnWithoutNotify(visData.EventsVisible);
    }
}
