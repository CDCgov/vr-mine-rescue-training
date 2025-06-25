using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleRadioGuiDemo : MonoBehaviour {

    public Text TextBox;
    public Image[] UiImages;
    public CanvasGroup CGroup;

    private string priorMessage = "";
    private float delay = 0;
    private bool fadeoutFlat = false;
    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        if (TextBox.text != priorMessage)
        {
            priorMessage = TextBox.text;
            delay = Time.time + 1;
            fadeoutFlat = true;
            CGroup.alpha = 1;
        }
        if(Time.time > delay)
        {
            if (fadeoutFlat)
            {
                if(CGroup.alpha > 0)
                {
                    CGroup.alpha = CGroup.alpha - 0.5f*Time.deltaTime;
                }
                else
                {
                    fadeoutFlat = false;
                }
            }
        }
    }
}
