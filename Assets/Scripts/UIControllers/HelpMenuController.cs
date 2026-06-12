using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HelpMenuController : MonoBehaviour
{
    [SerializeField]
    public CanvasGroup HelpMenuCanvasGroup;

    private bool _isOpen = false;

    private InputActionEventManager _inputActions;

    // Start is called before the first frame update
    void Start()
    {
        if(HelpMenuCanvasGroup == null)
        {
            HelpMenuCanvasGroup = GetComponent<CanvasGroup>();
        }
        HelpMenuCanvasGroup.alpha = 0;
        HelpMenuCanvasGroup.interactable = false;
        HelpMenuCanvasGroup.blocksRaycasts = false;

        _inputActions.RegisterPerformedHandler("Help", (context) =>
        {
            if (_isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        });

        _inputActions.RegisterPerformedHandler("Cancel", (context) =>
        {
            if (_isOpen)
                Close();
        });
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
            _inputActions.Dispose();
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.F1))
    //    {
    //        if (_isOpen)
    //        {
    //            Close();
    //        }
    //        else
    //        {
    //            Open();
    //        }
    //    }
    //    if(_isOpen && Input.GetKeyUp(KeyCode.Escape))
    //    {
    //        Close();
    //    }
    //}

    public void Close()
    {
        HelpMenuCanvasGroup.alpha = 0;
        HelpMenuCanvasGroup.interactable = false;
        HelpMenuCanvasGroup.blocksRaycasts = false;
        _isOpen = false;
    }
    public void Open()
    {
        HelpMenuCanvasGroup.alpha = 1;
        HelpMenuCanvasGroup.interactable = true;
        HelpMenuCanvasGroup.blocksRaycasts = true;
        _isOpen = true;
    }
}
