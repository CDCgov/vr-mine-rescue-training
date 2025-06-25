using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MinimizableWindowController : MonoBehaviour, IMinimizableWindow
{
    //    public Button MinimizeButton;
    public string Title = "Unknown";

    protected bool _minimized = false;

    public event Action<string> TitleChanged;
    public event Action<bool, RectTransform> StateChanged;



    public Color MinimizedColor;
    public Color MinimizedTextColor;
    public Color MaximizedColor;

    [Tooltip("Optional for disabling GUI interaction and visability but maintainining active status of scripts. Leave empty to simply disable the window game object when minimized")]
    [SerializeField ]CanvasGroup _canvasGroup;
    
    Button taskbarButton;
    TextMeshProUGUI tmp;
    ColorBlock colorBlock;

    public void Minimize(bool minimize)
    {
        if(_canvasGroup == null)
        {
            gameObject.SetActive(minimize);
        }
        else
        {
            _canvasGroup.interactable = minimize;
            _canvasGroup.blocksRaycasts = minimize;
            if (minimize){ _canvasGroup.alpha = 1;}
            else { _canvasGroup.alpha = 0; }
        }
        StateChanged?.Invoke(minimize, this.transform as RectTransform);
    }

    public void AssignTaskbarButton(Button button)
    {
        taskbarButton = button;
        tmp = taskbarButton.GetComponentInChildren<TextMeshProUGUI>();
        colorBlock = taskbarButton.colors;
    }
    public void ToggleMinimize()
    {
        bool minimized = false;
        if (_canvasGroup == null)
        {
            gameObject.SetActive(!gameObject.activeSelf);
            minimized = !gameObject.activeSelf;
        }
        else
        {
            _canvasGroup.interactable = !_canvasGroup.interactable;
            _canvasGroup.blocksRaycasts = !_canvasGroup.blocksRaycasts;
            if (_canvasGroup.alpha == 1) 
            {
                _canvasGroup.alpha = 0;
                minimized = true;
            }
            else 
            {
                _canvasGroup.alpha = 1;
                minimized = false;
            }
        }
        ChangeTaskbarButton(minimized);
        StateChanged?.Invoke(minimized, this.transform as RectTransform);
    }
    void ChangeTaskbarButton(bool minimized)
    {
        if (!taskbarButton) return;

        if (minimized)
        {
            tmp.color = MinimizedTextColor;
            colorBlock.normalColor = MinimizedColor;
            colorBlock.selectedColor = MinimizedColor;
            taskbarButton.colors = colorBlock;
        }
        else
        {
            tmp.color = Color.white;
            colorBlock.normalColor = MaximizedColor;
            colorBlock.selectedColor = MaximizedColor;
            taskbarButton.colors = colorBlock;
        }
    }

    public string GetTitle()
    {
        return Title;
    }
}
