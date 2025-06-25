using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public enum MineMaterialTypes
{
    Coal,
    WoodPillars,
    BratticeMaterial,
    CribBlocks,
}

[System.Serializable]
public class MineRawMaterial : MineElement
{
    public MineMaterialTypes MaterialType;
    public float MaterialAmount = 1.0f;

    public override void GetObjectInfo(StringBuilder sb)
    {
        sb.AppendFormat("Material Type: {0}\n", System.Enum.GetName(typeof(MineMaterialTypes), MaterialType));
        sb.AppendFormat("Material Amt : {0}\n", MaterialAmount);

        base.GetObjectInfo(sb);
    }

}