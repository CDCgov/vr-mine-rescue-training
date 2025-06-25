using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescaleRectTransform : MonoBehaviour
{
    public RectTransform TargetObject;

    public float TargetSize;
    public float Margin = 25.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (TargetObject == null)
        {
            Debug.LogError($"No target object on RescaleRectTransform {gameObject.name}");
            this.enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetObject == null)
            return;

        var rt = transform.parent as RectTransform;
        var goalRect = rt.rect;

        var targetSize = TargetObject.sizeDelta;

        ////var aspect = TargetObject.rect.width / TargetObject.rect.height;
        var aspect = targetSize.x / targetSize.y;

        //float scale = 1.0f;
        //if (goalRect.width > goalRect.height)
        //    scale = ((goalRect.height - Margin * 2.0f) * aspect) / TargetSize;
        //else
        //    scale = (goalRect.width - Margin * 2.0f) / TargetSize;

        var goalWidth = goalRect.width - Margin * 2.0f;
        var goalHeight = goalRect.height - Margin * 2.0f;

        var scaleX = goalWidth / targetSize.x;
        var scaleY = goalHeight / targetSize.y;

        var scale = Mathf.Min(scaleX, scaleY);

        if (float.IsNaN(scale) || float.IsInfinity(scale))
            scale = 1.0f;

        //TargetObject.anchoredPosition = new Vector2(Margin, Margin);
        //TargetObject.sizeDelta = new Vector2(TargetSize, TargetSize);
        //TargetObject.localScale = new Vector3(scale, scale, 1);
        transform.localScale = new Vector3(scale, scale, 1);

    }
}
