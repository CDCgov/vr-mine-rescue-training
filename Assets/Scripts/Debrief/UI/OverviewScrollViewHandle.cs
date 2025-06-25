using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class OverviewScrollViewHandle : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public DebriefOverviewUIController dbUICont;
    // Start is called before the first frame update
    void Start()
    {
        ScrollRect = GetComponent<ScrollRect>();
        ScrollRect.onValueChanged.AddListener(ScrollChanged);
        if(dbUICont == null)
        {
            dbUICont = FindObjectOfType<DebriefOverviewUIController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScrollChanged(Vector2 val)
    {
        Debug.Log($"Scroll value changed: {val.x}X, {val.y}Y");
        dbUICont.ScrollViewChanged(val);
    }
}
