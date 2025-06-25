using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScenarioEditorResizable
{
    public Vector3 Size { get; set; }
    public Vector3 LocalCenter { get; }

    public void SetSize(Vector3 size, Vector3 center);
}
