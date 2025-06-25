using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseButtonController : MonoBehaviour
{
    public Button CloseButton;

    // Start is called before the first frame update
    void Start()
    {        
        if (CloseButton != null)
        {
            CloseButton.onClick.AddListener(OnCloseButton);
        }
    }

    void OnCloseButton()
    {
        Destroy(gameObject);
    }

}