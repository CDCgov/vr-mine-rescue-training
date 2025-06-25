using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnShowAllEventMarkers : MonoBehaviour
{
    public SessionEventManager SessionEventManager;
    
    public enum ShowMarkerAction
    {
        Show,
        Hide,
        Toggle,
    };

    public ShowMarkerAction Action;

    private Button _button;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);

        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        if (Action == ShowMarkerAction.Show)
            SessionEventManager.ShowAllEventCategories(true);
        else if (Action == ShowMarkerAction.Hide)
            SessionEventManager.ShowAllEventCategories(false);
        else
        {
            //TODO: Implement toggle
            SessionEventManager.ShowAllEventCategories(true);
        }
    }
}
