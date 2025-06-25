using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Runtime.InteropServices;
using Unity.Collections;
using GLTFast;

public class MaterialManager : SceneManagerBase
{
    public SystemManager SystemManager;

    private Dictionary<string, MaterialMetadata> _metadata;

    private List<Texture2D> _activeTextures = new List<Texture2D>();

    public static MaterialManager GetDefault(GameObject self)
    {
        return Util.GetDefaultManager<MaterialManager>(self, "MaterialManager", true);
    }

    public IEnumerator LoadBuiltInMaterials()
    {
        var locHandle = Addressables.LoadResourceLocationsAsync("BuiltInMaterial", typeof(Material));
        yield return locHandle;

        if (locHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Error loading built in materials");
            yield break;
        }

        foreach (var loc in locHandle.Result)
        {

            var matHandle = Addressables.LoadAssetAsync<Material>(loc.PrimaryKey);
            yield return matHandle;

            if (matHandle.Status != AsyncOperationStatus.Succeeded)
                continue;

            MaterialMetadata data = new MaterialMetadata()
            {
                MaterialID = loc.PrimaryKey,
                LoadedMaterial = matHandle.Result,
            };

            AddMaterialMetadata(data);

            Debug.Log($"MaterialManager: Loaded built in material {data.MaterialID}");
        }

        Addressables.Release(locHandle);
        Debug.Log("MaterialManager: Internal material loading complete");
    }


    public IEnumerator LoadExternalMaterials()
    {
        LoadMetadataFiles();

        foreach (var data in _metadata.Values)
        {
            yield return LoadExternalMaterial(data);
        }

        yield break;
    }

    public void ReloadMaterialSettings()
    {
        if (_metadata == null)
            return;

        foreach (var matData in _metadata.Values)
        {
            ReloadMaterialSettings(matData);
        }
    }

    private void ReloadMaterialSettings(MaterialMetadata data)
    {
        if (data == null || data.SourceFile == null || data.SourceFolder == null)
            return;

        var filename = Path.Combine(data.SourceFolder, data.SourceFile);
        var newData = LoadMetadataFile(filename);
        if (newData == null)
            return;

        data.CopySettingsFrom(newData);
        data.ApplyMaterialSettings();
    }

