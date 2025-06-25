using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBtnListSelectedObjectActions : MonoBehaviour
{
    public SelectedObjectManager SelectedObjectManager;
    public Transform ParentTransform;

    public GameObject ActionButtonPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (SelectedObjectManager == null)
            SelectedObjectManager = SelectedObjectManager.GetDefault(gameObject);

        SelectedObjectManager.gameObject.tag = "Manager";
        if (ParentTransform == null)
            ParentTransform = transform;

        if (ActionButtonPrefab == null)
        {
            gameObject.SetActive(false);
            this.enabled = false;
            Debug.LogError($"UIBtnListSelectedObjectActions: No button prefab");
            return;
        }

        SelectedObjectManager.SelectionChanged += OnSelectionChanged;
    }

    private void OnDestroy()
    {
        if (SelectedObjectManager != null)
            SelectedObjectManager.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(GameObject obj)
    {
        UpdateActionList();
    }
    
    void ClearActionList()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void UpdateActionList()
    {
        ClearActionList();

        if (SelectedObjectManager.SelectedObject == null)
            return;

        foreach (var action in SelectedObjectManager.GetSelectedObjectActions())
        {
            AddActionButtion(action);
        }
    }

    void AddActionButtion(ISelectableObjectAction action)
    {
        var obj = Instantiate<GameObject>(ActionButtonPrefab);
        obj.transform.SetParent(ParentTransform, false);

        var text = obj.GetComponentInChildren<TMP_Text>();
        var btn = obj.GetComponentInChildren<Button>();

        if (text == null || btn == null)
        {
            Debug.LogError("UIBtnListSelectedObjectActions: Prefab missing necessary components");
            Destroy(obj);
            return;
        }

        text.text = action.SelectableActionName;
        btn.onClick.AddListener(() => { action.PerformSelectableObjectAction(); });
    }
}
