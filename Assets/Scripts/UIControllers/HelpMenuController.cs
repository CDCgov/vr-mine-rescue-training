using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpMenuController : MonoBehaviour
{
    [SerializeField]
    public CanvasGroup HelpMenuCanvasGroup;

    private bool _isOpen = false;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F1))
        {
            if (_isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
        if(_isOpen && Input.GetKeyUp(KeyCode.Escape))
        {
            Close();
        }
    }

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
