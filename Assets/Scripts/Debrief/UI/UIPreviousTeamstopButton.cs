using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIPreviousTeamstopButton : MonoBehaviour
{
    public TimelineController TimelineController;
    private Button _button;
    // Start is called before the first frame update
    void Start()
    {
        if (TimelineController == null)
            TimelineController = TimelineController.GetDefault(gameObject);

        _button = GetComponent<Button>();
        _button.onClick.AddListener(PrevButtonClicked);
    }

    private void PrevButtonClicked()
    {
        TimelineController.GoToPreviousTeamstop();
    }
}
