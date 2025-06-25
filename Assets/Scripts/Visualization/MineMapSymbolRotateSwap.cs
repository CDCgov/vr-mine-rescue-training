using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineMapSymbolRotateSwap : MonoBehaviour
{
    public MineMapSymbolRenderer SymbolRenderer;

    public MineMapSymbol BaseSymbol;
    public MineMapSymbol OneEightySwapSymbol;

    public MineMapSymbol NinetySymbol;
    public MineMapSymbol TwoSeventySymbol;
    private void Awake()
    {
        if(SymbolRenderer == null)
        {
            SymbolRenderer = GetComponent<MineMapSymbolRenderer>();
        }
        Debug.Log($"Changing symbol renderer based on rotation of: {transform.eulerAngles.y}");
        //if((transform.eulerAngles.y > 45 && transform.eulerAngles.y < 135) || (transform.eulerAngles.y < -135 && transform.eulerAngles.y > -270))
        //{
        //    SymbolRenderer.Symbol = OneEightySwapSymbol;
        //    //SymbolRenderer.Symbol.IgnoreRotation = true;
        //    //SymbolRenderer.SymbolAsset = OneEightySwapSymbol;
        //}
        //else
        //{
        //    SymbolRenderer.Symbol = BaseSymbol;
        //    //SymbolRenderer.Symbol.IgnoreRotation = false;
        //}
        //SymbolRenderer.Symbol.IgnoreRotation = false;   
        

        if(transform.eulerAngles.y >= 225 && transform.eulerAngles.y < 315)
        {
            SymbolRenderer.Symbol = TwoSeventySymbol;
        }
        else if(transform.eulerAngles.y >= 45 && transform.eulerAngles.y < 135)
        {
            SymbolRenderer.Symbol = NinetySymbol;
        }
        else if(transform.eulerAngles.y >= 135 && transform.eulerAngles.y < 225)
        {
            SymbolRenderer.Symbol = OneEightySwapSymbol;
        }
        else
        {
            SymbolRenderer.Symbol = BaseSymbol;
        }
    }
}
