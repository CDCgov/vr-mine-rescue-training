using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIColorPresetBtn : MonoBehaviour
{
    private ComponentInspector_Light _componentInspectorLight;
    private ComponentInspector_Color _componentInspector_Color;    
    private MenuColorPicker _menuColorPicker;
    private ColorField _colorField;

    private Button _btn;
    private Color _color;
    // Start is called before the first frame update
    void Start()
    {
        _btn = GetComponent<Button>();
        _componentInspectorLight = GetComponentInParent<ComponentInspector_Light>();
        _componentInspector_Color = GetComponentInParent<ComponentInspector_Color>();
        _colorField = GetComponentInParent<ColorField>();
        
        _menuColorPicker = GetComponentInParent<MenuColorPicker>();
        _color = _btn.image.color;
        _btn.onClick.AddListener(ColorPicked);
    }

    private void OnDestroy()
    {
        if(_btn != null)
            _btn.onClick.RemoveListener(ColorPicked);
    }

    void ColorPicked()
    {
        //_componentInspectorLight.SetRed(_color.r * 255, true);        
        //_componentInspectorLight.SetGreen(_color.g * 255, true);
        //_componentInspectorLight.SetBlue(_color.b * 255, true);
        if (_componentInspectorLight != null)
        {
            _componentInspectorLight.SetColor(_color, true);
        }
        else if (_componentInspector_Color)
        {
            _componentInspector_Color.SetColor(_color, true);
        }
        else if (_menuColorPicker != null)
        {
            _menuColorPicker.SetLightColorPreset(_color.r, _color.g, _color.b);
        }
        else if (_colorField != null)
        {
            _colorField.SetColor(_color);
        }


    }
}
