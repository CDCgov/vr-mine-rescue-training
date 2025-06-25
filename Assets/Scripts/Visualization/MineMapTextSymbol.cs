using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[HasCommandConsoleCommands]
[CreateAssetMenu(fileName = "MineMapTextSymbol", menuName = "VRMine/MineTextSymbol", order = 0)]
public class MineMapTextSymbol : MineMapSymbol
{


    TextMeshProUGUI[] _textMeshes;

    public override GameObject Instantiate()
    {
        GameObject prefab;
        if (SymbolPrefab == null) 
        {
            prefab = Resources.Load<GameObject>("MineTextSymbol");
        }
        else
        {
            prefab = Resources.Load<GameObject>(SymbolPrefab.name);//Presume that it needs to be loaded from Resources to avoid reference issues???
        }
        var obj = Instantiate<GameObject>(prefab);

        _text = obj.GetComponentInChildren<TextMeshProUGUI>();

        _text.text = SymbolText;
        
        if (FontSize > 0 && _text.enableAutoSizing)
            _text.fontSizeMax = FontSize;

        SetColor(obj, Color);

        return obj;
    }

    public void RefreshText()
    {
        _text.text = SymbolText;
    }

    public override void SetColor(GameObject instance, Color newColor)
    {
        _textMeshes = instance.GetComponentsInChildren<TextMeshProUGUI>();
        if (_textMeshes != null) { 
            //Debug.Log($"Setting Text Symbol color to {newColor.ToString()}");
            foreach (var textItem in _textMeshes)
            {
                textItem.color = newColor;
            }
            //text.color = newColor;
        }
        Color = newColor;
    }

    public override VRNSymbolData GetSymbolData()
    {
        var data = base.GetSymbolData();
        data.SymbolClass = "TextSymbol";

        return data;
    }
}