    private IEnumerator LoadExternalMaterial(MaterialMetadata data)
    {
        if (data.SourceFile == null || data.SourceFolder == null)
            yield break;

        //var mat = Resources.Load<Material>(data.BaseMaterialID);
        var handle = Addressables.LoadAssetAsync<Material>(data.BaseMaterialID.ToUpper());
        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"MaterialManager: Couldn't load base material {data.BaseMaterialID}");
            yield break;
        }

        var baseMat = handle.Result;

        var mat = Instantiate<Material>(baseMat);

        //the first SetTexture sems to not work? (Unity 2022.3.13f1)
        mat.SetTexture("_BaseMap", null);

        if (data.Textures != null)
        {
            foreach (var texData in data.Textures)
            {
                var tex = LoadTexture(data, texData.Value);
                if (tex == null)
                    continue;

                Debug.Log($"{data.MaterialID}: Setting texture {texData.Key}:{texData.Value.Filename}");
                mat.SetTexture(texData.Key, tex);
                //mat.SetTexture(texData.Key, tex);
                //HDMaterial.ValidateMaterial(mat);
            }
        }

        data.LoadedMaterial = mat;
        data.ApplyMaterialSettings();
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    private struct TexARGB
    {
        public byte a;
        public byte r;
        public byte g;
        public byte b;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    private struct TexRGB
    {
        public byte r;
        public byte g;
        public byte b;
    }


    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    private struct TexRG
    {
        public byte r;
        public byte g;
    }

    private string GetMaterialTextureCachePath(string textureFilename)
    {
        var cachePath = Path.Combine(Application.persistentDataPath, "CustomMaterialCache");
        Directory.CreateDirectory(cachePath);

        cachePath = Path.Combine(cachePath, $"{textureFilename}.bin");

        return cachePath;
    }

    private Texture2D LoadTexture(MaterialMetadata data, MaterialTextureData texData)
    {
        Texture2D result = null;
        string textureCachePath = null;
        bool linear = true;

        if (texData.Filename == null)
            return null;

        switch (texData.Format)
        {
            case MaterialTextureFormat.Linear:
            case MaterialTextureFormat.NormalMap:
            case MaterialTextureFormat.NormalMapDX:
                //tex = new Texture2D(4, 4, TextureFormat.ARGB32, true, true);
                linear = true;
                break;

            default:
                //tex = new Texture2D(4, 4, TextureFormat.ARGB32, true, false);
                linear = false;
                break;
        }

        if (SystemManager != null && SystemManager.SystemConfig != null &&
            SystemManager.SystemConfig.UseMaterialTextureCache)
        {
            textureCachePath = GetMaterialTextureCachePath(texData.Filename);

            try
            {                
                if (File.Exists(textureCachePath))
                {
                    //Texture2D cachedTex = new Texture2D(4, 4, TextureFormat.ARGB32, true, true);
                    //if (GltfImport.LoadTextureCache(cachedTex, textureCachePath))

                    var cachedTex = Util.LoadTextureCache(textureCachePath);
                    if (cachedTex != null)
                    {
                        //successfully loaded cached texture
                        return cachedTex;
                    }
                    else
                    {
                        Debug.LogWarning($"Unable to load cached texture file {textureCachePath}");
                        //Destroy(cachedTex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Unable to load cached texture for {texData.Filename}: {ex.Message}");
            }
        }

        try
        {
            Texture2D tex;

            //switch (texData.Format)
            //{
            //    case MaterialTextureFormat.Linear:
            //    case MaterialTextureFormat.NormalMap:
            //    case MaterialTextureFormat.NormalMapDX:
            //        tex = new Texture2D(4, 4, TextureFormat.ARGB32, true, true);
            //        break;

            //    default:
            //        tex = new Texture2D(4, 4, TextureFormat.ARGB32, true, false);
            //        break;
            //}

            tex = new Texture2D(4, 4, TextureFormat.ARGB32, true, linear);

            var texBytes = File.ReadAllBytes(data.GetFullPath(texData.Filename));
            tex.LoadImage(texBytes);

            if (texData.Format == MaterialTextureFormat.NormalMap)
                result = ProcessNormalMap(texData, tex, flipGreen: false);
            else if (texData.Format == MaterialTextureFormat.NormalMapDX)
                result = ProcessNormalMap(texData, tex, flipGreen: true);
            else
                result = ProcessTexture(texData, tex);

            if (textureCachePath != null &&
                SystemManager != null && SystemManager.SystemConfig != null && 
                SystemManager.SystemConfig.UseMaterialTextureCache)
            {
                try
                {
                    Util.SaveTextureCache(result, textureCachePath, linear);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Unable to save to texture cache for {texData.Filename}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"MaterialManager: Couldn't load texture {texData.Filename}: {ex.Message}");
            return null;
        }

        _activeTextures.Add(result);
        return result;
    }

    private Texture2D ProcessTexture(MaterialTextureData texData, Texture2D tex)
    {
        tex.Compress(true);
        tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);

        return tex;
    }

    private Texture2D ProcessNormalMap(MaterialTextureData texData, Texture2D tex, bool flipGreen)
    {
        var tex2 = new Texture2D(tex.width, tex.height, TextureFormat.RG16, true, true);

        if (tex.format == TextureFormat.ARGB32)
            ConvertARGBtoRG(tex, tex2, flipGreen);
        else if (tex.format == TextureFormat.RGB24)
            ConvertRGBtoRG(tex, tex2, flipGreen);
        else
        {
            Debug.LogError($"MaterialManager: {texData.Filename} normal map not in ARGB32 or RGB24 format ({tex.format})");
            Destroy(tex2);
            return tex;
        }        


        //Debug.Log($"TextureCompress: {tex2.format} readable: {tex2.isReadable}");       
        return tex2;
    }

    private void ConvertRGBtoRG(Texture2D tex, Texture2D tex2, bool flipGreen)
    {
        var pixels = tex.GetPixelData<TexRGB>(0);
        int numPixels = pixels.Length;
        var pixels2 = new NativeArray<TexRG>(numPixels, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (int i = 0; i < numPixels; i++)
        {
            var p = pixels[i];
            var p2 = new TexRG();

            p2.r = p.r;
            if (flipGreen)
                p2.g = (byte)(255 - p.g);
            else
                p2.g = p.g;

            pixels2[i] = p2;
        }
        tex2.SetPixelData<TexRG>(pixels2, 0);
        tex2.Apply(updateMipmaps: true, makeNoLongerReadable: false);

        pixels.Dispose();
        pixels2.Dispose();

    }

    private void ConvertARGBtoRG(Texture2D tex, Texture2D tex2, bool flipGreen)
    {
        var pixels = tex.GetPixelData<TexARGB>(0);
        int numPixels = pixels.Length;
        var pixels2 = new NativeArray<TexRG>(numPixels, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (int i = 0; i < numPixels; i++)
        {
            var p = pixels[i];
            var p2 = new TexRG();

            p2.r = p.r;
            if (flipGreen)
                p2.g = (byte)(255 - p.g);
            else
                p2.g = p.g;

            pixels2[i] = p2;
        }
        tex2.SetPixelData<TexRG>(pixels2, 0);
        tex2.Apply(updateMipmaps: true, makeNoLongerReadable: false);

        pixels.Dispose();
        pixels2.Dispose();
    }

    public bool TryFindMaterial(string materialID, out Material mat)
    {
        if (_metadata.TryGetValue(materialID.ToUpper(), out var data) && data.LoadedMaterial != null)
        {
            mat = data.LoadedMaterial;
            return true;
        }

        mat = null;
        return false;
    }

    public IEnumerable<MaterialMetadata> GetAllMaterials()
    {
        if (_metadata == null)
            yield break;

        foreach (var mat in _metadata.Values)
            yield return mat;
    }

    private IEnumerable<string> GetMetadataFiles(string directory = null)
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (directory == null)
            directory = SystemManager.SystemConfig.ExternalAssetsFolder;

        var files = Directory.GetFiles(directory, "*.yaml", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            yield return file;
        }
    }

    public void LoadMetadataFiles()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (_metadata == null)
            _metadata = new Dictionary<string, MaterialMetadata>();

        foreach (var file in GetMetadataFiles(SystemManager.SystemConfig.MaterialsFolder))
        {
            try
            {
                var matData = LoadMetadataFile(file);
                if (matData == null)
                    continue;

                AddMaterialMetadata(matData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading material: {ex.Message}");
            }
        }
    }

    private void AddMaterialMetadata(MaterialMetadata matData)
    {
        _metadata.Add(matData.MaterialID.ToUpper(), matData);
    }

    private MaterialMetadata LoadMetadataFile(string filename)
    {
        try
        {
            var matData = YAMLMetadata.Load<MaterialMetadata>(filename);
            if (matData == null)
                return null;

            matData.SourceFolder = Path.GetDirectoryName(filename);
            matData.SourceFile = Path.GetFileName(filename);
            matData.LoadedMaterial = null;

            if (string.IsNullOrEmpty(matData.MaterialID))
            {
                Debug.LogError($"MaterialManager: {matData.SourceFile} missing MaterialID");
                return null;
            }

            if (string.IsNullOrEmpty(matData.BaseMaterialID))
            {
                Debug.LogError($"MaterialManager: {matData.SourceFile} missing BaseMaterialID");
                return null;
            }

            if (matData.Textures != null)
            {
                foreach (var texData in matData.Textures)
                {
                    var texFile = Path.Combine(matData.SourceFolder, texData.Value.Filename);
                    if (!File.Exists(texFile))
                    {
                        Debug.LogError($"MaterialManager: Missing texture {texData.Value.Filename} for material {matData.SourceFile}");
                        return null;
                    }
                }
            }

            return matData;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading metadata {filename}: {ex.Message}");
        }

        return null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.LeftAlt) && 
            Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
        {
            ReloadMaterialSettings();
            //var placer = Placer.GetDefault();
            //if (placer != null && !placer.IsInputLocked)
            //{
            //    ReloadMaterialSettings();
            //}
        }

        if (Input.GetKeyDown(KeyCode.Y) && Input.GetKey(KeyCode.LeftAlt) &&
            Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
        {
            WireframeMode();
        }

        if (SystemManager != null && Input.GetKeyDown(KeyCode.G) && Input.GetKey(KeyCode.LeftAlt) &&
           Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
        {
            SystemManager.LoadGraphicsConfig();
        }
    }

    private void WireframeMode()
    {
        var matOverrides = GameObject.FindObjectsOfType<ComponentInfo_MaterialOverride>();
        foreach (var matOverride in matOverrides)
        {
            matOverride.MaterialOverrideID = "WIREFRAME";
        }

        var loadOverrides = GameObject.FindObjectsOfType<ComponentInfo_LODOverride>();
        foreach (var lodOverride in loadOverrides)
        {
            lodOverride.LODOverrideID = "LOD1";
        }
    }

    private void Awake()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _metadata = new Dictionary<string, MaterialMetadata>();

        Util.DontDestroyOnLoad(gameObject);
    }

}
