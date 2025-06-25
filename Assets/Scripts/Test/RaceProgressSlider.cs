using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceProgressSlider : MonoBehaviour
{
    public Slider ProgressSlider;
    private Vector3 _startPosition;
    // Start is called before the first frame update
    void Start()
    {
        _startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        ProgressSlider.value = (transform.position.z - _startPosition.z) / 200;
    }
}
