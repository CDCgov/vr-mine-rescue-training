using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct IndexedColorData
{
    public Color Color;
    public string Name;
}

[CreateAssetMenu(fileName = "IndexedColorList", menuName = "VRMine/IndexedColorList", order = 0)]
public class IndexedColorList : ScriptableObject
{
    public List<IndexedColorData> IndexedColors;

    public IndexedColorData DefaultColorData;
    

    void Reset()
    {
        DefaultColorData = new IndexedColorData
        {
            Color = Color.magenta,
            Name = "Error",
        };
    }

    public IndexedColorData GetColorData(int index)
    {
        if (IndexedColors == null || index < 0 || index >= IndexedColors.Count)
            return DefaultColorData;

        return IndexedColors[index];
    }

    public Color GetColor(int index)
    {
        var data = GetColorData(index);
        return data.Color;
    }

    public Color GetRandomColor()
    {
        if (IndexedColors == null || IndexedColors.Count <= 0)
            return Color.magenta;

        int index = Random.Range(0, IndexedColors.Count - 1);

        return GetColor(index);
    }
}
