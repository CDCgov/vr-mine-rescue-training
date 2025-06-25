using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DebriefSelectionHandler : MonoBehaviour
{
    public TextMeshProUGUI FileNameLabel;
    public TextMeshProUGUI SessionNameLabel;
    public TextMeshProUGUI ScenarioNameLabel;
    public TextMeshProUGUI DateLabel;
    public TextMeshProUGUI TimeLabel;
    public TextMeshProUGUI DurationLabel;
    public Button SelectButton;
    public UnityEvent<DebriefSelectionHandler> ScenarioSelected;
    public int SelectedLength;
    
    public void ButtonClicked()
    {
        ScenarioSelected.Invoke(this);
    }

    public void Selected()
    {
        FileNameLabel.color = Color.yellow;
        SessionNameLabel.color = Color.yellow;
        ScenarioNameLabel.color = Color.yellow;
        DateLabel.color = Color.yellow;
        TimeLabel.color = Color.yellow;
        DurationLabel.color = Color.yellow;
    }

    public void UnSelected()
    {
        FileNameLabel.color = Color.white;
        SessionNameLabel.color = Color.white;
        ScenarioNameLabel.color = Color.white;
        DateLabel.color = Color.white;
        TimeLabel.color = Color.white;
        DurationLabel.color = Color.white;
    }
}
