using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MineMapSymbolRenderer))]
public class MineMapSymbolAnnotation : MonoBehaviour
{
    public MineMapSymbolRenderer MineMapSymbolRenderer;
    public string Text;
    public MineMapSymbol Parent;

    private void Awake()
    {
        MineMapSymbolRenderer.Symbol = new MineMapTextSymbol();
        MineMapSymbolRenderer.Symbol.SymbolText = Text;

        //if (Parent != null)
        //{ 
        //    Parent.ChildSymbol = MineMapSymbolRenderer.Symbol;
        //}
    }
}
