using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineMapSymbolQuadrantRotateSwap : MonoBehaviour
{
    public MineMapSymbolRenderer SymbolRenderer;

    public List<MineMapSymbol> BaseSymbols;
    
    private void Awake()
    {
        if (SymbolRenderer == null)
        {
            SymbolRenderer = GetComponent<MineMapSymbolRenderer>();
        }
        Debug.Log($"Changing symbol renderer based on rotation of: {transform.eulerAngles.y}");
        if ((transform.eulerAngles.y > 315 || transform.eulerAngles.y < 45))
        {
            SymbolRenderer.Symbol = BaseSymbols[0];
            //SymbolRenderer.Symbol.IgnoreRotation = true;
            //SymbolRenderer.SymbolAsset = OneEightySwapSymbol;
        }
        else
        {
            SymbolRenderer.Symbol = BaseSymbols[1];
            //SymbolRenderer.Symbol.IgnoreRotation = false;
        }
        SymbolRenderer.Symbol.IgnoreRotation = false;

    }
}
