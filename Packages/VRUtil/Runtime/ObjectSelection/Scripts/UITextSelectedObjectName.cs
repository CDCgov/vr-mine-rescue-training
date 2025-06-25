using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UITextSelectedObjectName : MonoBehaviour
{
    public SelectedObjectManager SelectedObjectManager;

    private TMP_Text _text;
    
    void Start()
    {
        if (SelectedObjectManager == null)
            SelectedObjectManager = SelectedObjectManager.GetDefault(gameObject);

        SelectedObjectManager.gameObject.tag = "Manager";
        _text = GetComponent<TMP_Text>();

        SelectedObjectManager.SelectionChanged += OnSelectionChanged;

        UpdateText();
    }

    private void OnDestroy()
    {
        if (SelectedObjectManager != null)
            SelectedObjectManager.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(GameObject obj)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (_text == null)
            return;

        var selObj = SelectedObjectManager.GetSelectableObjectComponent();
        if (selObj != null && selObj.ObjectName != null && selObj.ObjectName.Length > 0)
        {
            _text.text = selObj.ObjectName;
        }
        else if (SelectedObjectManager.SelectedObject != null)
        {
            _text.text = SelectedObjectManager.SelectedObject.name;
        }
        else
        {
            _text.text = "No Selection";
        }

        
    }
}
