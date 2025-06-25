using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComponentInfo_EntryLabel : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Entry Number";

    [InspectableStringProperty("Label Text")]
    public string LabelText
    {
        get
        {
            return _labelText;
        }
        set
        {
            SetText(value);
        }
    }

    [InspectableNumericProperty("Font Size", MinValue = 5, MaxValue = 50, SliderControl = true)]
    public float FontSize
    {
        get
        {
            return _fontSize;
        }
        set
        {
            SetFontSize(value);
        }
    }

    public string ComponentInspectorTitle => "Map Label";

    //protected ObjectInfo objectInfo;
    public Inspector.ExposureLevel volumeExposureLevel;
    public MineMapSymbolRenderer m_symbolRenderer;
    public MineMapSymbol symbol;
    public MineMapTextSymbols symbolList;

    private TextMeshPro _label;
    private string _labelText;
    private float _fontSize = 25.0f;

    private void Awake()
    {
        //objectInfo = GetComponent<ObjectInfo>();
        //if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
        //if (objectInfo != null)
        //{
        //    if (!objectInfo.componentInfo_EntryLabels.Contains(this)) objectInfo.componentInfo_EntryLabels.Add(this);
        //}
        m_symbolRenderer = GetComponent<MineMapSymbolRenderer>();
        if(symbol == null) { symbol = symbolList.GetCollectionByString("Entry-1"); }

        _label = gameObject.GetComponentInChildren<TextMeshPro>();

    }
    public string[] SaveInfo()
    { 
        return new string[] { 
            "Label|" + _labelText,
            "FontSize|" + _fontSize,
        };
    }
    public string SaveName()
    {
        return componentName;
    }

    public void LoadInfo(SavedComponent component)
    {
        if(component == null) return;
        componentName = component.GetComponentName();
        //symbol = symbolList.GetCollectionByString(component.GetParamValueAsStringByName("Label"));

        string text = component.GetParamValueAsStringByName("Label");
        //m_symbolRenderer.Symbol.SymbolText = text;
        
        LabelText = text;
        
        //if (_label != null)
        //    _label.text = text;

        FontSize = component.GetParamValueFloat("FontSize", 25.0f);
    }

    internal void SetSymbol(MineMapSymbol symb)
    {
        symbol = symb;
        Debug.Log("SWITCHING TO: " + symb.name);
        m_symbolRenderer.Symbol = symb;
        Debug.Log("SET AS: " + m_symbolRenderer.Symbol);
    }

    private void SetText(string text)
    {
        _labelText = text;

        //if (symbol != null)
        //    symbol.SymbolText = text;
        
        if (m_symbolRenderer != null)
            m_symbolRenderer.SymbolText = text;

        if(_label != null)
            _label.text = text;
    }

    private void SetFontSize(float fontSize)
    {
        _fontSize = fontSize;

        //if (symbol != null)
        //    symbol.FontSize = fontSize;

        if (m_symbolRenderer != null)
        {
            m_symbolRenderer.Symbol.FontSize = fontSize;
            m_symbolRenderer.FontSize = fontSize;
        }

        //if (_label != null)
        //    _label.fontSizeMax = fontSize;

    }
}
