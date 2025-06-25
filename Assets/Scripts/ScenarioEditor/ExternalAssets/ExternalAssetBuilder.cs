using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Threading.Tasks;
using GLTFast;
using System.IO;
using System.Linq;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;
using Unity.Collections;
using GLTFast.Schema;
using GLTFast.Logging;
using GLTFast.Loading;
using System.Drawing.Text;

public static class ExternalAssetBuilder
{
    private static Dictionary<string, Sprite> _iconCache;

    /// <summary>
    /// Constructs a game object using the passed in metadata to import an external model and apply settings.
    /// </summary>
    /// <param name="metadata">Data for the object being imported.</param>
    /// <returns>A game object made form an external model file.</returns>
    public static async Task<GameObject> BuildGeometryObject(ExternalAssetMetadata metadata, LoadableAssetManager loadableAssetManager)
    {
        // Create base object to build on
        GameObject geometryObj = new GameObject(metadata.AssetID);
        var geomObjInfo = geometryObj.AddComponent<GeometryObjectInfo>();

        bool success = false;

        try
        {
            // Set up transform
            success = await ImportGeometry(metadata, geometryObj);

            if (success)
            {
                // Set up colliders and LODs
                SetupLODsAndColliders(metadata, geometryObj);

                SetupColliders(geomObjInfo, metadata, geometryObj, loadableAssetManager);

                // >>>>>>>>Set up additional components<<<<<<<<<
                // Set up lights
                SetupLights(geometryObj);
            }
        }
        catch (System.Exception ex)
        {
            success = false;
            Debug.LogError($"Error importing external asset {metadata.AssetID}: {ex.Message}");
        }

        // Error check to ensure everything loaded okay
        if (success)
        {
            UnityEngine.Object.DontDestroyOnLoad(geometryObj);

            // Hide the completed object to keep in background after loading
            geometryObj.SetActive(false);

            return geometryObj;
        }
        else
        {
            // Destroy the failed object
            UnityEngine.Object.Destroy(geometryObj);

            Debug.LogError("ERROR! Failed to build geometry object (" + metadata.AssetName + ")");

            return null;
        }
    }

    public static string GetIconCachePath(string iconFilename)
    {
        var cachePath = Path.Combine(Application.persistentDataPath, "IconCache");
        Directory.CreateDirectory(cachePath);

        cachePath = Path.Combine(cachePath, $"{iconFilename}.bin");

        return cachePath;
    }

    public static async Task<Sprite> LoadAssetIcon(ExternalAssetMetadata metadata, LoadableAssetManager loadableAssetManager)
    {
        if (loadableAssetManager == null)
            return null;

        if (metadata == null || string.IsNullOrEmpty(metadata.IconFilename) || string.IsNullOrEmpty(metadata.SourceFolder))
            return loadableAssetManager.DefaultIcon;

        if (_iconCache == null)
            _iconCache = new Dictionary<string, Sprite>();

        if (_iconCache.TryGetValue(metadata.IconFilename, out var cachedSprite))
        {
            Debug.Log($"Using cached icon {metadata.IconFilename}");
            return cachedSprite;
        }

        var iconFilename = Path.Combine(metadata.SourceFolder, metadata.IconFilename);
        var cacheFilename = GetIconCachePath(metadata.IconFilename);

        if (!File.Exists(iconFilename))
            return loadableAssetManager.DefaultIcon;

        try
        {

            var tex = new Texture2D(4, 4, TextureFormat.ARGB32, true, false);
            var texBytes = File.ReadAllBytes(iconFilename);
            tex.LoadImage(texBytes);

            if (tex.width != 512 || tex.height != 512)
            {
                //Debug.LogWarning($"Resizing icon texture for {metadata.AssetID}");
                //tex.Reinitialize(512, 512);
            }

            //var sprite = Sprite.Create(tex, new Rect(0, 0, 512, 512), new Vector2(256, 256));
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width/2, tex.height/2));
            _iconCache.Add(metadata.IconFilename, sprite);

