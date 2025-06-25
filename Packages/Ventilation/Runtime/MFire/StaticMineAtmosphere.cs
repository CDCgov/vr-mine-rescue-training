using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StaticMineAtmosphere", menuName = "VRMine/Static Mine Atmosphere", order = 1)]
public class StaticMineAtmosphere : ScriptableObject
{
    public MineAtmosphere MineAtmosphere;
    public MineAtmosphere MineAtmosphereVariation;
    public float VariationSpeed = 0.03f;

    public MineAtmosphere GetAtmosphere()
    {
        //compute variation 
        var atmVariation = MineAtmosphereVariation;
        atmVariation.Oxygen *= NoiseMultiplier(0);
        atmVariation.CarbonMonoxide *= NoiseMultiplier(1);
        atmVariation.Methane *= NoiseMultiplier(2);
        atmVariation.HydrogenSulfide *= NoiseMultiplier(3);

        var atmosphere = MineAtmosphere + atmVariation;

        return atmosphere;
    }
    protected float NoiseMultiplier(float y)
    {
        return Mathf.PerlinNoise(Time.time * VariationSpeed, y) * 2.0f - 1.0f;
    }
}
