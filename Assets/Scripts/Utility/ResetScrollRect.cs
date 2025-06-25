using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ResetScrollRect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var scrollRect = GetComponent<ScrollRect>();

        scrollRect.normalizedPosition = Vector2.one;

    }

}
