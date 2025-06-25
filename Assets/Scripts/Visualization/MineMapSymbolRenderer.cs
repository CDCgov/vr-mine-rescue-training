using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets; 

public class MineMapSymbolRenderer : MonoBehaviour
{
    public MineMapSymbolManager MineMapSymbolManager;

    public MineMapSymbol Symbol;
    public AssetReference SymbolAsset;
    public Color Color = Color.white;
    public MineSegment MineSegment;
    public bool ShowOnMapMan = false;
    public bool DoNotDelete = false;
    public bool ConstantUpdate = true;
    public float FontSize = -1;
    public string SymbolText = null;
    public ComponentInfo_MineSegment ComponentInfo_MineSegment;

    MineMapSymbol _symbolInstance;

    // Start is called before the first frame update
    void Start()
    {
        if (ScenarioSaveLoad.IsScenarioEditor)
            return;

        if (MineMapSymbolManager == null)
            MineMapSymbolManager = MineMapSymbolManager.GetDefault(gameObject);

        if (Symbol == null)
            return;

        if (MineSegment == null)
            TryGetComponent<MineSegment>(out MineSegment);

        if (MineSegment != null && !MineSegment.IncludeInMap)
        {
            this.enabled = false;
            return;
        }

        if (!ShowOnMapMan && MineMapSymbolManager.IsMapBoardSymbolManager)
            return;

        if (ComponentInfo_MineSegment != null && !ComponentInfo_MineSegment.IsMapped)//Do not show symbol if there is an associated mine segment info that says this is not mapped
        {
            return;
        }

        var symbol = Instantiate<MineMapSymbol>(Symbol);
        symbol.WorldPosition = transform.position;
        symbol.WorldRotation = transform.rotation;
        symbol.ShowOnMapMan = ShowOnMapMan;        
        symbol.Color = Color;
        symbol.DoNotDelete = DoNotDelete;
        symbol.FontSize = FontSize;
        if (!string.IsNullOrEmpty(SymbolText))
        {
            symbol.SymbolText = SymbolText;
        }

        //string test = symbol.SpanEntry ? "Yes" : "No";
        //Debug.Log($"{symbol.AddressableKey} Symbol instantiated! Should it span? {test}");
        //VectorMineMap.ActiveSymbols.Add(symbol);
        MineMapSymbolManager.AddSymbol(symbol);
        _symbolInstance = symbol;
    }

    public void UpdateColor(Color color)
    {
        Color = color;
        if (_symbolInstance != null)
        {
            _symbolInstance.Color = color;
            MineMapSymbolManager.UpdateSymbolColor(_symbolInstance);
        }
    }

    private void OnDestroy()
    {
        if (_symbolInstance != null)
            MineMapSymbolManager.RemoveSymbol(_symbolInstance);
    }

    // Update is called once per frame
    void Update()
    {
        if (_symbolInstance != null && ConstantUpdate)
        {
            _symbolInstance.WorldPosition = transform.position;
            _symbolInstance.WorldRotation = transform.rotation;
            _symbolInstance.Color = Color;
        }
    }
}
