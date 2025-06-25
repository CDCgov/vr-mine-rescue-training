using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectedObjectPanel : MonoBehaviour
{
    public SelectedObjectManager SelectedObjectManager;

    // Start is called before the first frame update
    void Start()
    {
        if (SelectedObjectManager == null)
            SelectedObjectManager = SelectedObjectManager.GetDefault(gameObject);


        SelectedObjectManager.gameObject.tag = "Manager";
        SelectedObjectManager.SelectionChanged += OnSelectionChanged;

        UpdateVisibility();
    }

    private void OnSelectionChanged(GameObject obj)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (SelectedObjectManager == null)
            return;

        if (SelectedObjectManager.SelectedObject == null)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    }
}
