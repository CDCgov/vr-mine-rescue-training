using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EventTooltipHandler : MonoBehaviour
{
    public TextMeshProUGUI TooltipText;
    public Button CloseButton;
    public GameObject TooltipPanel;
    public SessionEventManager SessionEventManager;

    private DebriefEventItem _priorSelectedItem;
    private bool _locked = false;
    // Start is called before the first frame update
    void Start()
    {
        TooltipText.text = "";
        CloseButton.onClick.AddListener(OnCloseBtnBehavior);
        if (TooltipPanel.activeSelf)
        {
            TooltipPanel.SetActive(false);
        }

        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);
    }


    public void OnCloseBtnBehavior()
    {
        TooltipPanel.SetActive(false);
        _priorSelectedItem.DisableHighlight();
        _priorSelectedItem = null;
    }

    public void ActivateTooltip(string tip, DebriefEventItem debriefEventItem)
    {
        if (_locked)
        {
            return;
        }
        TooltipText.text = tip;
        TooltipPanel.SetActive(true);
        if(_priorSelectedItem != null)
        {
            _priorSelectedItem.DisableHighlight();
        }
        SessionEventManager.HideHighlightCircle();
        debriefEventItem.EnableHighlight();
        _priorSelectedItem = debriefEventItem;
        _locked = true;
    }

    public void ActivateTooltipHover(string tip)
    {
        if (_locked)
        {
            return;
        }
        TooltipText.text = tip;
        TooltipPanel.SetActive(true);
        if (_priorSelectedItem != null)
        {
            _priorSelectedItem.DisableHighlight();
            _priorSelectedItem = null;
        }
        _locked = true;
    }

    public void ActivateTooltipHover(string tip, Vector2 position)
    {
        if (_locked)
        {
            return;
        }
        TooltipText.text = tip;
        TooltipPanel.SetActive(true);
        SessionEventManager.ShowHighlightCircle();
        SessionEventManager.PositionHighlightCircle(position);
        if (_priorSelectedItem != null)
        {
            _priorSelectedItem.DisableHighlight();
            _priorSelectedItem = null;
        }
        _locked = true;
    }
    private void FixedUpdate()
    {
        if (_locked)
        {
            _locked = false;
        }
    }
}
