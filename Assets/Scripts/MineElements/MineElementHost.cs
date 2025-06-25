using UnityEngine;
using System.Collections;
/// <summary>
/// Monobehavior derived class that passes messages to an associated MineElement. This can be attached to GameObjects in Unity.
/// </summary>
public abstract class MineElementHost : MineElementHostBase
{
    public abstract MineElement GetMineElement();
}
