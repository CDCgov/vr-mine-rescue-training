using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class HighlightTextMeshUI : MonoBehaviour
{
    public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    public Button SelectButton;
    public UnityEvent<HighlightTextMeshUI> ScenarioSelected;
    public int SelectedLength;

    public void ButtonClicked()
    {
        ScenarioSelected.Invoke(this);
    }

    public void Selected()
    {
        foreach(TextMeshProUGUI text in texts)
        {
            text.color = Color.yellow;
        }
    }

    public void UnSelected()
    {

        foreach (TextMeshProUGUI text in texts)
        {
            text.color = Color.white;
        }
    }
}
