using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalkDNITextHandle : MonoBehaviour
{
    public TextTexture TTScript;
    public bool UseGenericDate = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (!UseGenericDate)
        {
            TTScript.Text = "X\n" + System.DateTime.Today.Month.ToString() + "/" + System.DateTime.Today.Day.ToString();
        }
        else
        {
            TTScript.Text = "X\nMM/DD";
        }
    }
}
