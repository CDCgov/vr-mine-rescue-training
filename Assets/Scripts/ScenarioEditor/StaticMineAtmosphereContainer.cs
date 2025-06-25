using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StaticMineAtmosphereContainer",  menuName = "ScriptableObject/ScenarioEditor/ScenarioEditorStaticMineAtmosphereContainer", order = 3)]

public class StaticMineAtmosphereContainer : ScriptableObject
{
    public StaticMineAtmosphere[] staticMineAtmospheres;
}
