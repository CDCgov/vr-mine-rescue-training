using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ExternalAssetEditModeTests
{
    [Test]
    public void TestMetadataFile()
    {
        ExternalAssetMetadata testData = new ExternalAssetMetadata();

        testData.SourceFolder = "SourceFolder";
        testData.SourceFile = "SourceFile.yaml";
        testData.AssetID = "TestAssetID";
        testData.AssetName = "Test Asset";
        testData.GeometryFilename = "InvalidFilename.gltf";
        testData.MeshColliderName = "MeshColliderObject";
        testData.PhysicalProperties.Mass = 999;
        testData.PhysicalProperties.Density = 0.1f;
        testData.PlacementOptions.EditorLayer = NIOSH_EditorLayers.LayerManager.EditorLayer.Object;
        testData.PlacementOptions.ShowInAssetWindow = true;
        

        testData.MeshMaterialOverrides.Add("MeshName", "MaterialID");
        //testData.CustomMaterialDefinitions.Add("CustomMaterial1", new ExternalAssetMat
        //{
        //    Diffuse = "DiffuseTexture.jpg",
        //    NormalMap = "NormalMap.png",
        //    MaskMap = "MaskMap.png",
        //});

        var audioProperties = new Dictionary<string, string>();
        audioProperties.Add("DefaultVolume", "0.75");

        testData.Components.Add(new ExternalAssetComponent
        {
            ComponentType = ExternalAssetComponentType.Audio,
            ComponentName = "Test Audio",
            ObjectName = "ObjectToAttachTo",
            Filename = "invalid_sound.wav",
            Properties = audioProperties,
        });

        testData.Components.Add(new ExternalAssetComponent
        {
            ComponentType = ExternalAssetComponentType.Light,
            ComponentName = "Test Light",
            ObjectName = "ObjectToAttachTo",
            PrefabID = "InvalidLightPrefabAddressableName",
            Properties = null,
        });

        testData.AssetCategories = LoadableAssetCategories.MineTile | LoadableAssetCategories.Object | LoadableAssetCategories.ScannedEnvironment;

        testData.Tags = new List<string>();
        testData.Tags.Add("Tag1");
        testData.Tags.Add("Tag2");

        var userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folder = Path.Combine(userprofile, "VRMine", "ExternalAssets");
        Directory.CreateDirectory(folder);

        var filename = Path.Combine(folder, "testasset.yaml");
        YAMLMetadata.Save(filename, testData);

        var loadedData = YAMLMetadata.Load<ExternalAssetMetadata>(filename);

        Assert.That(loadedData.AssetID, Is.EqualTo(testData.AssetID));
        Assert.That(loadedData.AssetName, Is.EqualTo(testData.AssetName));
        Assert.That(loadedData.GeometryFilename, Is.EqualTo(testData.GeometryFilename));
        Assert.That(loadedData.PhysicalProperties.Mass, Is.EqualTo(testData.PhysicalProperties.Mass));
        Assert.That(loadedData.PhysicalProperties.Density, Is.EqualTo(testData.PhysicalProperties.Density));
        Assert.That(loadedData.PlacementOptions.EditorLayer, Is.EqualTo(testData.PlacementOptions.EditorLayer));
        Assert.That(loadedData.PlacementOptions.ShowInAssetWindow, Is.EqualTo(testData.PlacementOptions.ShowInAssetWindow));

        Assert.That(loadedData.MeshMaterialOverrides["MeshName"], Is.EqualTo("MaterialID"));
        //Assert.That(loadedData.CustomMaterialDefinitions.ContainsKey("CustomMaterial1"));

        Assert.That(loadedData.SourceFolder, Is.Null);
        Assert.That(loadedData.SourceFile, Is.Null);

        Debug.Log($"Asset Categories: {loadedData.AssetCategories} {loadedData.AssetCategories:X}");
        Assert.That((loadedData.AssetCategories & LoadableAssetCategories.ScannedEnvironment) > 0, Is.True, "Didn't contain ScannedEnvironment category");
        Assert.That((loadedData.AssetCategories & LoadableAssetCategories.Object) > 0, Is.True, "Didn't contain Object category");
        Assert.That((loadedData.AssetCategories & LoadableAssetCategories.Cable) > 0, Is.Not.True, "Incorrectly contained Cable category");

        var fileText = File.ReadAllText(filename);
        Debug.Log(fileText);
    }


    [Test]
    public void TestMaterialMetadataFile()
    {
        var systemManager = SystemManager.GetDefault();

        MaterialMetadata data = new MaterialMetadata()
        {
            MaterialID = "TEST_MAT_ID",
            BaseMaterialID = "TriplanarStoneBanded",
            Textures = new Dictionary<string, MaterialTextureData>(),
            Colors = new Dictionary<string, string>(),
            Settings = new Dictionary<string, float>(),
        };

        //data.Textures.Add("_BaseMap", "Tex1.jpg");
        //data.Textures.Add("_BaseMap2", "Tex2.jpg");
        //data.Textures.Add("_NormalMap", "NormalMap1.png");
        //data.Textures.Add("_NormalMap2", "NormalMap2.png");

        data.Textures.Add("_BaseMap", new MaterialTextureData("Tex1.jpg"));
        data.Textures.Add("_NormalMap", new MaterialTextureData("Tex2.png"));

        data.Colors.Add("_Color", "#0031E0");
        data.Colors.Add("_Color2", "#03E1A8");

        data.Settings.Add("_Tile", 0.5f);

        var folder = systemManager.SystemConfig.MaterialsFolder;
        Directory.CreateDirectory(folder);

        var filename = Path.Combine(folder, "test_mat.yaml");
        YAMLMetadata.Save(filename, data);

        var loadedData = YAMLMetadata.Load<MaterialMetadata>(filename);

        Assert.That(loadedData.MaterialID, Is.EqualTo(data.MaterialID));
    }

}
