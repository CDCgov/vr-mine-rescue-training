using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MineAtmosphere
{
	public float Oxygen; //percent
	public float CarbonMonoxide; //ppm/100
	public float Methane; //percent
	public float HydrogenSulfide; //ppm/100

    public float OxygenStrength;
    public float CarbonMonoxideStrength;
    public float MethaneStrength;
    public float HydrogenSulfideStrength;

    //public Color DefaultColor;

    public string OxygenText
    {
        get => (Oxygen * 100).ToString("F1");
    }

    public string CarbonMonoxideText
    {
        get => Mathf.Round((CarbonMonoxide * 1000000.0f)).ToString("F0");
    }

    public string MethaneText
    {
        get => (Methane * 100).ToString("F1");
    }

    public string HydrogenSulfideText
    {
        get => (Mathf.Round(HydrogenSulfide * 1000000.0f)).ToString("F0");
    }

    public static MineAtmosphere NormalAtmosphere
    {
        get
        {
            return new MineAtmosphere(0.21f, 0, 0, 0);
        }
    }

    public MineAtmosphere(float oxygen, float carbonMonoxide, float methane, float hydrogenSulfide)
    {
        Oxygen = oxygen;
        CarbonMonoxide = carbonMonoxide;
        Methane = methane;
        HydrogenSulfide = hydrogenSulfide;

        OxygenStrength = 1.0f;
        CarbonMonoxideStrength = 1.0f;
        MethaneStrength = 1.0f;
        HydrogenSulfideStrength = 1.0f;

        //DefaultColor = new Color(0, 1, 0, 0.13f);
    }

    public void SetStrength(float strength)
    {
        OxygenStrength = strength;
        CarbonMonoxideStrength = strength;
        MethaneStrength = strength;
        HydrogenSulfideStrength = strength;
    }

    public void ScaleStrength(float scale)
    {
        OxygenStrength *= scale;
        CarbonMonoxideStrength *= scale;
        MethaneStrength *= scale;
        HydrogenSulfideStrength *= scale;
    }

    public static MineAtmosphere  operator +(MineAtmosphere a, MineAtmosphere b)
    {
        a.Oxygen += b.Oxygen;
        a.CarbonMonoxide += b.CarbonMonoxide;
        a.Methane += b.Methane;
        a.HydrogenSulfide += b.HydrogenSulfide;

        a.OxygenStrength = Mathf.Max(a.OxygenStrength, b.OxygenStrength);
        a.CarbonMonoxideStrength = Mathf.Max(a.CarbonMonoxideStrength, b.CarbonMonoxideStrength);
        a.MethaneStrength = Mathf.Max(a.MethaneStrength, b.MethaneStrength);
        a.HydrogenSulfideStrength = Mathf.Max(a.HydrogenSulfideStrength, b.HydrogenSulfideStrength);

        return a;
    }

    public static MineAtmosphere operator *(MineAtmosphere a, float scale)
    {
        a.Oxygen = a.Oxygen * scale;
        a.CarbonMonoxide = a.CarbonMonoxide * scale;
        a.Methane = a.Methane * scale;
        a.HydrogenSulfide = a.HydrogenSulfide * scale;

        return a;
    }

    /// <summary>
    /// Combine gas values from b based on their strength value
    /// may result in a non-normalized atmosphere with
    /// strength values exceeding 1.0f
    /// </summary>
    /// <param name="b"></param>
    public void Combine(MineAtmosphere b)
    {
        CombineGas(ref Oxygen, ref OxygenStrength, ref b.Oxygen, ref b.OxygenStrength);
        CombineGas(ref CarbonMonoxide, ref CarbonMonoxideStrength, ref b.CarbonMonoxide, ref b.CarbonMonoxideStrength);
        CombineGas(ref Methane, ref MethaneStrength, ref b.Methane, ref b.MethaneStrength);
        CombineGas(ref HydrogenSulfide, ref HydrogenSulfideStrength, ref b.HydrogenSulfide, ref b.HydrogenSulfideStrength);
    }

    private void CombineGas(ref float source, ref float sourceStrength, ref float addedConc, ref float addedStrength)
    {
        source += addedConc * addedStrength;
        sourceStrength += addedStrength;
    }


    /// <summary>
    /// Normalize gas values to a strength of one, 
    /// using defaultAtm for values that have less than 1.0 strength
    /// </summary>
    /// <param name="defaultAtm"></param>
    public void Normalize(MineAtmosphere defaultAtm)
    {
        NormalizeGas(ref Oxygen, ref OxygenStrength, defaultAtm.Oxygen);
        NormalizeGas(ref CarbonMonoxide, ref CarbonMonoxideStrength, defaultAtm.CarbonMonoxide);
        NormalizeGas(ref Methane, ref MethaneStrength, defaultAtm.Methane);
        NormalizeGas(ref HydrogenSulfide, ref HydrogenSulfideStrength, defaultAtm.HydrogenSulfide);
    }

    private void NormalizeGas(ref float conc, ref float strength, float defaultConc)
    {
        if (strength < 1.0f)
        {
            float defaultStrength = 1.0f - strength;
            conc = conc + (defaultConc * defaultStrength);
        }
        else
        {
            conc /= strength;
        }

        strength = 1.0f;

    }

    public int GetClampedCO_PPM(int max)
    {
        int co = (int)(Mathf.Round(CarbonMonoxide * 1000000.0f));
        if (co > max)
            co = max;

        return co;
    }

    public override string ToString()
    {
        return $"O2: {OxygenText} CO: {CarbonMonoxideText} M: {MethaneText} H2S: {HydrogenSulfideText}";
    }
}
