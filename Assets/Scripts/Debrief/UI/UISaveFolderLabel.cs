using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UISaveFolderLabel : MonoBehaviour
{
    private TextMeshProUGUI _label;
    public DebriefFileController DebriefFileController;
    // Start is called before the first frame update
    void Start()
    {
        if (_label == null)
        {
            _label = GetComponent<TextMeshProUGUI>();
        }
        if (DebriefFileController == null)
        {
            DebriefFileController = FindObjectOfType<DebriefFileController>();
            if(DebriefFileController == null)
            {
                Debug.LogError("No debrief file controller found.");
            }
        }
    }

    private void OnEnable()
    {
        if(_label == null)
        {
            _label = GetComponent<TextMeshProUGUI>();
        }
        if(DebriefFileController == null)
        {
            Debug.LogError("Debrief file controller was null!");
        }
        _label.text = DebriefFileController.GetSaveFolder();
        DebriefFileController.SaveFolderChange.AddListener(RefreshLabel);
    }

    private void OnDisable()
    {
        if (DebriefFileController != null)
            DebriefFileController.SaveFolderChange.RemoveListener(RefreshLabel);
    }

    public void RefreshLabel()
    {
        _label.text = DebriefFileController.GetSaveFolder();
    }

    private void OnDestroy()
    {
        
    }
}
