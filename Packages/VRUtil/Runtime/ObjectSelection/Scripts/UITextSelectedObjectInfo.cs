using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UITextSelectedObjectInfo : MonoBehaviour
{
    public SelectedObjectManager SelectedObjectManager;

    private TMP_Text _text;
    private StringBuilder _sb;

    void Start()
    {
        if (SelectedObjectManager == null)
            SelectedObjectManager = SelectedObjectManager.GetDefault(gameObject);

        SelectedObjectManager.gameObject.tag = "Manager";
        _text = GetComponent<TMP_Text>();
        _sb = new StringBuilder();

        SelectedObjectManager.SelectionChanged += OnSelectionChanged;

        //UpdateText();

        InvokeRepeating(nameof(UpdateText), 0, 0.5f);
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

        if (SelectedObjectManager.SelectedObject == null)
        {
            _text.text = "";
            return;
        }

        _sb.Clear();
        SelectedObjectManager.GetSelectedObjectInfo(_sb);

        _text.text = _sb.ToString();
    }
}
