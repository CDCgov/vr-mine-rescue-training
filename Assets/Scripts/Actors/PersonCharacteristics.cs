using UnityEngine;
using System.Collections;

public class PersonCharacteristics{
    public float Height;
    public float Weight;
    public float Age;
    public float BaseSpeed;
    public float JumpHeight;
    public string Name;

    public PersonCharacteristics()
    {
        Height = 1.8f;
        Weight = 80;
        Age = 30;
        BaseSpeed = 1.4f;
        JumpHeight = 0.5f;
        Name = null;
    }
}
