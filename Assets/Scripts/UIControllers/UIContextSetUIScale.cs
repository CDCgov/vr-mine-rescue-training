using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContextSetUIScale : UIContextBase
{
    public SystemManager SystemManager;

    private Canvas _canvas;

    protected override void Start()
    {
        base.Start();

        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        
        _context.SetVariable("UI_SCALE", SystemManager.SystemConfig.UIScale);
        _canvas = GetComponentInParent<Canvas>();

        _context.ContextDataChanged += OnContextDataChanged;

        UpdateUIScale(SystemManager.SystemConfig.UIScale);
    }

    private void OnContextDataChanged(string obj)
    {
        if (obj != "UI_SCALE")
            return;

        float scale = _context.GetFloatVariable("UI_SCALE");
        UpdateUIScale(scale);
    }

    private void UpdateUIScale(float scale)
    {
        Debug.Log($"Setting UI Scale to {scale:F2}");

        _canvas.scaleFactor = scale;
    }
}
