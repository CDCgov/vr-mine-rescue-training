using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

[CreateAssetMenu(fileName = "MineMapTextSymbols", menuName = "VRMine/MineTextSymbols", order = 0)]
public class MineMapTextSymbols : ScriptableObject
{
    public MineMapTextSymbol[] collection;

    public MineMapTextSymbol GetCollectionByString(string name)
    {
        return collection.Where(symbol => symbol.name == name).First();
    }
}
