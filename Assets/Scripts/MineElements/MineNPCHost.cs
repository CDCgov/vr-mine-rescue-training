using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineNPCHost : MineElementHostBase, IMineElementHost, ISelectableObject
{
    public MineNPC MineNPC;
    public MineElement GetMineElement()
    {
        return MineNPC;
    }
}
