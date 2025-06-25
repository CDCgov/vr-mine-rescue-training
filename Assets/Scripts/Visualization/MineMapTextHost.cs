using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MineMapSymbolRenderer))]
public class MineMapTextHost : MonoBehaviour
{
    public MineMapSymbolRenderer MineMapSymbolRenderer;
    public string Text;

    private void Awake()
    {
        MineMapSymbolRenderer.Symbol = new MineMapTextSymbol();
        MineMapSymbolRenderer.Symbol.SymbolText = Text;
    }
}
