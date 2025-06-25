using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningArrow : MonoBehaviour
{
    private bool _isGrowing = true;
    private float _time = 0;
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Time.deltaTime);
        transform.Rotate(0, Time.deltaTime * 10, 0);
        if (_isGrowing)
        {
            _time = Time.deltaTime+ _time;
            float scale = Mathf.Lerp(1, 1.5f, _time);
            transform.localScale = new Vector3(scale, scale, scale);
            if (_time > 1)
            {
                _time = 0;
                _isGrowing = false;
            }
        }
        else
        {
            _time = Time.deltaTime + _time;
            float scale = Mathf.Lerp(1.5f, 1, _time);
            transform.localScale = new Vector3(scale, scale, scale);
            if (_time > 1)
            {
                _time = 0;
                _isGrowing = true;
            }
        }
    }
}
