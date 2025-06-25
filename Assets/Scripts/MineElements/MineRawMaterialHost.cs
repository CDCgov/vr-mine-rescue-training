using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MineRawMaterialHost : MineElementHostBase, IMineElementHost, ISelectableObject
{
    public MineRawMaterial MineRawMaterial;

    public MineElement GetMineElement()
    {
        return MineRawMaterial;	
    }
    
}