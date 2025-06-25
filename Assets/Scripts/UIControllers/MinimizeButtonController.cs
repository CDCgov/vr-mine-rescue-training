using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimizeButtonController : MonoBehaviour
{
    public Button MinimizeButton;
    private IMinimizableWindow _win;

    // Start is called before the first frame update
    void Start()
    {
        _win = GetComponent<IMinimizableWindow>();

        if (MinimizeButton != null)
        {
            MinimizeButton.onClick.AddListener(OnMinimizeButtonClicked);
        }
    }

    void OnMinimizeButtonClicked()
    {
        if (_win != null)
        {
            _win.ToggleMinimize();
        }
    }

}
