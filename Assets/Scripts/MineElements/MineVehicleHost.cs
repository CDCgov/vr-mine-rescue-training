using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class MineVehicleHost : MineElementHostBase, IMineElementHost, ISelectableObject
{
    public MineVehicle MineVehicle;

    public MineElement GetMineElement()
    {
        return MineVehicle;
    }	
}