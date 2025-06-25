using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHotkey : MonoBehaviour
{
    public KeyCode ExitKey = KeyCode.Escape;
    public float HoldTime = 3;

    private bool _keyHeld = false;
    private float _targetTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if  (Input.GetKeyDown(ExitKey))
        {
            //Application.Quit();
            _keyHeld = true;
            _targetTime = Time.time + HoldTime;
        }
        if (Input.GetKeyUp(ExitKey))
        {
            _keyHeld = false;
        }

        if (_keyHeld && Time.time > _targetTime)
        {
            Application.Quit();
        }
    }
}
