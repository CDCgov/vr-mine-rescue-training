using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ComponentModel;
using YamlDotNet.Serialization;
using System;
using UnityEngine.Rendering.HighDefinition;

public enum MaterialTextureFormat
{
    Unknown = 0,
    Linear = 1,
    sRGB = 2,
    NormalMap = 3,
    NormalMapDX = 4,
}

[System.Serializable]
public struct MaterialTextureData
{
    [Description("Texture filename")]
    public string Filename;
    [Description("Texture format (Linear, sRGB, NormalMap, NormalMapDX)")]
    public MaterialTextureFormat Format;

    //optional additional properties
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string MaskMap;

    public MaterialTextureData(string filename)
    {
        Filename = filename;
        Format = MaterialTextureFormat.sRGB;
        MaskMap = null;
    }
}

[System.Serializable]
public class MaterialMetadata
{
    [YamlIgnore]
    public string SourceFolder;
    [YamlIgnore]
    public string SourceFile;

    [Description("ID of the material")]
    public string MaterialID;
    [Description("ID of the material used as a base")]
    public string BaseMaterialID;

    //public Dictionary<string, MaterialTextureData> TextureDefinitions;

    public Dictionary<string, MaterialTextureData> Textures;
    public Dictionary<string, string> Colors;
    public Dictionary<string, float> Settings;

    [YamlIgnore]
    public Material LoadedMaterial;

    public string GetFullPath(string filename)
    {
        if (SourceFolder == null)
            return filename;

        return Path.Combine(SourceFolder, filename);
    }

    public void CopySettingsFrom(MaterialMetadata data)
    {
        Settings = data.Settings;
        Colors = data.Colors;
    }

    public void ApplyMaterialSettings()
    {
        if (LoadedMaterial == null)
            return;

        if (Colors != null)
        {
            foreach (var colorData in Colors)
            {
                if (!ColorUtility.TryParseHtmlString(colorData.Value, out var color))
                {
                    Debug.Log($"MaterialManager: Couldn't parse color for {MaterialID}: {colorData.Value}");
                    continue;
                }

                LoadedMaterial.SetColor(colorData.Key, color);
            }
        }

        if (Settings != null)
        {
            foreach (var settingData in Settings)
            {
                LoadedMaterial.SetFloat(settingData.Key, settingData.Value);
            }
        }

        HDMaterial.ValidateMaterial(LoadedMaterial);
    }
}
