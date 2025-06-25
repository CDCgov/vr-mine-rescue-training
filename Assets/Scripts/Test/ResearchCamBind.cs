using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchCamBind : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var rc = GetComponent<ResearcherCamController>();
        var input = GetComponent<InputManager>();

        input.BindKeyboardAndMouse(rc);   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
