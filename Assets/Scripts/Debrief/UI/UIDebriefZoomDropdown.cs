using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class UIDebriefZoomDropdown : MonoBehaviour
{
    public TMP_Dropdown ZoomDropdown;
    public UIBtnSessionPlayPause uIBtnSessionPlayPause;
    public Slider ZoomSlider;
    public GameObject CustomZoomInputPanel;

    private int _savedSpeed = 1;
    // Start is called before the first frame update
    void Start()
    {
        if (ZoomDropdown == null)
        {
            ZoomDropdown = GetComponent<TMP_Dropdown>();
        }        
        ZoomDropdown.onValueChanged.AddListener(OnSpeedChange);
    }

    private void OnSpeedChange(int arg0)
    {
        switch (ZoomDropdown.value)
        {
            case 0:
                ZoomSlider.value = 0;
                _savedSpeed = 0;
                break;
            case 1:
                ZoomSlider.value = 0.5f;
                _savedSpeed = 1;
                break;
            case 2:
                ZoomSlider.value = 0.78f;
                _savedSpeed = 2;
                break;
            case 3:
                ZoomSlider.value = 0.9f;
                _savedSpeed = 3;
                break;            
            default:
                break;
        }

    }

    public void OnReset()
    {
        ZoomDropdown.value = 0;
    }
}