            return sprite;

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading icon for {metadata.AssetID} : {ex.Message} {ex.StackTrace}");
            return loadableAssetManager.DefaultIcon;
        }
    }

    private class GLTFInstantiator : IInstantiator
    {
        public void AddAnimation(AnimationClip[] animationClips)
        {
        }

        public void AddCamera(uint nodeIndex, uint cameraIndex)
        {
        }

        public void AddLightPunctual(uint nodeIndex, uint lightIndex)
        {
        }

        public void AddPrimitive(uint nodeIndex, string meshName, MeshResult meshResult, uint[] joints = null, uint? rootJoint = null, float[] morphTargetWeights = null, int primitiveNumeration = 0)
        {
        }

        public void AddPrimitiveInstanced(uint nodeIndex, string meshName, MeshResult meshResult, uint instanceCount, NativeArray<Vector3>? positions, NativeArray<Quaternion>? rotations, NativeArray<Vector3>? scales, int primitiveNumeration = 0)
        {
        }

        public void BeginScene(string name, uint[] rootNodeIndices)
        {
        }

        public void CreateNode(uint nodeIndex, uint? parentIndex, Vector3 position, Quaternion rotation, Vector3 scale)
        {
        }

        public void EndScene(uint[] rootNodeIndices)
        {
        }

        public void SetNodeName(uint nodeIndex, string name)
        {
        }
    }

    private class GLTFMaterialGenerator : GLTFast.Materials.IMaterialGenerator
    {
        private UnityEngine.Material _material;


        public UnityEngine.Material GenerateMaterial(MaterialBase gltfMaterial, IGltfReadable gltf, bool pointsSupport = false)
        {
            if (_material == null)
                _material = Resources.Load<UnityEngine.Material>("HDRPDefaultMat");

            return _material;
        }

        public UnityEngine.Material GetDefaultMaterial(bool pointsSupport = false)
        {
            if (_material == null)
                _material = Resources.Load<UnityEngine.Material>("HDRPDefaultMat");

            return _material;
        }

        public void SetLogger(ICodeLogger logger)
        {
            
        }
    }

    private class GLTFDownloadProvider : GLTFast.Loading.IDownloadProvider
    {
        public async Task<IDownload> Request(System.Uri url)
        {
            Debug.Log("GLTFDownload:" + url);
            var req = new AwaitableDownload(url);
            await req.WaitAsync();
            return req;
        }

        public Task<ITextureDownload> RequestTexture(System.Uri url, bool nonReadable)
        {
#if UNITY_WEBREQUEST_TEXTURE
            var req = new AwaitableTextureDownload(url,nonReadable);
            await req.WaitAsync();
            return req;
#else
            return null;
#endif

        }
    }


    /// <summary>
    /// Import external model to the transform of the passed in object to build from.
    /// </summary>
    private static async Task<bool> ImportGeometry(ExternalAssetMetadata metadata, GameObject baseObj)
    {
        //GltfImport gltf = new GltfImport(materialGenerator: new GLTFMaterialGenerator());
        GltfImport gltf = new GltfImport();
        gltf.CacheID = metadata.AssetID;

        ImportSettings settings = new ImportSettings();
        settings.GenerateMipMaps = true;

        //settings.AnisotropicFilterLevel = 0;
        //settings.DefaultMagFilterMode = Sampler.MagFilterMode.None;
        //settings.DefaultMinFilterMode = Sampler.MinFilterMode.None;
        //settings.GenerateMipMaps = false;

        string targetFile = Path.Combine(metadata.SourceFolder, metadata.GeometryFilename);

        //Error checking
        if (!File.Exists(targetFile))
        {
            Debug.LogError($"Error loading {metadata.AssetID}, geometry file ({targetFile}) does not exist!");
            return false;
        }

        //byte[] data = File.ReadAllBytes(targetFile);

        //bool success = await gltf.LoadGltfBinary(data, new System.Uri(targetFile), settings);
        bool success = await gltf.LoadFile(targetFile, new System.Uri(targetFile), settings);

        if (!success)
        {
            Debug.LogError($"Error loading {metadata.AssetID}, Failed to load GLTF binary");
            return false;
        }

        success = await gltf.InstantiateMainSceneAsync(baseObj.transform);

        //var instantiationSettings = new InstantiationSettings();
        //instantiationSettings.Mask = ComponentType.Mesh;
        //GameObjectInstantiator instantiator = new GameObjectInstantiator(gltf, baseObj.transform, settings:instantiationSettings);
        //success = await gltf.InstantiateMainSceneAsync(instantiator);

        if(!success)
        {
            Debug.LogError($"Error loading {metadata.AssetID}, Failed to instantiate geometry!");
            return false;
        }

        return true;
    }

    public static async Task<LoadableAudioProperties> LoadAudio(ExternalAssetMetadata metadata, AudioMaterialList validMaterials)
    {
        LoadableAudioProperties lAP = new LoadableAudioProperties();
        
        try
        {
            AudioMaterial am;
            if (!string.IsNullOrEmpty(metadata.AudioMaterial) && validMaterials.TryGetMaterialByName(metadata.AudioMaterial, out am))
            {
                //AudioMaterial am = await Addressables.LoadAssetAsync<AudioMaterial>($"AudioMaterial/{metadata.AudioMaterial}").Task;
                if (am != null)
                {
                    lAP.AudioMaterial = am;
                }
                else
                {
                    //AudioMaterial fallback = await Addressables.LoadAssetAsync<AudioMaterial>($"AudioMaterial/Stone").Task;
                    AudioMaterial fallback = validMaterials.FallbackMaterial;
                    lAP.AudioMaterial = fallback;
                }
            }
            else
            {
                //AudioMaterial fallback = await Addressables.LoadAssetAsync<AudioMaterial>($"AudioMaterial/Stone").Task;
                AudioMaterial fallback = validMaterials.FallbackMaterial;
                lAP.AudioMaterial = fallback;
            }
            lAP.UseSizeOverride = metadata.UseSizeOverride;
            Bounds b = new Bounds(metadata.SizeOverrideCenter.ToVector3(), metadata.SizeOverrideSize.ToVector3());
            lAP.SizeOverride = b;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading audio properties: {ex.Message} {ex.StackTrace}");
        }
        
        return lAP;
    }

    private static void SetupColliders(GeometryObjectInfo geomObjInfo, ExternalAssetMetadata metadata, GameObject baseObj, LoadableAssetManager loadableAssetManager)
    {
        if (baseObj.TryGetComponent<Collider>(out var _))
        {
            //colliders already setup
            return;
        }

        LOD[] lodGroups = null;
        var lodGroup = baseObj.GetComponentInChildren<LODGroup>();
        if (lodGroup != null)
        {
            lodGroups = lodGroup.GetLODs();
        }


        int colliderCount = 0;
        MeshFilter[] childFilters = baseObj.GetComponentsInChildren<MeshFilter>();

        if (childFilters == null || childFilters.Length <= 0)
        {
            Debug.LogError($"ExternalAsset: Couldn't add colliders to {metadata.AssetID}: no mesh filters found");
            return;
        }

        foreach (MeshFilter child in childFilters)
        {
            // COLLIDER SETUP
            // Does the child have the collider name tag?
            if (metadata.MeshColliderName != null && child.name.Contains(metadata.MeshColliderName))
            {
                //AddMeshCollider(child.gameObject);
                GameObject targetObj = null;
                if (lodGroup != null)
                    targetObj = lodGroup.gameObject;
                else
                    targetObj = child.gameObject;

                loadableAssetManager.AddDeferredMeshCollider(geomObjInfo, targetObj, child.sharedMesh);
                colliderCount++;
            }
        }

        if (colliderCount > 0)
        {
            Debug.Log($"ExternalAsset: Added {colliderCount} mesh colliders to {metadata.AssetID} matching collider name {metadata.MeshColliderName}");
            return;
        }

        int lodGroupIndex = -1;
        LOD lod = default;
        if (lodGroups != null && lodGroups.Length > 0)
        {
            lod = lodGroups[lodGroups.Length - 1];

            //make sure the lod group is valid before setting index
            if (lod.renderers != null && lod.renderers.Length > 0)
                lodGroupIndex = lodGroups.Length - 1;
            
        }

        if (lodGroupIndex >= 0)
        {
            Debug.Log($"ExternalAsset: Creating {lod.renderers.Length} mesh colliders for {metadata.AssetID} from lod group {lodGroups.Length - 1}");

            foreach (var renderer in (lod.renderers))
            {
                if (renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                    continue; //ignore shadow caster

                //AddMeshCollider(renderer.gameObject);
                if (renderer.gameObject.TryGetComponent<MeshFilter>(out var filter))
                {
                    loadableAssetManager.AddDeferredMeshCollider(geomObjInfo, lodGroup.gameObject/*renderer.gameObject*/, filter.sharedMesh);
                }
            }
        }
        else
        {
            Debug.Log($"ExternalAsset: Creating {childFilters.Length} mesh colliders for {metadata.AssetID} (no lod groups)");

            foreach (var child in childFilters)
            {
                //AddMeshCollider(child.gameObject);
                loadableAssetManager.AddDeferredMeshCollider(geomObjInfo, child.gameObject, child.sharedMesh);
            }
        }
    }

    private static void AddMeshCollider(GameObject obj)
    {
        if (obj == null)
            return;

        var meshCollider = obj.AddComponent<MeshCollider>();

        //meshCollider.cookingOptions = MeshColliderCookingOptions.EnableMeshCleaning | 
        //    MeshColliderCookingOptions.CookForFasterSimulation | 
        //    MeshColliderCookingOptions.UseFastMidphase | 
        //    MeshColliderCookingOptions.WeldColocatedVertices;
    }


    /// <summary>
    /// Create colliders from tagged children and set up LOD groups with the meshes in children named with LOD tag.
    /// </summary>
    private static void SetupLODsAndColliders(ExternalAssetMetadata metadata, GameObject baseObj)
    {
        if(baseObj.transform.childCount < 1)
        {
            Debug.LogWarning($"Base object for {metadata.AssetID} does not have a child, so no LODS or colliders will be generated.");
            return;
        }

        MeshFilter[] childFilters = baseObj.GetComponentsInChildren<MeshFilter>();

        Dictionary<string, List<MeshFilter>> lodGroupMap = new Dictionary<string, List<MeshFilter>>();

        // Sort LOD children into groups based on name
        foreach (MeshFilter child in childFilters)
        {
            // COLLIDER SETUP
            // Does the child have the collider name tag?
            if (metadata.MeshColliderName != null && child.name.Contains(metadata.MeshColliderName))
            {
                //child.gameObject.AddComponent<MeshCollider>();
            }

            // LOD SETUP
            // Is this an LOD object?
            int lodCharIndex = child.name.IndexOf("LOD") - 1;
            if(lodCharIndex >= 0)
            {// Yes, add to group
                string groupName = child.name[..lodCharIndex];

                // Check if a group has already been made
                if (lodGroupMap.ContainsKey(groupName))
                {// Already a group, add child to it
                    lodGroupMap[groupName].Add(child);
                }
                else
                {// Need to make a new group
                    lodGroupMap.Add(groupName, new List<MeshFilter>());
                    lodGroupMap[groupName].Add(child);
                }
            }
        }

        // Check if we didnt find any children with the LOD tag and log
        //if(lodGroupMap.Keys.Count == 0)
        //{
        //    Debug.LogWarning("No LOD tags found in children, so no LODS will be generated. Adding mesh collider to all meshes");

        //    // If not LODs are present, add mesh collider to all meshes.
        //    foreach (MeshFilter filter in childFilters)
        //    {
        //        //filter.gameObject.AddComponent<MeshCollider>();
        //    }
        //    return;
        //}

        ////bool noColliders = false;
        //// Check if we didnt set up any colliders
        //if (baseObj.GetComponentInChildren<Collider>() == null)
        //{
        //    Debug.LogWarning("No colliders were created. Add default colliders during LOD grouping!");
        //}

        Regex lodReg = new Regex("^.*?LOD(\\d+)");

        // Make group objects and set up LOD component
        foreach (string groupName in lodGroupMap.Keys)
        {

            if (lodGroupMap[groupName].Count <= 1)
                continue;

            // Create object to hold children of the group
            GameObject groupObj = new GameObject(groupName);
            groupObj.transform.SetParent(baseObj.transform);

            LODGroup groupLODGroup = groupObj.AddComponent<LODGroup>();

            List<LOD> lods = new List<LOD>();
            SortedDictionary<int, List<MeshRenderer>> lodMeshRenderers = new SortedDictionary<int, List<MeshRenderer>>();
            
            // If we found only one LOD in the group then we should skip doing grouping and just attach collider
            //if(lodGroupMap[groupName].Count == 1)
            //{
            //    Object.Destroy(groupObj);
            //    //lodGroupMap[groupName][0].gameObject.AddComponent<MeshCollider>();
            //    continue;
            //}

            foreach(MeshFilter lodChild in lodGroupMap[groupName])
            {
                // Set group as parent of child for organization
                lodChild.transform.SetParent(groupObj.transform);

                // Determine LOD number
                //int lodNumber = int.Parse(lodChild.name.Substring(lodChild.name.IndexOf("LOD") + 3));
                var match = lodReg.Match(lodChild.name);
                int lodNumber = -1;

                if (!match.Success || match.Groups.Count < 2 || !int.TryParse(match.Groups[1].Value, out lodNumber))
                {
                    Debug.LogError($"Couldn't parse LOD group from {lodChild.name} when importing {metadata.AssetID}");
                    continue;
                }

                var lodRenderer = lodChild.GetComponent<MeshRenderer>();

                lodRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                List<MeshRenderer> rendererList = null;
                if (!lodMeshRenderers.TryGetValue(lodNumber, out rendererList))
                {
                    rendererList = new List<MeshRenderer>();
                    lodMeshRenderers.Add(lodNumber, rendererList);
                }

                rendererList.Add(lodRenderer);

                //if (metadata.LODLevels != null && lodNumber < metadata.LODLevels.Count)
                //{
                //    // If we have less LODs than expected, make sure not to cull.
                //    if (lodNumber == lodGroupMap[groupName].Count - 1 && lodGroupMap[groupName].Count < metadata.LODLevels.Count)
                //    {
                //        Debug.LogWarning("Number of children with LOD tag does not match number of LOD levels in metadata! Avoiding culling!");
                //        lods.Add(new LOD(0, new[] { lodRenderer }));
                //    }
                //    else
                //    {
                //        lods.Add(new LOD(metadata.LODLevels[lodNumber].ScreenRelativeHeight, new[] { lodRenderer }));
                //    }
                //}
                //else
                //{// Dont have or missing LOD levels, so we need to generate some
                //    Debug.LogWarning("Missing LOD values, generating default values!");

                //    List<LODLevelData> defaultLevels = ExternalAssetManager.GenerateLODValues(lodGroupMap[groupName].Count);

                //    lods.Add(new LOD(defaultLevels[lodNumber].ScreenRelativeHeight, new[] { lodRenderer }));
                //}
            }

            List<LODLevelData> lodLevelData = metadata.LODLevels;
            if (lodLevelData == null || lodMeshRenderers.Count != metadata.LODLevels.Count)
            {
                Debug.LogWarning($"Lod level count mismatch on {metadata.AssetID} using default lod levels (expected {lodMeshRenderers.Count})");
                lodLevelData = ExternalAssetManager.GenerateLODValues(lodMeshRenderers.Count);
            }

            //sequentially build the lod levels from the sorted dictionary, ignoring any gaps in the numbering
            var lastLodLevel = -1;
            int lodLevel = 0;
            foreach (var kvp in lodMeshRenderers)
            {
                if (kvp.Key < lastLodLevel)
                {
                    Debug.LogError($"Error in lod level sorting for {metadata.AssetID}, lod {kvp.Key}");
                }
                lastLodLevel = kvp.Key;
                var lodData = lodLevelData[lodLevel];

                lods.Add(new LOD(lodData.ScreenRelativeHeight, kvp.Value.ToArray()));
                lodLevel++;
            }

            //setup shadow caster
            var shadowFilter = groupObj.AddComponent<MeshFilter>();
            var shadowRend = groupObj.AddComponent<MeshRenderer>();

            var shadowMesh = lods[lods.Count - 1].renderers[0].GetComponent<MeshFilter>().sharedMesh;
            shadowFilter.sharedMesh = shadowMesh;
            shadowRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;


            // Sort lods by decreasing detail so they are added correctly to the lod group
            lods = lods.OrderByDescending(lod => lod.screenRelativeTransitionHeight).ToList();

            // Use least detailed LOD to be default collider
            if (groupObj.gameObject.GetComponentsInChildren<Collider>().Length == 0)
            {
                //lods[lods.Count - 1].renderers[0].gameObject.AddComponent<MeshCollider>();
            }

            // Sort the lod array to ensure LODs are ordered from LOD0 to LOD2
            groupLODGroup.SetLODs(lods.ToArray());
        }
    }

    //private static void AddMeshToLODGroup(ExternalAssetMetadata metadata, MeshRenderer mesh, int lodNumber, Dictionary<int, LOD> lods)
    //{
    //    LOD lod;

    //    if (lods.TryGetValue(lodNumber, out lod))
    //    {

    //    }
    //    else
    //    {
    //        if (metadata.LODLevels != null && metadata.LODLevels.Count > lodNumber)
    //            lod = new LOD(metadata.LODLevels[lodNumber].ScreenRelativeHeight, new[] { mesh } );
    //        else
    //            lod = 
    //    }
    //}


    private static void SetupLights(GameObject baseObj)
    {
        Light[] childLights = baseObj.GetComponentsInChildren<Light>();        

        foreach(Light light in childLights)
        {
            ComponentInfo_Light compInfo = light.gameObject.AddComponent<ComponentInfo_Light>();

            //light.EnableColorTemperature(true);
            light.useColorTemperature = true;

            //compInfo.m_component = light;
            compInfo.m_Lights = new List<Light>();
            compInfo.m_Lights.Add(light);
            
            // Initialize component info with light values
            compInfo.intensity = light.intensity;
            compInfo.range = light.range;
            compInfo.color = light.color;
            compInfo.isOn = light.isActiveAndEnabled;
            compInfo.RGBValue = ColorUtility.ToHtmlStringRGBA(light.color);
        }
    }


}