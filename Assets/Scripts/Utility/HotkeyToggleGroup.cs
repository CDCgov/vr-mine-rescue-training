using System;
using UnityEngine;

[Serializable]
public class HotkeyToggleGroup
{
    public KeyCode KeyCode;
    public bool ModAlt;
    public bool ModCtrl;
    public bool ModShift;
    public GameObject[] GameObjects;
}