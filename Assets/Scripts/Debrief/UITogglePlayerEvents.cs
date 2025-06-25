using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UITogglePlayerEvents : MonoBehaviour
{
    public SessionEventManager SessionEventManager;

    public string SelectedPlayerVar = "SELECTED_PLAYER";

    private Toggle _toggle;
    private UIContextData _context;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);

        _toggle = GetComponent<Toggle>();
        _context = transform.GetComponentInParent<UIContextData>();

        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool val)
    {
        PlayerRepresentation player = null;

        if (_context != null)
        {
            player = _context.GetVariable(SelectedPlayerVar) as PlayerRepresentation;
        }

        if (player != null)
        {

        }
        else
        {

        }
    }
}