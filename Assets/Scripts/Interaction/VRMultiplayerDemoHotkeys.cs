using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.PostProcessing;

public class VRMultiplayerDemoHotkeys : MonoBehaviour
{
    public float FadeDuration = 1.5f;

    //private PostProcessVolume _postVolume;
    //private AutoExposure _autoExposure;

    private bool _fading = false;
    private float _fadeStart = 0;
    private float _fadeProgress = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        var volumeObj = GameObject.Find("PostProcessVolume");
        if (volumeObj == null)
            return;

        //_postVolume = volumeObj.GetComponent<PostProcessVolume>();
        //if (_postVolume == null)
        //    return;

        //_postVolume.profile.TryGetSettings<AutoExposure>(out _autoExposure);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // if (_autoExposure != null)
            // {
            //     _autoExposure.keyValue.value = Random.Range(0.0f, 10.0f);
            // }

            _fading = true;
            _fadeProgress = 0;
            _fadeStart = Time.time;
        }

        //if (_fading && _autoExposure != null)
        //{
        //    _fadeProgress = (Time.time - _fadeStart) / FadeDuration;

        //    float fadeValue = 0;
        //    if (_fadeProgress >= 1.0f)
        //    {
        //        fadeValue = 1.0f;
        //        _fading = false;
        //    }
        //    if (_fadeProgress < 0.5f)
        //    {
        //        fadeValue = 1.0f - (_fadeProgress * 2);
        //    }
        //    else
        //    {
        //        fadeValue = (_fadeProgress - 0.5f) * 2;
        //    }

        //    _autoExposure.keyValue.value = fadeValue;

            
        //}
    }
}
