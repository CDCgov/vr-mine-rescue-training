using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineEquipmentHost : MineElementHostBase, IMineElementHost, ISelectableObject
{
    public MineEquipment MineEquipment;

    public MineElement GetMineElement()
    {
        return MineEquipment;
    }
}