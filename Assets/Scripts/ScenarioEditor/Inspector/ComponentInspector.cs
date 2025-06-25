using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class ComponentInspector : MonoBehaviour
{
    public abstract void SetTargetComponent(ModularComponentInfo component);
}

public abstract class ComponentInspector<T> : ComponentInspector where T: ModularComponentInfo
{
    [System.NonSerialized]
    public T TargetComponentInfo;

    RectTransformInspector _rti;
    WindowLayoutController _wlc;

    public virtual void Start()
    {
        if (TargetComponentInfo == null)
        {
            Debug.LogError($"TargetComponentInfo not assigned on {gameObject.name}");
        }
        //_rti = GetComponentInParent<RectTransformInspector>();
        //_wlc = GetComponentInParent<WindowLayoutController>();
        //if (_rti != null)
        //    _rti.onSizeChanged += OnSizeChange;
        //if (_wlc != null)
        //_wlc.onLayoutChanged += OnSizeChange;
    }
        

    public virtual void OnDestroy()
    {
        //if (_rti != null)
        //    _rti.onSizeChanged -= OnSizeChange;
        //if (_wlc != null)
        //    _wlc.onLayoutChanged -= OnSizeChange;
    }

    public override void SetTargetComponent(ModularComponentInfo component)
    {
        TargetComponentInfo = component as T;

        if (TargetComponentInfo == null)
        {
            Debug.LogError($"Invalid ModularComponentInfo set on {gameObject.name}");
        }

        //UpdateInspector(); 
    }

    //public abstract void UpdateInspector();
    /// <summary>
    /// Attempts to resize the children such that it should stretch properly.
    /// </summary>
    //private void OnSizeChange()
    //{
    //    if(this == null)
    //    {
    //        return;
    //    }
    //    Debug.Log($"Transform change detected: {gameObject.name}");
    //    RectTransform myRect = this.GetComponent<RectTransform>();
    //    //RectTransform parentRect = this.transform.parent.GetComponent<RectTransform>();
    //    //if(myRect != null && parentRect != null)
    //    //{
    //    //    Vector2 delta = myRect.sizeDelta;
    //    //    delta.x = parentRect.sizeDelta.x;
    //    //    myRect.sizeDelta = delta;
    //    //}
    //    Vector2 myDelta = myRect.sizeDelta;
    //    myDelta.x = _rti.cachedSize.x;
    //    myRect.sizeDelta = myDelta;
    //}
}
