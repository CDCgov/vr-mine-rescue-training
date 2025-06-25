using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Google.Protobuf;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public class MineMapSymbolManager : MonoBehaviour
{
    public static MineMapSymbolManager GetDefault(GameObject self)
    {
        MineMapSymbolManager manager = null;
        //for this case prefer the named manager
        var obj = GameObject.Find("MineMapSymbolManager");
        if (obj != null && obj.TryGetComponent<MineMapSymbolManager>(out manager))
        {
            return manager;
        }

        manager = self.GetDefaultManager<MineMapSymbolManager>("MineMapSymbolManager");
        manager.tag = "Manager";

        return manager;
    }
    //{
    //    var obj = GameObject.Find("MineMapSymbolManager");
    //    if (obj == null)
    //    {
    //        obj = new GameObject("MineMapSymbolManager");
    //        obj.tag = "Manager";
    //    }
    //    var manager = obj.GetComponent<MineMapSymbolManager>();
    //    if (manager == null)
    //        manager = obj.AddComponent<MineMapSymbolManager>();

    //    return manager;
    //}

    public event System.Action<MineMapSymbol> SymbolAdded;
    public event System.Action<MineMapSymbol> SymbolRemoved;
    public event System.Action<MineMapSymbol> SymbolColorChanged;

    public bool IsMapBoardSymbolManager = false;
    public bool IsVRDebugSymbolManager = false;

    public List<MineMapSymbol> ActiveSymbols = new List<MineMapSymbol>();
    private Dictionary<long, MineMapSymbol> _symbolMap = new Dictionary<long, MineMapSymbol>();

    protected static long _symbolIDCounter = 0;

    public void Start()
    {
        //only make the default symbol manager not destroyed on load
        if (gameObject.name == "MineMapSymbolManager")
            Util.DontDestroyOnLoad(gameObject);

        if (IsVRDebugSymbolManager)
        {
            var systemManager = SystemManager.GetDefault();
            if (systemManager != null && systemManager.SystemConfig != null && !systemManager.SystemConfig.ShowAllMapSymbolsInDebugMap)
            {
                IsMapBoardSymbolManager = true;
            }
        }
    }

    public void RemoveAllSymbols()
    {
        MineMapSymbol[] symbols = new MineMapSymbol[_symbolMap.Values.Count];
        _symbolMap.Values.CopyTo(symbols, 0);

        foreach (var symbol in symbols)
        {
            RemoveSymbol(symbol);
        }
    }

    public void RemoveAllSymbolsIncludingScene()
    {
        if (ActiveSymbols == null)
            return;
        
        for (int i = ActiveSymbols.Count - 1; i >= 0; i--) 
        {
            var symbol = ActiveSymbols[i];
            RemoveSymbol(symbol);
        }
    }

    public void AddSceneSymbols(bool instantiateNetworkedSymbols)
    {
        MineMapSymbolRenderer[] startSymbols = GameObject.FindObjectsOfType<MineMapSymbolRenderer>();
        foreach (MineMapSymbolRenderer sym in startSymbols)
        {
            if (sym.ShowOnMapMan)
            {
                if (sym.SymbolAsset == null || sym.SymbolAsset.AssetGUID == null || sym.SymbolAsset.AssetGUID.Length <= 0)
                {
                    //add local symbol
                    var symbol = Instantiate<MineMapSymbol>(sym.Symbol);
                    symbol.WorldPosition = sym.transform.position;
                    symbol.WorldRotation = sym.transform.rotation;
                    symbol.Color = sym.Color;
                    symbol.DoNotDelete = sym.DoNotDelete;
                    symbol.SymbolID = -1;
                    symbol.FontSize = sym.FontSize;
                    if (!string.IsNullOrEmpty(sym.SymbolText))
                    {
                        symbol.SymbolText = sym.SymbolText;
                    }

                    //VectorMineMap.ActiveSymbols.Add(symbol);
                    AddSymbol(symbol);
                }
                else if (instantiateNetworkedSymbols)
                {
                    //create networked symbol
                    InstantiateSymbol(sym.SymbolAsset.AssetGUID, sym.transform.position, sym.transform.rotation);
                }
            }
        }
    }

    //return the full serialized state of the symbol manager
    public VRNSymbolManagerState GetSerializedState()
    {
        VRNSymbolManagerState state = new VRNSymbolManagerState();

        foreach (var symbol in ActiveSymbols)
        {
            var data = symbol.GetSymbolData();
            state.Symbols.Add(data);
        }

        return state;
    }

    public void LoadFromSerializedState(VRNSymbolManagerState state)
    {
        if (state == null || state.Symbols == null)
            return;

        HashSet<long> activeSymbols = new HashSet<long>();

        foreach (var data in state.Symbols)
        {
            if (data.Addressable == null || data.Addressable.Length <= 0)
                continue;

            if (!_symbolMap.ContainsKey(data.SymbolID))
            {
                InstantiateSymbol(data);
            }

            UpdateSymbol(data);

            activeSymbols.Add(data.SymbolID);
        }

        List<MineMapSymbol> removeList = new List<MineMapSymbol>();

        foreach (var symbol in _symbolMap.Values)
        {
            if (symbol.SymbolID < 0)
                continue; //ignore locally added non-addressable symbols for now

            if (!activeSymbols.Contains(symbol.SymbolID))
            {
                //todo: make this more efficient  
                removeList.Add(symbol);
            }
        }

        foreach (var symbol in removeList)
        {
            //Debug.Log($"SymbolManager: Removing {symbol.name} from {state.SymbolManagerName}");
            RemoveSymbol(symbol);
        }

        Debug.Log($"SymbolManager: Loaded serialized mine map, {activeSymbols.Count} symbols active, {ActiveSymbols.Count} symbols in list");
    }

    public void InstantiateSymbol(VRNSymbolData data)
    {
        if (data.SymbolClass == "TextSymbol")
        {
            InstantiateTextSymbol(data.Addressable, data.WorldPosition.ToVector3(),
                data.WorldRotation.ToQuaternion(), data.SymbolText, data.SymbolID);
        }
        else
        {
            InstantiateSymbol(data.Addressable, data.WorldPosition.ToVector3(),
                data.WorldRotation.ToQuaternion(), data.SymbolText, data.SymbolID);
        }
    }

    public async Task<MineMapSymbol> InstantiateSymbolAsync(string addressable, Vector3 pos, Quaternion rot, string text = null, long id = -1)
    {
        var handle = Addressables.LoadAssetAsync<MineMapSymbol>(addressable);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return InstantiateSymbol(handle.Result, addressable, pos, rot, text, id);
        }
        else
        {
            Debug.LogError($"Couldn't instantiate symbol address: {addressable}");
            return null;
        }
    }

    public void InstantiateSymbol(string addressable, Vector3 pos, Quaternion rot, string text = null, long id = -1, 
        bool overrideRotation = false, System.Action<MineMapSymbol> callback = null)
    {
        var handle = Addressables.LoadAssetAsync<MineMapSymbol>(addressable);
        handle.Completed += (h) =>
        {
            var symbol = InstantiateSymbol(h.Result, addressable, pos, rot, text, id, overrideRotation);
            callback?.Invoke(symbol);
        };
    }

    private MineMapSymbol InstantiateSymbol(MineMapSymbol symbolPrefab, string addressableKey, Vector3 pos, Quaternion rot, string text = null, long id = -1, bool overrideRotation = false)
    {
        var symbol = Instantiate<MineMapSymbol>(symbolPrefab);
        //if (isGasReading)
        //{
        //    symbol.MineSymbolType = MineMapSymbol.SymbolType.GasCheck;
        //}
        symbol.WorldPosition = pos;
        symbol.WorldRotation = rot;
        if (text != null)
        {
            symbol.SymbolText = text;
        }
        symbol.SymbolID = id;
        symbol.AddressableKey = addressableKey;

        if (overrideRotation)
        {
            symbol.IgnoreRotation = false;
        }

        AddSymbol(symbol);
        return symbol;
    }

    //public async MineMapSymbol InstantiateSymbolAsync(string addressable, Vector3 pos, Quaternion rot, string text = null, long id = -1)
    //{
    //	var handle = await Addressables.LoadAssetAsync<MineMapSymbol>(addressable).Task.GetAwaiter();

    //	handle.Completed += (h) =>
    //	{
    //		var symbol = Instantiate<MineMapSymbol>(h.Result);
    //		symbol.WorldPosition = pos;
    //		symbol.WorldRotation = rot;
    //		symbol.SymbolText = text;
    //		symbol.SymbolID = id;
    //		symbol.AddressableKey = addressable;

    //		AddSymbol(symbol);
    //	};
    //}

    public void InstantiateTextSymbol(string addressable, Vector3 pos, Quaternion rot, string value, long id = -1)
    {
        var handle = Addressables.LoadAssetAsync<MineMapTextSymbol>(addressable);
        handle.Completed += (h) =>
        {
            var symbol = Instantiate<MineMapTextSymbol>(h.Result);
            symbol.WorldPosition = pos;
            symbol.WorldRotation = rot;
            symbol.SymbolText = value;
            symbol.SymbolID = id;
            symbol.AddressableKey = addressable;
            symbol.DoNotDelete = false;

            AddSymbol(symbol);
        };
    }

    public void InstantiateTextAnnotation(string addressable, Vector3 pos, Quaternion rot, string value, MineMapSymbol symbolParent,long id = -1)
    {
        var handle = Addressables.LoadAssetAsync<MineMapTextSymbol>(addressable);
        handle.Completed += (h) =>
        {
            var symbol = Instantiate<MineMapTextSymbol>(h.Result);
            symbol.WorldPosition = pos;
            symbol.WorldRotation = rot;
            symbol.SymbolText = value;
            symbol.SymbolID = id;
            symbol.AddressableKey = addressable;
            symbol.DoNotDelete = false;
            symbol.IsAnnotation = true;
            //symbol.IsChild = true;
            symbol.ParentSymbol = symbolParent;
            AddSymbol(symbol);

            symbol.RefreshText();
            symbolParent.ChildSymbol = symbol;
        };
    }

    public bool GetNearestSymbol(Vector3 worldPos, out MineMapSymbol symbol, out float dist)
    {
        dist = float.MaxValue;
        symbol = null;

        foreach (var sym in ActiveSymbols)
        {
            //if (sym.ParentSymbol != null)
            //{
            //    continue;
            //}
            var d = (sym.WorldPosition - worldPos).magnitude;
            if (d < dist)
            {
                symbol = sym;
                dist = d;
            }
        }

        if (symbol != null)
            return true;
        else
            return false;
    }

    public List<MineMapSymbol> GetAllSymbolsWithinRange(Vector3 worldPos, float range, List<MineMapSymbol> cachedSymbols,bool includeDoNotDelete = true)
    {
        //List<MineMapSymbol> symbols = new List<MineMapSymbol>();
        cachedSymbols.Clear();
        foreach (var sym in ActiveSymbols)
        {
            //if (sym.ParentSymbol != null)
            //{
            //    continue;
            //}
            var d = (sym.WorldPosition - worldPos).magnitude;
            if (d < range)
            {
                if (sym.DoNotDelete && includeDoNotDelete)
                {
                    cachedSymbols.Add(sym);
                }
                else if (!sym.DoNotDelete)
                {
                    cachedSymbols.Add(sym);
                }
            }
        }
        if(cachedSymbols.Count > 0)
        {
            cachedSymbols.Sort((x, y) => Vector3.Distance(worldPos, x.WorldPosition).CompareTo(Vector3.Distance(worldPos, y.WorldPosition)));
        }
        return cachedSymbols;
    }

    public void AddSymbol(MineMapSymbol symbol)
    {
        if (symbol.AddressableKey == null || symbol.AddressableKey.Length <= 0)
        {
            //local symbol
            symbol.SymbolID = -1;
            ActiveSymbols.Add(symbol);
        }
        else
        {
            if (symbol.SymbolID <= 0)
            {
                _symbolIDCounter++;
                symbol.SymbolID = _symbolIDCounter;
            }

            ActiveSymbols.Add(symbol);
            _symbolMap[symbol.SymbolID] = symbol;
        }

        SymbolAdded?.Invoke(symbol);
    }

    public void RemoveSymbol(MineMapSymbol symbol)
    {
        //if(symbol.ChildSymbol != null)
        //{
        //    RemoveSymbol(symbol.ChildSymbol);
        //}        

        if (symbol == null)
            return;

        if (ActiveSymbols != null)
            ActiveSymbols.Remove(symbol);

        if (_symbolMap != null)
            _symbolMap.Remove(symbol.SymbolID);

        SymbolRemoved?.Invoke(symbol);

        Destroy(symbol);
    }

    public void UpdateSymbol(VRNSymbolData data)
    {
        MineMapSymbol symbol;
        if (_symbolMap.TryGetValue(data.SymbolID, out symbol))
        {
            var newColor = new Color(data.Color.R, data.Color.G, data.Color.B);
            bool colorChanged = newColor != symbol.Color;

            symbol.WorldPosition = data.WorldPosition.ToVector3();
            symbol.WorldRotation = data.WorldRotation.ToQuaternion();
            symbol.Color = newColor;
            symbol.Size = new Vector2(data.Size.X, data.Size.Y);

            if (colorChanged)
                UpdateSymbolColor(symbol);
        }
    }

    public void UpdateSymbolColor(MineMapSymbol symbol)
    {
        SymbolColorChanged?.Invoke(symbol);
    }

}
