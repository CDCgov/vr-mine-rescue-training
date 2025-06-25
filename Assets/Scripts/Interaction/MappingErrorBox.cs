using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MappingErrorBox : MonoBehaviour
{
    public Button OkButton;
    public TextMeshProUGUI ErrorTextBox;
    public RectTransform ErrorBoxRectTransform;

    private void Start()
    {
        if(OkButton == null)
        {
            OkButton = GetComponentInChildren<Button>();
        }
        if(ErrorTextBox == null)
        {
            ErrorTextBox = GetComponentInChildren<TextMeshProUGUI>();
        }
        if(ErrorBoxRectTransform == null)
        {
            TryGetComponent<RectTransform>(out ErrorBoxRectTransform);
        }
    }
}
