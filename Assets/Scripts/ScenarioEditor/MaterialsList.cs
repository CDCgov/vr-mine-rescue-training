using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/MaterialsList")]
public class MaterialsList : ScriptableObject
{
    [SerializeField] private List<Material> rockDustMaterials = new List<Material>();

    public List<Material> GetAllMaterials()
    {
        return rockDustMaterials;
    }

    public void SetRockdust(float value)
    {
        foreach (var mat in rockDustMaterials)
        {
            mat.SetFloat("_Rockdust", value);
        }
    }
}
