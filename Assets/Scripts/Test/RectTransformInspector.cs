using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformInspector : MonoBehaviour
{
    RectTransform rt;
    [SerializeField] Vector2 worldPosition;
    [SerializeField] Vector2 localPosition;
    [SerializeField] Vector2 anchoredPosition;

    [SerializeField] Vector2 rectPosition;
    [SerializeField] Vector2 rectCenter;
    [SerializeField] Vector2 size;
    [SerializeField] Vector2 rectSize;
    [SerializeField] Vector2 rectRangeY;
    [SerializeField] Vector2 rectRangeX;

    public Vector2 cachedSize;
    public event Action onSizeChanged;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cachedSize = rt.rect.size;
    }

    private void Update()
    {
        worldPosition = rt.position;
        localPosition = rt.localPosition;
        anchoredPosition = rt.anchoredPosition;

        rectPosition = new Vector2(rt.rect.x, rt.rect.y);
        rectCenter = rt.rect.center;
        size = rt.rect.size;
        rectSize = new Vector2(rt.rect.width, rt.rect.height);
        rectRangeY = new Vector2(rt.rect.yMin, rt.rect.yMax);
        rectRangeX = new Vector2(rt.rect.xMin, rt.rect.xMax);

        if (size != cachedSize)
        {
            onSizeChanged?.Invoke();
        }
        cachedSize = size;
    }
}
