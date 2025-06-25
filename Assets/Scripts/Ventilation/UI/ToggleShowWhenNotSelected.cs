using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleShowWhenNotSelected : MonoBehaviour
{
    public GameObject ObjectToShow;

    private Toggle _toggle;

    // Start is called before the first frame update
    void Start()    
    {
        if (ObjectToShow == null)
            return;

        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnToggleChanged);

        UpdateObjectVisibility();
    }

    private void OnToggleChanged(bool arg0)
    {
        UpdateObjectVisibility();
    }

    public void UpdateObjectVisibility()
    {
        if (ObjectToShow == null || _toggle == null)
            return;

        ObjectToShow.SetActive(!_toggle.isOn);
    }

}
