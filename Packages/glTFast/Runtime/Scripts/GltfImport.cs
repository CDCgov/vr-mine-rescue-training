// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if !UNITY_WEBGL || UNITY_EDITOR
#define GLTFAST_THREADS
#endif

#if KTX_UNITY_2_2_OR_NEWER || (!UNITY_2021_2_OR_NEWER && KTX_UNITY_1_3_OR_NEWER)
#define KTX
#elif KTX_UNITY
#warning You have to update *KTX for Unity* to enable support for KTX textures in glTFast
#endif

// #define MEASURE_TIMINGS

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System;

using GLTFast.Addons;
using GLTFast.Jobs;
#if MEASURE_TIMINGS
using GLTFast.Tests;
#endif
#if KTX
using KtxUnity;
#endif
#if MESHOPT
using Meshoptimizer;
#endif
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine;
using Debug = UnityEngine.Debug;

[assembly: InternalsVisibleTo("glTFast.Editor")]
[assembly: InternalsVisibleTo("glTFast.Editor.Tests")]
[assembly: InternalsVisibleTo("glTFast.Export")]
[assembly: InternalsVisibleTo("glTFast.Tests")]

namespace GLTFast
{

    using Loading;
    using Logging;
    using Materials;
    using Schema;

    /// <summary>
    /// Loads a glTF's content, converts it to Unity resources and is able to
    /// feed it to an <see cref="IInstantiator"/> for instantiation.
    /// Uses the efficient and fast JsonUtility/<see cref="GltfJsonUtilityParser"/> for JSON parsing.
    /// </summary>
    public class GltfImport : GltfImportBase<Root>
    {
        static GltfJsonUtilityParser s_Parser;

        /// <inheritdoc cref="GltfImportBase(IDownloadProvider,IDeferAgent,IMaterialGenerator,ICodeLogger"/>
        public GltfImport(
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
        ) : base(downloadProvider, deferAgent, materialGenerator, logger) { }

        /// <inheritdoc />
        protected override RootBase ParseJson(string json)
        {
            s_Parser ??= new GltfJsonUtilityParser();
            return s_Parser.ParseJson(json);
        }
    }

    /// <inheritdoc cref="GltfImportBase"/>
    /// <typeparam name="TRoot">Root schema class to use for de-serialization.</typeparam>
    public abstract class GltfImportBase<TRoot> : GltfImportBase, IGltfReadable<TRoot>
        where TRoot : RootBase
    {
        /// <inheritdoc cref="GltfImportBase(IDownloadProvider,IDeferAgent,IMaterialGenerator,ICodeLogger"/>
        public GltfImportBase(
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
        ) : base(downloadProvider, deferAgent, materialGenerator, logger) { }

        TRoot m_Root;

        /// <inheritdoc />
        protected override RootBase Root
        {
            get => m_Root;
            set => m_Root = (TRoot)value;
        }

        /// <inheritdoc />
        public TRoot GetSourceRoot()
        {
            return m_Root;
        }
    }

    /// <summary>
    /// Loads a glTF's content, converts it to Unity resources and is able to
    /// feed it to an <see cref="IInstantiator"/> for instantiation.
    /// </summary>
    public abstract class GltfImportBase : IGltfReadable, IGltfBuffers, IDisposable
    {
        public string CacheID = null;

        /// <summary>
        /// Default value for a C# Job's innerloopBatchCount parameter.
        /// </summary>
        /// <seealso cref="IJobParallelForExtensions.Schedule(x,int,int,JobHandle)"/>
        internal const int DefaultBatchCount = 512;

        /// <summary>
        /// JSON parse speed in bytes per second
        /// Measurements based on a MacBook Pro Intel(R) Core(TM) i9-9980HK CPU @ 2.40GHz
        /// and reduced by ~ 20%
        /// </summary>
        const int k_JsonParseSpeed =
#if UNITY_EDITOR
            45_000_000;
#else
            80_000_000;
#endif
        /// <summary>
        /// Base 64 string to byte array decode speed in bytes per second
        /// Measurements based on a MacBook Pro Intel(R) Core(TM) i9-9980HK CPU @ 2.40GHz
        /// and reduced by ~ 20%
        /// </summary>
        const int k_Base64DecodeSpeed =
#if UNITY_EDITOR
            60_000_000;
#else
            150_000_000;
#endif

        const string k_PrimitiveName = "Primitive";

        static readonly HashSet<string> k_SupportedExtensions = new HashSet<string> {
#if DRACO_UNITY
            ExtensionName.DracoMeshCompression,
#endif
#if KTX
            ExtensionName.TextureBasisUniversal,
#endif // KTX_UNITY
#if MESHOPT
            ExtensionName.MeshoptCompression,
#endif
            ExtensionName.MaterialsPbrSpecularGlossiness,
            ExtensionName.MaterialsUnlit,
            ExtensionName.TextureTransform,
            ExtensionName.MeshQuantization,
            ExtensionName.MaterialsTransmission,
            ExtensionName.MeshGPUInstancing,
            ExtensionName.LightsPunctual,
            ExtensionName.MaterialsClearcoat,
        };

        static IDeferAgent s_DefaultDeferAgent;

        IDownloadProvider m_DownloadProvider;
        IMaterialGenerator m_MaterialGenerator;
        IDeferAgent m_DeferAgent;

        Dictionary<Type, ImportAddonInstance> m_ImportInstances;

        ImportSettings m_Settings;

        byte[][] m_Buffers;

        /// <summary>
        /// GCHandles for pinned managed arrays <see cref="m_Buffers"/>
        /// </summary>
        GCHandle?[] m_BufferHandles;

        /// <summary>
        /// NativeArray views into <see cref="m_Buffers"/>
        /// </summary>
        NativeArray<byte>[] m_NativeBuffers;

        GlbBinChunk[] m_BinChunks;

        Dictionary<int, Task<IDownload>> m_DownloadTasks;
#if KTX
        Dictionary<int,Task<IDownload>> m_KtxDownloadTasks;
#endif
        Dictionary<int, TextureDownloadBase> m_TextureDownloadTasks;

        AccessorDataBase[] m_AccessorData;
        AccessorUsage[] m_AccessorUsage;
        JobHandle m_AccessorJobsHandle;
        PrimitiveCreateContextBase[] m_PrimitiveContexts;
        Dictionary<MeshPrimitiveBase, VertexBufferConfigBase> m_VertexAttributes;
        /// <summary>
        /// Array of dictionaries, indexed by mesh ID
        /// The dictionary contains all the mesh's primitives, clustered
        /// by Vertex Attribute and Morph Target usage (Primitives with identical vertex
        /// data will be clustered; <see cref="MeshPrimitiveBase.Equals(object)"/>).
        /// </summary>
        Dictionary<MeshPrimitiveBase, List<(int MeshIndex, MeshPrimitiveBase Primitive)>>[] m_MeshPrimitiveCluster;
        List<ImageCreateContext> m_ImageCreateContexts;
#if KTX
        List<KtxLoadContextBase> m_KtxLoadContextsBuffer;
#endif // KTX_UNITY


        /// <summary>
        /// Loaded glTF images (Raw texture without sampler settings)
        /// <seealso cref="m_Textures"/>
        /// </summary>
        Texture2D[] m_Images;

        /// <summary>
        /// In glTF a texture is an image with a certain sampler setting applied.
        /// So any `images` member is also in `textures`, but not necessary the
        /// other way around.
        /// /// <seealso cref="m_Images"/>
        /// </summary>
        Texture2D[] m_Textures;

#if KTX
        HashSet<int> m_NonFlippedYTextureIndices;
#endif
        ImageFormat[] m_ImageFormats;
        bool[] m_ImageReadable;
        bool[] m_ImageGamma;

        /// optional glTF-binary buffer
        /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#binary-buffer
        GlbBinChunk? m_GlbBinChunk;

#if MESHOPT
        Dictionary<int, NativeArray<byte>> m_MeshoptBufferViews;
        NativeArray<int> m_MeshoptReturnValues;
        JobHandle m_MeshoptJobHandle;
#endif

        /// <summary>
        /// Material IDs of materials that require points topology support.
        /// </summary>
        HashSet<int> m_MaterialPointsSupport;
        bool m_DefaultMaterialPointsSupport;

        /// <summary>Main glTF data structure</summary>
        protected abstract RootBase Root { get; set; }
        UnityEngine.Material[] m_Materials;
        List<UnityEngine.Object> m_Resources;

        /// <summary>
        /// Unity's animation system addresses target GameObjects by hierarchical name.
        /// To make sure names are consistent and have no conflicts they are precalculated
        /// and stored in this array.
        /// </summary>
        string[] m_NodeNames;

        MeshResult[] m_Primitives;
        int[] m_MeshPrimitiveIndex;
        Matrix4x4[][] m_SkinsInverseBindMatrices;
#if UNITY_ANIMATION
        AnimationClip[] m_AnimationClips;
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Required for Editor import only to preserve default/fallback materials
        /// </summary>
        public UnityEngine.Material defaultMaterial;
#endif

        /// <summary>
        /// True, when loading has finished and glTF can be instantiated
        /// </summary>
        public bool LoadingDone { get; private set; }

        /// <summary>
        /// True if an error happened during glTF loading
        /// </summary>
        public bool LoadingError { get; private set; }

        ICodeLogger m_Logger;

        /// <summary>
        /// Constructs a GltfImport instance with injectable customization objects.
        /// </summary>
        /// <param name="downloadProvider">Provides file access or download customization</param>
        /// <param name="deferAgent">Provides custom update loop behavior for better frame rate control</param>
        /// <param name="materialGenerator">Provides custom glTF to Unity material conversion</param>
        /// <param name="logger">Provides custom message logging</param>
        public GltfImportBase(
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
            )
        {
            m_DownloadProvider = downloadProvider ?? new DefaultDownloadProvider();

            if (deferAgent == null)
            {
                if (s_DefaultDeferAgent == null
                    || (s_DefaultDeferAgent is UnityEngine.Object agent && agent == null) // Cast to Object to enforce Unity Object's null check (is MonoBehavior alive?)
                    )
                {
                    var defaultDeferAgentGameObject = new GameObject("glTF-StableFramerate");
                    // Keep it across scene loads
                    UnityEngine.Object.DontDestroyOnLoad(defaultDeferAgentGameObject);
                    SetDefaultDeferAgent(defaultDeferAgentGameObject.AddComponent<TimeBudgetPerFrameDeferAgent>());
                    // Adding a DefaultDeferAgent component will make it un-register via <see cref="UnsetDefaultDeferAgent"/>
                    defaultDeferAgentGameObject.AddComponent<DefaultDeferAgent>();
                }
                m_DeferAgent = s_DefaultDeferAgent;
            }
            else
            {
                m_DeferAgent = deferAgent;
            }
            m_MaterialGenerator = materialGenerator ?? MaterialGenerator.GetDefaultMaterialGenerator();

            m_Logger = logger;

            ImportAddonRegistry.InjectAllAddons(this);
        }

        /// <summary>
        /// Sets the default <see cref="IDeferAgent"/> for subsequently
        /// generated GltfImport instances.
        /// </summary>
        /// <param name="deferAgent">New default <see cref="IDeferAgent"/></param>
        public static void SetDefaultDeferAgent(IDeferAgent deferAgent)
        {
#if DEBUG
            if (s_DefaultDeferAgent!=null && s_DefaultDeferAgent != deferAgent) {
                Debug.LogWarning("GltfImport.defaultDeferAgent got overruled! Make sure there is only one default at any time", deferAgent as UnityEngine.Object);
            }
#endif
            s_DefaultDeferAgent = deferAgent;
        }

        /// <summary>
        /// Allows un-registering default <see cref="IDeferAgent"/>.
        /// For example if it's no longer available.
        /// </summary>
        /// <param name="deferAgent"><see cref="IDeferAgent"/> in question</param>
        public static void UnsetDefaultDeferAgent(IDeferAgent deferAgent)
        {
            if (s_DefaultDeferAgent == deferAgent)
            {
                s_DefaultDeferAgent = null;
            }
        }

        /// <summary>
        /// Adds an import add-on instance. To be called before any loading is initiated.
        /// </summary>
        /// <param name="importInstance">The import instance to add.</param>
        /// <typeparam name="T">Type of the import instance</typeparam>
        public void AddImportAddonInstance<T>(T importInstance) where T : ImportAddonInstance
        {
            if (m_ImportInstances == null)
            {
                m_ImportInstances = new Dictionary<Type, ImportAddonInstance>();
            }
            m_ImportInstances[typeof(T)] = importInstance;
        }

        /// <summary>
        /// Queries the import instance of a particular type.
        /// </summary>
        /// <typeparam name="T">Type of the import instance</typeparam>
        /// <returns>The import instance that was previously added. False if there was none.</returns>
        public T GetImportAddonInstance<T>() where T : ImportAddonInstance
        {
            if (m_ImportInstances == null)
                return null;

            if (m_ImportInstances.TryGetValue(typeof(T), out var addonInstance))
            {
                return (T)addonInstance;
            }

            return null;
        }

        /// <summary>
        /// Load a glTF file (JSON or binary)
        /// The URL can be a file path (using the "file://" scheme) or a web address.
        /// </summary>
        /// <param name="url">Uniform Resource Locator. Can be a file path (using the "file://" scheme) or a web address.</param>
        /// <param name="importSettings">Import Settings (<see cref="ImportSettings"/> for details)</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public async Task<bool> Load(
            string url,
            ImportSettings importSettings = null,
            CancellationToken cancellationToken = default
            )
        {
            return await Load(new Uri(url, UriKind.RelativeOrAbsolute), importSettings, cancellationToken);
        }

        /// <summary>
        /// Load a glTF file (JSON or binary)
        /// The URL can be a file path (using the "file://" scheme) or a web address.
        /// </summary>
        /// <param name="url">Uniform Resource Locator. Can be a file path (using the "file://" scheme) or a web address.</param>
        /// <param name="importSettings">Import Settings (<see cref="ImportSettings"/> for details)</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public async Task<bool> Load(
            Uri url,
            ImportSettings importSettings = null,
            CancellationToken cancellationToken = default
            )
        {
            m_Settings = importSettings ?? new ImportSettings();
            return await LoadFromUri(url, cancellationToken);
        }

        /// <summary>
        /// Load a glTF from a byte array.
        /// If the type (JSON or glTF-Binary) is known,
        /// <see cref="LoadGltfJson"/> and <see cref="LoadGltfBinary"/>
        /// should be preferred.
        /// </summary>
        /// <param name="data">Either glTF-Binary data or a glTF JSON</param>
        /// <param name="uri">Base URI for relative paths of external buffers or images</param>
        /// <param name="importSettings">Import Settings (<see cref="ImportSettings"/> for details)</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public async Task<bool> Load(
            byte[] data,
            Uri uri = null,
            ImportSettings importSettings = null,
            CancellationToken cancellationToken = default
            )
        {
            if (GltfGlobals.IsGltfBinary(data))
            {
                return await LoadGltfBinary(data, uri, importSettings, cancellationToken);
            }

            // Fallback interpreting data as string
            var json = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
            return await LoadGltfJson(json, uri, importSettings, cancellationToken);
        }

        /// <summary>
        /// Load glTF from a local file path.
        /// </summary>
        /// <param name="localPath">Local path to glTF or glTF-Binary file.</param>
        /// <param name="uri">Base URI for relative paths of external buffers or images</param>
        /// <param name="importSettings">Import Settings (<see cref="ImportSettings"/> for details)</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public async Task<bool> LoadFile(
            string localPath,
            Uri uri = null,
            ImportSettings importSettings = null,
            CancellationToken cancellationToken = default
            )
        {
            var firstBytes = new byte[4];

#if UNITY_2021_3_OR_NEWER && NET_STANDARD_2_1
            await using
#endif
            var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read);
            var bytesRead = await fs.ReadAsync(firstBytes, 0, firstBytes.Length, cancellationToken);

            if (bytesRead != firstBytes.Length)
            {
                m_Logger?.Error(LogCode.Download, "Failed reading first bytes", localPath);
                return false;
            }

            if (cancellationToken.IsCancellationRequested) return false;

            if (GltfGlobals.IsGltfBinary(firstBytes))
            {
                var data = new byte[fs.Length];
                for (var i = 0; i < firstBytes.Length; i++)
                {
                    data[i] = firstBytes[i];
                }
                var length = (int)fs.Length - 4;
                var read = await fs.ReadAsync(data, 4, length, cancellationToken);
                fs.Close();
                if (read != length)
                {
                    m_Logger?.Error(LogCode.Download, "Failed reading data", localPath);
                    return false;
                }

                return await LoadGltfBinary(data, uri, importSettings, cancellationToken);
            }
            fs.Close();

            return await LoadGltfJson(
#if UNITY_2021_3_OR_NEWER
                await File.ReadAllTextAsync(localPath,cancellationToken),
#else
                File.ReadAllText(localPath),
#endif
                uri,
                importSettings, cancellationToken);
        }

        /// <summary>
        /// Load a glTF-binary asset from a byte array.
        /// </summary>
        /// <param name="bytes">byte array containing glTF-binary</param>
        /// <param name="uri">Base URI for relative paths of external buffers or images</param>
        /// <param name="importSettings">Import Settings (<see cref="ImportSettings"/> for details)</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public async Task<bool> LoadGltfBinary(
            byte[] bytes,
            Uri uri = null,
            ImportSettings importSettings = null,
            CancellationToken cancellationToken = default
            )
        {
            m_Settings = importSettings ?? new ImportSettings();
            var success = await LoadGltfBinaryBuffer(bytes, uri);
            if (success) await LoadContent();
            success = success && await Prepare();
            DisposeVolatileData();
            LoadingError = !success;
            LoadingDone = true;
            return success;
        }

        /// <summary>
        /// Load a glTF JSON from a string
        /// </summary>
        /// <param name="json">glTF JSON</param>
        /// <param name="uri">Base URI for relative paths of external buffers or images</param>
        /// <param name="importSettings">Import Settings (<see cref="ImportSettings"/> for details)</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public async Task<bool> LoadGltfJson(
            string json,
            Uri uri = null,
            ImportSettings importSettings = null,
            CancellationToken cancellationToken = default
            )
        {
            m_Settings = importSettings ?? new ImportSettings();
            var success = await LoadGltf(json, uri);
            if (success) await LoadContent();
            success = success && await Prepare();
            DisposeVolatileData();
            LoadingError = !success;
            LoadingDone = true;
            return success;
        }

        /// <inheritdoc cref="InstantiateMainSceneAsync(Transform,CancellationToken)"/>
        [Obsolete("Use InstantiateMainSceneAsync for increased performance and safety. Consult the Upgrade Guide for instructions.")]
        public bool InstantiateMainScene(Transform parent)
        {
            return InstantiateMainSceneAsync(parent).Result;
        }

        /// <inheritdoc cref="InstantiateMainSceneAsync(IInstantiator,CancellationToken)"/>
        [Obsolete("Use InstantiateMainSceneAsync for increased performance and safety. Consult the Upgrade Guide for instructions.")]
        public bool InstantiateMainScene(IInstantiator instantiator)
        {
            return InstantiateMainSceneAsync(instantiator).Result;
        }

        /// <inheritdoc cref="InstantiateSceneAsync(Transform,int,CancellationToken)"/>
        [Obsolete("Use InstantiateSceneAsync for increased performance and safety. Consult the Upgrade Guide for instructions.")]
        public bool InstantiateScene(Transform parent, int sceneIndex = 0)
        {
            return InstantiateSceneAsync(parent, sceneIndex).Result;
        }

        /// <inheritdoc cref="InstantiateSceneAsync(IInstantiator,int,CancellationToken)"/>
        [Obsolete("Use InstantiateSceneAsync for increased performance and safety. Consult the Upgrade Guide for instructions.")]
        public bool InstantiateScene(IInstantiator instantiator, int sceneIndex = 0)
        {
            return InstantiateSceneAsync(instantiator, sceneIndex).Result;
        }

        /// <summary>
        /// Creates an instance of the main scene of the glTF ( <see cref="RootBase.scene">scene</see> property in the JSON at root level)
        /// If the main scene index is not set, it instantiates nothing (as defined in the glTF 2.0 specification)
        /// </summary>
        /// <param name="parent">Transform that the scene will get parented to</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the main scene was instantiated or was not set. False in case of errors.</returns>
        /// <seealso cref="DefaultSceneIndex"/>
        public async Task<bool> InstantiateMainSceneAsync(
            Transform parent,
            CancellationToken cancellationToken = default
            )
        {
            var instantiator = new GameObjectInstantiator(this, parent);
            var success = await InstantiateMainSceneAsync(instantiator, cancellationToken);
            return success;
        }

        /// <summary>
        /// Creates an instance of the main scene of the glTF ( <see cref="RootBase.scene">scene</see> property in the JSON at root level)
        /// If the main scene index is not set, it instantiates nothing (as defined in the glTF 2.0 specification)
        /// </summary>
        /// <param name="instantiator">Instantiator implementation; Receives and processes the scene data</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the main scene was instantiated or was not set. False in case of errors.</returns>
        /// <seealso cref="DefaultSceneIndex"/>
        public async Task<bool> InstantiateMainSceneAsync(
            IInstantiator instantiator,
            CancellationToken cancellationToken = default
            )
        {
            if (!LoadingDone || LoadingError) return false;
            // According to glTF specification, loading nothing is
            // the correct behavior
            if (Root.scene < 0)
            {
#if DEBUG
                Debug.LogWarning("glTF has no (main) scene defined. No scene will be instantiated.");
#endif
                return true;
            }
            return await InstantiateSceneAsync(instantiator, Root.scene, cancellationToken);
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="parent">Transform that the scene will get parented to</param>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the scene was instantiated. False in case of errors.</returns>
        /// <seealso cref="SceneCount"/>
        /// <seealso cref="GetSceneName"/>
        public async Task<bool> InstantiateSceneAsync(
            Transform parent,
            int sceneIndex = 0,
            CancellationToken cancellationToken = default
            )
        {
            if (!LoadingDone || LoadingError) return false;
            if (sceneIndex < 0 || sceneIndex > Root.Scenes.Count) return false;
            var instantiator = new GameObjectInstantiator(this, parent);
            var success = await InstantiateSceneAsync(instantiator, sceneIndex, cancellationToken);
            return success;
        }

        /// <summary>
        /// Creates an instance of the scene specified by the scene index.
        /// </summary>
        /// <param name="instantiator">Instantiator implementation; Receives and processes the scene data</param>
        /// <param name="sceneIndex">Index of the scene to be instantiated</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>True if the scene was instantiated. False in case of errors.</returns>
        /// <seealso cref="SceneCount"/>
        /// <seealso cref="GetSceneName"/>
        public async Task<bool> InstantiateSceneAsync(
            IInstantiator instantiator,
            int sceneIndex = 0,
            CancellationToken cancellationToken = default
            )
        {
            if (!LoadingDone || LoadingError) return false;
            if (sceneIndex < 0 || sceneIndex > Root.Scenes.Count) return false;
            await InstantiateSceneInternal(Root, instantiator, sceneIndex);
            return true;
        }

        /// <summary>
        /// Frees up memory by disposing all sub assets.
        /// There can be no instantiation or other element access afterwards.
        /// </summary>
        public void Dispose()
        {
            if (m_ImportInstances != null)
            {
                foreach (var importInstance in m_ImportInstances)
                {
                    importInstance.Value.Dispose();
                }
                m_ImportInstances = null;
            }

            m_NodeNames = null;

            void DisposeArray(IEnumerable<UnityEngine.Object> objects)
            {
                if (objects != null)
                {
                    foreach (var obj in objects)
                    {
                        SafeDestroy(obj);
                    }
                }
            }

            DisposeArray(m_Materials);
            m_Materials = null;

#if UNITY_ANIMATION
            DisposeArray(m_AnimationClips);
            m_AnimationClips = null;
#endif

            DisposeArray(m_Textures);
            m_Textures = null;

            if (m_AccessorData != null)
            {
                foreach (var ad in m_AccessorData)
                {
                    ad?.Dispose();
                }
                m_AccessorData = null;
            }

            DisposeArray(m_Resources);
            m_Resources = null;
        }

        /// <summary>
        /// Number of materials
        /// </summary>
        public int MaterialCount => m_Materials?.Length ?? 0;

        /// <summary>
        /// Number of images
        /// </summary>
        public int ImageCount => m_Images?.Length ?? 0;

        /// <summary>
        /// Number of textures
        /// </summary>
        public int TextureCount => m_Textures?.Length ?? 0;

        /// <summary>
        /// Default scene index
        /// </summary>
        public int? DefaultSceneIndex => Root != null && Root.scene >= 0 ? Root.scene : (int?)null;

        /// <summary>
        /// Number of scenes
        /// </summary>
        public int SceneCount => Root?.Scenes?.Count ?? 0;

        /// <summary>
        /// Get a glTF's scene's name by its index
        /// </summary>
        /// <param name="sceneIndex">glTF scene index</param>
        /// <returns>Scene name or null</returns>
        public string GetSceneName(int sceneIndex)
        {
            return Root?.Scenes?[sceneIndex]?.name;
        }

        /// <inheritdoc />
        public UnityEngine.Material GetMaterial(int index = 0)
        {
            if (m_Materials != null && index >= 0 && index < m_Materials.Length)
            {
                return m_Materials[index];
            }
            return null;
        }

        /// <inheritdoc />
        public UnityEngine.Material GetDefaultMaterial()
        {
#if UNITY_EDITOR
            if (defaultMaterial == null) {
                m_MaterialGenerator.SetLogger(m_Logger);
                defaultMaterial = m_MaterialGenerator.GetDefaultMaterial(m_DefaultMaterialPointsSupport);
                m_MaterialGenerator.SetLogger(null);
            }
            return defaultMaterial;
#else
            m_MaterialGenerator.SetLogger(m_Logger);
            var material = m_MaterialGenerator.GetDefaultMaterial(m_DefaultMaterialPointsSupport);
            m_MaterialGenerator.SetLogger(null);
            return material;
#endif
        }

        /// <summary>
        /// Returns a texture by its glTF image index
        /// </summary>
        /// <param name="index">glTF image index</param>
        /// <returns>Corresponding Unity texture</returns>
        public Texture2D GetImage(int index = 0)
        {
            if (m_Images != null && index >= 0 && index < m_Images.Length)
            {
                return m_Images[index];
            }
            return null;
        }

        /// <summary>
        /// Returns a texture by its glTF texture index
        /// </summary>
        /// <param name="index">glTF texture index</param>
        /// <returns>Corresponding Unity texture</returns>
        public Texture2D GetTexture(int index = 0)
        {
            if (m_Textures != null && index >= 0 && index < m_Textures.Length)
            {
                return m_Textures[index];
            }
            return null;
        }

        /// <inheritdoc cref="IGltfReadable.IsTextureYFlipped"/>
        public bool IsTextureYFlipped(int index = 0)
        {
#if KTX
            return (m_NonFlippedYTextureIndices == null || !m_NonFlippedYTextureIndices.Contains(index)) && GetSourceTexture(index).IsKtx;
#else
            return false;
#endif
        }

#if UNITY_ANIMATION
        /// <summary>
        /// Returns all imported animation clips
        /// </summary>
        /// <returns>All imported animation clips</returns>
        public AnimationClip[] GetAnimationClips() {
            return m_AnimationClips;
        }
#endif

        /// <summary>
        /// Returns all imported meshes
        /// </summary>
        /// <returns>All imported meshes</returns>
        public UnityEngine.Mesh[] GetMeshes()
        {
            if (m_Primitives == null || m_Primitives.Length < 1) return null;
            var result = new UnityEngine.Mesh[m_Primitives.Length];
            for (var index = 0; index < m_Primitives.Length; index++)
            {
                var primitive = m_Primitives[index];
                result[index] = primitive.mesh;
            }
            return result;
        }

        /// <inheritdoc />
        public CameraBase GetSourceCamera(uint index)
        {
            if (Root?.Cameras != null && index < Root.Cameras.Count)
            {
                return Root.Cameras[(int)index];
            }
            return null;
        }

        /// <inheritdoc />
        public LightPunctual GetSourceLightPunctual(uint index)
        {
            if (Root?.Extensions?.KHR_lights_punctual.lights != null && index < Root.Extensions.KHR_lights_punctual.lights.Length)
            {
                return Root.Extensions.KHR_lights_punctual.lights[index];
            }
            return null;
        }

        /// <inheritdoc />
        public Scene GetSourceScene(int index = 0)
        {
            if (Root?.Scenes != null && index >= 0 && index < Root.Scenes.Count)
            {
                return Root.Scenes[index];
            }
            return null;
        }

        /// <inheritdoc />
        public MaterialBase GetSourceMaterial(int index = 0)
        {
            if (Root?.Materials != null && index >= 0 && index < Root.Materials.Count)
            {
                return Root.Materials[index];
            }
            return null;
        }

        /// <inheritdoc />
        public NodeBase GetSourceNode(int index = 0)
        {
            if (Root?.Nodes != null && index >= 0 && index < Root.Nodes.Count)
            {
                return Root.Nodes[index];
            }
            return null;
        }

        /// <inheritdoc />
        public TextureBase GetSourceTexture(int index = 0)
        {
            if (Root?.Textures != null && index >= 0 && index < Root.Textures.Count)
            {
                return Root.Textures[index];
            }
            return null;
        }

        /// <inheritdoc />
        public Image GetSourceImage(int index = 0)
        {
            if (Root?.Images != null && index >= 0 && index < Root.Images.Count)
            {
                return Root.Images[index];
            }
            return null;
        }

        /// <inheritdoc />
        public Matrix4x4[] GetBindPoses(int skinId)
        {
            if (m_SkinsInverseBindMatrices == null) return null;
            if (m_SkinsInverseBindMatrices[skinId] != null)
            {
                return m_SkinsInverseBindMatrices[skinId];
            }

            var skin = Root.Skins[skinId];
            var result = new Matrix4x4[skin.joints.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = Matrix4x4.identity;
            }
            m_SkinsInverseBindMatrices[skinId] = result;
            return result;
        }

        /// <inheritdoc />
        public NativeSlice<byte> GetAccessor(int accessorIndex)
        {
            if (Root?.Accessors == null || accessorIndex < 0 || accessorIndex >= Root?.Accessors.Count)
            {
                return new NativeSlice<byte>();
            }
            var accessor = Root.Accessors[accessorIndex];
            return GetBufferView(accessor.bufferView, accessor.byteOffset, accessor.ByteSize);
        }

        async Task<bool> LoadFromUri(Uri url, CancellationToken cancellationToken)
        {

            var download = await m_DownloadProvider.Request(url);
            var success = download.Success;

            if (cancellationToken.IsCancellationRequested)
            {
                return true;
            }

            if (success)
            {

                var gltfBinary = download.IsBinary ?? UriHelper.IsGltfBinary(url);

                if (gltfBinary ?? false)
                {
                    var data = download.Data;
                    download.Dispose();
                    success = await LoadGltfBinaryBuffer(data, url);
                }
                else
                {
                    var text = download.Text;
                    download.Dispose();
                    success = await LoadGltf(text, url);
                }
                if (success)
                {
                    success = await LoadContent();
                }
                success = success && await Prepare();
            }
            else
            {
                m_Logger?.Error(LogCode.Download, download.Error, url.ToString());
            }

            DisposeVolatileData();
            LoadingError = !success;
            LoadingDone = true;
            return success;
        }

        async Task<bool> LoadContent()
        {

            var success = await WaitForBufferDownloads();
            m_DownloadTasks?.Clear();

#if MESHOPT
            if (success) {
                MeshoptDecode();
            }
#endif

            if (m_TextureDownloadTasks != null)
            {
                success = success && await WaitForTextureDownloads();
                m_TextureDownloadTasks.Clear();
            }

#if KTX
            if (m_KtxDownloadTasks != null) {
                success = success && await WaitForKtxDownloads();
                m_KtxDownloadTasks.Clear();
            }
#endif // KTX_UNITY

            return success;
        }

        /// <summary>
        /// De-serializes a glTF JSON string and returns the glTF root schema object.
        /// </summary>
        /// <param name="json">glTF JSON</param>
        /// <returns>De-serialized glTF root object.</returns>
        protected abstract RootBase ParseJson(string json);

        async Task<bool> ParseJsonAndLoadBuffers(string json, Uri baseUri)
        {
            var predictedTime = json.Length / (float)k_JsonParseSpeed;
#if GLTFAST_THREADS && !MEASURE_TIMINGS
            if (m_DeferAgent.ShouldDefer(predictedTime))
            {
                // JSON is larger than threshold
                // => parse in a thread
                Root = await Task.Run(() => ParseJson(json));
            }
            else
#endif
            {
                // Parse immediately on main thread
                Root = ParseJson(json);

                // Loading subsequent buffers and images has to start asap.
                // That's why parsing JSON right away is *very* important.
            }

            if (Root == null)
            {
                Debug.LogError("JsonParsingFailed");
                m_Logger?.Error(LogCode.JsonParsingFailed);
                return false;
            }

            if (!CheckExtensionSupport(Root))
            {
                return false;
            }

            if (Root.Buffers != null)
            {
                var bufferCount = Root.Buffers.Count;
                if (bufferCount > 0)
                {
                    m_Buffers = new byte[bufferCount][];
                    m_BufferHandles = new GCHandle?[bufferCount];
                    m_NativeBuffers = new NativeArray<byte>[bufferCount];
                    m_BinChunks = new GlbBinChunk[bufferCount];
                }

                for (var i = 0; i < bufferCount; i++)
                {
                    var buffer = Root.Buffers[i];
                    if (!string.IsNullOrEmpty(buffer.uri))
                    {
                        if (buffer.uri.StartsWith("data:"))
                        {
                            var decodedBuffer = await DecodeEmbedBufferAsync(
                                buffer.uri,
                                true // usually there's just one buffer and it's time-critical
                            );
                            m_Buffers[i] = decodedBuffer?.Item1;
                            if (m_Buffers[i] == null)
                            {
                                m_Logger?.Error(LogCode.EmbedBufferLoadFailed);
                                return false;
                            }
                        }
                        else
                        {
                            LoadBuffer(i, UriHelper.GetUriString(buffer.uri, baseUri));
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Validates required and used glTF extensions and reports unsupported ones.
        /// </summary>
        /// <param name="gltfRoot"></param>
        /// <returns>False if a required extension is not supported. True otherwise.</returns>
        bool CheckExtensionSupport(RootBase gltfRoot)
        {
            if (!CheckExtensionSupport(gltfRoot.extensionsRequired))
            {
                return false;
            }
            CheckExtensionSupport(gltfRoot.extensionsUsed, false);
            return true;
        }

        bool CheckExtensionSupport(IEnumerable<string> extensions, bool required = true)
        {
            if (extensions == null)
                return true;
            foreach (var ext in extensions)
            {
                var supported = k_SupportedExtensions.Contains(ext);
                if (!supported && m_ImportInstances != null)
                {
                    foreach (var extension in m_ImportInstances)
                    {
                        if (extension.Value.SupportsGltfExtension(ext))
                        {
                            supported = true;
                            break;
                        }
                    }
                }
                if (!supported)
                {
#if !DRACO_UNITY
                    if (ext == ExtensionName.DracoMeshCompression)
                    {
                        m_Logger?.Error(LogCode.PackageMissing, "Draco for Unity", ext);

                    }
#endif
#if !KTX_UNITY
                    if (ext == ExtensionName.TextureBasisUniversal)
                    {
                        m_Logger?.Error(LogCode.PackageMissing, "KTX for Unity", ext);
                    }
                    else
#endif
                    {
                        if (required)
                        {
                            m_Logger?.Error(LogCode.ExtensionUnsupported, ext);
                        }
                        else
                        {
                            m_Logger?.Warning(LogCode.ExtensionUnsupported, ext);
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        async Task<bool> LoadGltf(string json, Uri url)
        {
            var baseUri = UriHelper.GetBaseUri(url);
            var success = await ParseJsonAndLoadBuffers(json, baseUri);
            if (success) await LoadImages(baseUri);
            return success;
        }

        async Task LoadImages(Uri baseUri)
        {

            if (Root.Textures != null && Root.Images != null)
            {

                Profiler.BeginSample("LoadImages.Prepare");

                m_Images = new Texture2D[Root.Images.Count];
                m_ImageFormats = new ImageFormat[Root.Images.Count];

                if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                {

                    m_ImageGamma = new bool[Root.Images.Count];

                    void SetImageGamma(TextureInfoBase txtInfo)
                    {
                        if (
                            txtInfo != null &&
                            txtInfo.index >= 0 &&
                            txtInfo.index < Root.Textures.Count
                        )
                        {
                            var imageIndex = Root.Textures[txtInfo.index].GetImageIndex();
                            m_ImageGamma[imageIndex] = true;
                        }
                    }

                    if (Root.Materials != null)
                    {
                        for (int i = 0; i < Root.Materials.Count; i++)
                        {
                            var mat = Root.Materials[i];
                            if (mat.PbrMetallicRoughness != null)
                            {
                                SetImageGamma(mat.PbrMetallicRoughness.BaseColorTexture);
                            }
                            SetImageGamma(mat.EmissiveTexture);
                            if (mat.Extensions?.KHR_materials_pbrSpecularGlossiness != null)
                            {
                                SetImageGamma(mat.Extensions.KHR_materials_pbrSpecularGlossiness.diffuseTexture);
                                SetImageGamma(mat.Extensions.KHR_materials_pbrSpecularGlossiness.specularGlossinessTexture);
                            }
                        }
                    }
                }

#if KTX
                // Derive image type from texture extension
                for (int i = 0; i < Root.Textures.Count; i++) {
                    var texture = Root.Textures[i];
                    if(texture.IsKtx) {
                        var imgIndex = texture.GetImageIndex();
                        m_ImageFormats[imgIndex] = ImageFormat.Ktx;
                    }
                }
#endif // KTX_UNITY

                // Determine which images need to be readable, because they
                // are applied using different samplers.
                var imageVariants = new HashSet<int>[m_Images.Length];
                foreach (var txt in Root.Textures)
                {
                    var imageIndex = txt.GetImageIndex();
                    if (imageVariants[imageIndex] == null)
                    {
                        imageVariants[imageIndex] = new HashSet<int>();
                    }
                    imageVariants[imageIndex].Add(txt.sampler);
                }

                m_ImageReadable = new bool[m_Images.Length];
                for (int i = 0; i < m_Images.Length; i++)
                {
                    m_ImageReadable[i] = imageVariants[i] != null && imageVariants[i].Count > 1;
                }

                Profiler.EndSample();
                List<Task> imageTasks = null;

                for (int imageIndex = 0; imageIndex < Root.Images.Count; imageIndex++)
                {
                    var img = Root.Images[imageIndex];

                    if (!string.IsNullOrEmpty(img.uri) && img.uri.StartsWith("data:"))
                    {
#if UNITY_IMAGECONVERSION
                        var decodedBufferTask = DecodeEmbedBufferAsync(img.uri);
                        if (imageTasks == null) {
                            imageTasks = new List<Task>();
                        }
                        var imageTask = LoadImageFromBuffer(decodedBufferTask, imageIndex, img);
                        imageTasks.Add(imageTask);
#else
                        m_Logger?.Warning(LogCode.ImageConversionNotEnabled);
#endif
                    }
                    else
                    {
                        ImageFormat imgFormat;
                        if (m_ImageFormats[imageIndex] == ImageFormat.Unknown)
                        {
                            imgFormat = string.IsNullOrEmpty(img.mimeType)
                                ? UriHelper.GetImageFormatFromUri(img.uri)
                                : GetImageFormatFromMimeType(img.mimeType);
                            m_ImageFormats[imageIndex] = imgFormat;
                        }
                        else
                        {
                            imgFormat = m_ImageFormats[imageIndex];
                        }

                        if (imgFormat != ImageFormat.Unknown)
                        {
                            if (img.bufferView < 0)
                            {
                                // Not Inside buffer
                                if (!string.IsNullOrEmpty(img.uri))
                                {
                                    LoadImage(imageIndex, UriHelper.GetUriString(img.uri, baseUri), !m_ImageReadable[imageIndex], imgFormat == ImageFormat.Ktx);
                                }
                                else
                                {
                                    m_Logger?.Error(LogCode.MissingImageURL);
                                }
                            }
                        }
                        else
                        {
                            m_Logger?.Error(LogCode.ImageFormatUnknown, imageIndex.ToString(), img.uri);
                        }
                    }
                }

                if (imageTasks != null)
                {
                    await Task.WhenAll(imageTasks);
                }
            }
        }

#if UNITY_IMAGECONVERSION
        async Task LoadImageFromBuffer(Task<Tuple<byte[],string>> decodeBufferTask, int imageIndex, Image img) {
            var decodedBuffer = await decodeBufferTask;
            await m_DeferAgent.BreakPoint();
            Profiler.BeginSample("LoadImages.FromBase64");
            var data = decodedBuffer.Item1;
            string mimeType = decodedBuffer.Item2;
            var imgFormat = GetImageFormatFromMimeType(mimeType);
            if (data == null || imgFormat == ImageFormat.Unknown) {
                m_Logger?.Error(LogCode.EmbedImageLoadFailed);
                return;
            }

            if (m_ImageFormats[imageIndex] != ImageFormat.Unknown && m_ImageFormats[imageIndex] != imgFormat) {
                m_Logger?.Error(LogCode.EmbedImageInconsistentType, m_ImageFormats[imageIndex].ToString(), imgFormat.ToString());
            }

            m_ImageFormats[imageIndex] = imgFormat;
            if (m_ImageFormats[imageIndex] != ImageFormat.Jpeg && m_ImageFormats[imageIndex] != ImageFormat.PNG) {
                // TODO: support embed KTX textures
                m_Logger?.Error(LogCode.EmbedImageUnsupportedType, m_ImageFormats[imageIndex].ToString());
            }

            // TODO: Investigate alternative: native texture creation in worker thread
            bool forceSampleLinear = m_ImageGamma != null && !m_ImageGamma[imageIndex];
            var txt = CreateEmptyTexture(img, imageIndex, forceSampleLinear);
            txt.LoadImage(data,!m_ImageReadable[imageIndex]);
            m_Images[imageIndex] = txt;
            Profiler.EndSample();
        }
#endif

        async Task<bool> WaitForBufferDownloads()
        {
            if (m_DownloadTasks != null)
            {
                foreach (var downloadPair in m_DownloadTasks)
                {
                    var download = await downloadPair.Value;
                    if (download.Success)
                    {
                        Profiler.BeginSample("GetData");
                        m_Buffers[downloadPair.Key] = download.Data;
                        download.Dispose();
                        Profiler.EndSample();
                    }
                    else
                    {
                        m_Logger?.Error(LogCode.BufferLoadFailed, download.Error, downloadPair.Key.ToString());
                        return false;
                    }
                }
            }

            if (m_Buffers != null)
            {
                Profiler.BeginSample("CreateGlbBinChunks");
                for (int i = 0; i < m_Buffers.Length; i++)
                {
                    if (i == 0 && m_GlbBinChunk.HasValue)
                    {
                        // Already assigned in LoadGltfBinary
                        continue;
                    }
                    var b = m_Buffers[i];
                    if (b != null)
                    {
                        m_BinChunks[i] = new GlbBinChunk(0, (uint)b.Length);
                    }
                }
                Profiler.EndSample();
            }
            return true;
        }

        async Task<bool> WaitForTextureDownloads()
        {
            foreach (var dl in m_TextureDownloadTasks)
            {
                await dl.Value.Load();
                var www = dl.Value.Download;

                if (www == null)
                {
                    m_Logger?.Error(LogCode.TextureDownloadFailed, "?", dl.Key.ToString());
                    return false;
                }

                if (www.Success)
                {
                    var imageIndex = dl.Key;
                    Texture2D txt;
                    // TODO: Loading Jpeg/PNG textures like this creates major frame stalls. Main thread is waiting
                    // on Render thread, which is occupied by Gfx.UploadTextureData for 19 ms for a 2k by 2k texture
                    if (LoadImageFromBytes(imageIndex))
                    {
#if UNITY_IMAGECONVERSION
                        var forceSampleLinear = m_ImageGamma!=null && !m_ImageGamma[imageIndex];
                        txt = CreateEmptyTexture(Root.Images[imageIndex], imageIndex, forceSampleLinear);
                        // TODO: Investigate for NativeArray variant to avoid `www.data`
                        txt.LoadImage(www.Data,!m_ImageReadable[imageIndex]);
#else
                        m_Logger?.Warning(LogCode.ImageConversionNotEnabled);
                        txt = null;
#endif
                    }
                    else
                    {
                        Assert.IsTrue(www is ITextureDownload);
                        txt = ((ITextureDownload)www).Texture;
                        txt.name = GetImageName(Root.Images[imageIndex], imageIndex);
                    }
                    www.Dispose();
                    m_Images[imageIndex] = txt;
                    await m_DeferAgent.BreakPoint();
                }
                else
                {
                    m_Logger?.Error(LogCode.TextureDownloadFailed, www.Error, dl.Key.ToString());
                    www.Dispose();
                    return false;
                }
            }
            return true;
        }


#if KTX
        async Task<bool> WaitForKtxDownloads() {
            var tasks = new Task<bool>[m_KtxDownloadTasks.Count];
            var i = 0;
            foreach( var dl in m_KtxDownloadTasks ) {
                tasks[i] = ProcessKtxDownload(dl.Key, dl.Value);
                i++;
            }
            await Task.WhenAll(tasks);
            foreach (var task in tasks) {
                if (!task.Result) return false;
            }
            return true;
        }

        async Task<bool> ProcessKtxDownload(int imageIndex, Task<IDownload> downloadTask) {
            var www = await downloadTask;
            if(www.Success) {
                var ktxContext = new KtxLoadContext(imageIndex,www.Data);
                www.Dispose();
                var forceSampleLinear = m_ImageGamma!=null && !m_ImageGamma[imageIndex];
                var result = await ktxContext.LoadTexture2D(forceSampleLinear);
                if (result.errorCode == ErrorCode.Success) {
                    m_Images[imageIndex] = result.texture;
                    if (!result.orientation.IsYFlipped())
                    {
                        m_NonFlippedYTextureIndices ??= new HashSet<int>();
                        m_NonFlippedYTextureIndices.Add(imageIndex);
                    }
                    return true;
                }
            } else {
                m_Logger?.Error(LogCode.TextureDownloadFailed,www.Error,imageIndex.ToString());
                www.Dispose();
            }
            return false;
        }
#endif // KTX_UNITY

        void LoadBuffer(int index, Uri url)
        {
            Profiler.BeginSample("LoadBuffer");
            if (m_DownloadTasks == null)
            {
                m_DownloadTasks = new Dictionary<int, Task<IDownload>>();
            }
            m_DownloadTasks.Add(index, m_DownloadProvider.Request(url));
            Profiler.EndSample();
        }

        async Task<Tuple<byte[], string>> DecodeEmbedBufferAsync(string encodedBytes, bool timeCritical = false)
        {
            var predictedTime = encodedBytes.Length / (float)k_Base64DecodeSpeed;
#if MEASURE_TIMINGS
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#elif GLTFAST_THREADS
            if (!timeCritical || m_DeferAgent.ShouldDefer(predictedTime))
            {
                // TODO: Not sure if thread safe? Maybe create a dedicated Report for the thread and merge them afterwards?
                return await Task.Run(() => DecodeEmbedBuffer(encodedBytes, m_Logger));
            }
#endif
            await m_DeferAgent.BreakPoint(predictedTime);
            var decodedBuffer = DecodeEmbedBuffer(encodedBytes, m_Logger);
#if MEASURE_TIMINGS
            stopWatch.Stop();
            var elapsedSeconds = stopWatch.ElapsedMilliseconds / 1000f;
            var relativeDiff = (elapsedSeconds-predictedTime) / predictedTime;
            if (Mathf.Abs(relativeDiff) > .2f) {
                Debug.LogWarning($"Base 64 unexpected duration! diff: {relativeDiff:0.00}% predicted: {predictedTime} sec actual: {elapsedSeconds} sec");
            }
            var throughput = encodedBytes.Length / elapsedSeconds;
            Debug.Log($"Base 64 throughput: {throughput} bytes/sec ({encodedBytes.Length} bytes in {elapsedSeconds} seconds)");
#endif
            return decodedBuffer;
        }

        static Tuple<byte[], string> DecodeEmbedBuffer(string encodedBytes, ICodeLogger logger)
        {
            Profiler.BeginSample("DecodeEmbedBuffer");
            logger?.Warning(LogCode.EmbedSlow);
            var mediaTypeEnd = encodedBytes.IndexOf(';', 5, Math.Min(encodedBytes.Length - 5, 1000));
            if (mediaTypeEnd < 0)
            {
                Profiler.EndSample();
                return null;
            }
            var mimeType = encodedBytes.Substring(5, mediaTypeEnd - 5);
            var tmp = encodedBytes.Substring(mediaTypeEnd + 1, 7);
            if (tmp != "base64,")
            {
                Profiler.EndSample();
                return null;
            }
            var data = Convert.FromBase64String(encodedBytes.Substring(mediaTypeEnd + 8));
            Profiler.EndSample();
            return new Tuple<byte[], string>(data, mimeType);
        }

        void LoadImage(int imageIndex, Uri url, bool nonReadable, bool isKtx)
        {

            Profiler.BeginSample("LoadTexture");

            if (isKtx)
            {
#if KTX
                var downloadTask = m_DownloadProvider.Request(url);
                if(m_KtxDownloadTasks==null) {
                    m_KtxDownloadTasks = new Dictionary<int, Task<IDownload>>();
                }
                m_KtxDownloadTasks.Add(imageIndex, downloadTask);
#else
                m_Logger?.Error(LogCode.PackageMissing, "KTX for Unity", ExtensionName.TextureBasisUniversal);
                Profiler.EndSample();
                return;
#endif // KTX_UNITY
            }
            else
            {
#if UNITY_IMAGECONVERSION
                var downloadTask = LoadImageFromBytes(imageIndex)
                    ? (TextureDownloadBase) new TextureDownload<IDownload>(m_DownloadProvider.Request(url))
                    : new TextureDownload<ITextureDownload>(m_DownloadProvider.RequestTexture(url,nonReadable));
                if(m_TextureDownloadTasks==null) {
                    m_TextureDownloadTasks = new Dictionary<int, TextureDownloadBase>();
                }
                m_TextureDownloadTasks.Add(imageIndex, downloadTask);
#else
                m_Logger?.Warning(LogCode.ImageConversionNotEnabled);
#endif
            }
            Profiler.EndSample();
        }

        /// <summary>
        /// UnityWebRequestTexture always loads Jpegs/PNGs in sRGB color space
        /// without mipmaps. This method figures if this is not desired and the
        /// texture data needs to be loaded from raw bytes.
        /// </summary>
        /// <param name="imageIndex">glTF image index</param>
        /// <returns>True if image texture had to be loaded manually from bytes, false otherwise.</returns>
        bool LoadImageFromBytes(int imageIndex)
        {

#if UNITY_EDITOR
            if (IsEditorImport) {
                // Use the original texture at Editor (asset database) import
                return false;
            }
#endif
#if UNITY_WEBREQUEST_TEXTURE
            var forceSampleLinear = m_ImageGamma != null && !m_ImageGamma[imageIndex];
            return forceSampleLinear || m_Settings.GenerateMipMaps;
#else
            m_Logger?.Warning(LogCode.UnityWebRequestTextureNotEnabled);
            return true;
#endif
        }

        async Task<bool> LoadGltfBinaryBuffer(byte[] bytes, Uri uri = null)
        {
            Profiler.BeginSample("LoadGltfBinary.Phase1");

            if (!GltfGlobals.IsGltfBinary(bytes))
            {
                m_Logger?.Error(LogCode.GltfNotBinary);
                Profiler.EndSample();
                return false;
            }

            uint version = BitConverter.ToUInt32(bytes, 4);
            //uint length = BitConverter.ToUInt32( bytes, 8 );

            if (version != 2)
            {
                m_Logger?.Error(LogCode.GltfUnsupportedVersion, version.ToString());
                Profiler.EndSample();
                return false;
            }

            int index = 12; // first chunk header

            var baseUri = UriHelper.GetBaseUri(uri);

            Profiler.EndSample();

            while (index < bytes.Length)
            {

                if (index + 8 > bytes.Length)
                {
                    m_Logger?.Error(LogCode.ChunkIncomplete);
                    return false;
                }

                uint chLength = BitConverter.ToUInt32(bytes, index);
                index += 4;
                uint chType = BitConverter.ToUInt32(bytes, index);
                index += 4;

                if (index + chLength > bytes.Length)
                {
                    m_Logger?.Error(LogCode.ChunkIncomplete);
                    return false;
                }

                if (chType == (uint)ChunkFormat.Binary)
                {
                    Assert.IsFalse(m_GlbBinChunk.HasValue); // There can only be one binary chunk
                    m_GlbBinChunk = new GlbBinChunk(index, chLength);
                }
                else if (chType == (uint)ChunkFormat.Json)
                {
                    Assert.IsNull(Root);

                    Profiler.BeginSample("GetJSON");
                    string json = System.Text.Encoding.UTF8.GetString(bytes, index, (int)chLength);
                    Profiler.EndSample();

                    var success = await ParseJsonAndLoadBuffers(json, baseUri);

                    if (!success)
                    {
                        return false;
                    }
                }
                else
                {
                    m_Logger?.Error(LogCode.ChunkUnknown, chType.ToString());
                    return false;
                }

                index += (int)chLength;
            }

            if (Root == null)
            {
                m_Logger?.Error(LogCode.ChunkJsonInvalid);
                return false;
            }

            if (m_GlbBinChunk.HasValue && m_BinChunks != null)
            {
                m_BinChunks[0] = m_GlbBinChunk.Value;
                m_Buffers[0] = bytes;
            }
            await LoadImages(baseUri);
            return true;
        }

        byte[] GetBuffer(int index)
        {
            return m_Buffers[index];
        }

        NativeSlice<byte> GetBufferView(int bufferViewIndex, int offset = 0, int length = 0)
        {
            var bufferView = Root.BufferViews[bufferViewIndex];
#if MESHOPT
            if (bufferView.Extensions?.EXT_meshopt_compression != null) {
                var fullSlice = m_MeshoptBufferViews[bufferViewIndex];
                if (offset == 0 && length <= 0) {
                    return fullSlice;
                }
                Assert.IsTrue(offset >= 0);
                if (length <= 0) {
                    length = fullSlice.Length - offset;
                }
                Assert.IsTrue(offset+length <= fullSlice.Length);
                return  new NativeSlice<byte>(fullSlice,offset,length);
            }
#endif
            return GetBufferViewSlice(bufferView, offset, length);
        }

        unsafe NativeSlice<byte> GetBufferViewSlice(
            IBufferView bufferView,
            int offset = 0,
            int length = 0
            )
        {
            Assert.IsTrue(offset >= 0);
            if (length <= 0)
            {
                length = bufferView.ByteLength - offset;
            }
            Assert.IsTrue(offset + length <= bufferView.ByteLength);

            var bufferIndex = bufferView.Buffer;
            if (!m_NativeBuffers[bufferIndex].IsCreated)
            {
                Profiler.BeginSample("ConvertToNativeArray");
                var buffer = GetBuffer(bufferIndex);
                m_BufferHandles[bufferIndex] = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                fixed (void* bufferAddress = &(buffer[0]))
                {
                    m_NativeBuffers[bufferIndex] = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(bufferAddress, buffer.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    var safetyHandle = AtomicSafetyHandle.Create();
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(array: ref m_NativeBuffers[bufferIndex], safetyHandle);
#endif
                }
                Profiler.EndSample();
            }
            var chunk = m_BinChunks[bufferIndex];
            var nativeBuffer = m_NativeBuffers[bufferIndex];
            var totalOffset = chunk.Start + bufferView.ByteOffset + offset;
            Assert.IsTrue(bufferView.ByteOffset + offset <= chunk.Length);
            Assert.IsTrue(totalOffset + length <= nativeBuffer.Length);
            return new NativeSlice<byte>(
                m_NativeBuffers[bufferIndex],
                totalOffset,
                length
                );
        }

#if MESHOPT
        void MeshoptDecode() {
            if(Root.BufferViews!=null) {
                List<JobHandle> jobHandlesList = null;
                for (var i = 0; i < Root.BufferViews.Count; i++) {
                    var bufferView = Root.BufferViews[i];
                    if (bufferView.Extensions?.EXT_meshopt_compression != null) {
                        var meshopt = bufferView.Extensions.EXT_meshopt_compression;
                        if (jobHandlesList == null) {
                            m_MeshoptBufferViews = new Dictionary<int, NativeArray<byte>>();
                            jobHandlesList = new List<JobHandle>(Root.BufferViews.Count);
                            m_MeshoptReturnValues = new NativeArray<int>(Root.BufferViews.Count, Allocator.TempJob);
                        }

                        var arr = new NativeArray<byte>(meshopt.count * meshopt.byteStride, Allocator.Persistent);

                        var origBufferView = GetBufferViewSlice(meshopt);

                        var jobHandle = Decode.DecodeGltfBuffer(
                            new NativeSlice<int>(m_MeshoptReturnValues,i,1),
                            arr,
                            meshopt.count,
                            meshopt.byteStride,
                            origBufferView,
                            meshopt.GetMode(),
                            meshopt.GetFilter()
                        );
                        jobHandlesList.Add(jobHandle);
                        m_MeshoptBufferViews[i] = arr;
                    }
                }

                if (jobHandlesList != null) {
                    using (var jobHandles = new NativeArray<JobHandle>(jobHandlesList.ToArray(), Allocator.Temp)) {
                        m_MeshoptJobHandle = JobHandle.CombineDependencies(jobHandles);
                    }
                }
            }
        }

        async Task<bool> WaitForMeshoptDecode() {
            var success = true;
            if (m_MeshoptBufferViews != null) {
                while (!m_MeshoptJobHandle.IsCompleted) {
                    await Task.Yield();
                }
                m_MeshoptJobHandle.Complete();

                foreach (var returnValue in m_MeshoptReturnValues) {
                    success &= returnValue == 0;
                }
                m_MeshoptReturnValues.Dispose();
            }
            return success;
        }

#endif // MESHOPT

        async Task<bool> Prepare()
        {
            if (Root.Meshes != null)
            {
                m_MeshPrimitiveIndex = new int[Root.Meshes.Count + 1];
            }

            m_Resources = new List<UnityEngine.Object>();

            if (Root.Images != null && Root.Textures != null && Root.Materials != null)
            {
                if (m_Images == null)
                {
                    m_Images = new Texture2D[Root.Images.Count];
                }
                else
                {
                    Assert.AreEqual(m_Images.Length, Root.Images.Count);
                }
                m_ImageCreateContexts = new List<ImageCreateContext>();
#if KTX
                await
#endif
                CreateTexturesFromBuffers(Root.Images, Root.BufferViews, m_ImageCreateContexts);
            }
            await m_DeferAgent.BreakPoint();

            // RedundantAssignment potentially becomes necessary when MESHOPT is not available
            // ReSharper disable once RedundantAssignment
            var success = true;

#if MESHOPT
            success = await WaitForMeshoptDecode();
            if (!success) return false;
#endif

            if (Root.Accessors != null)
            {
                success = await LoadAccessorData(Root);
                await m_DeferAgent.BreakPoint();

                while (!m_AccessorJobsHandle.IsCompleted)
                {
                    await Task.Yield();
                }
                m_AccessorJobsHandle.Complete();
                foreach (var ad in m_AccessorData)
                {
                    ad?.Unpin();
                }
            }
            if (!success) return success;

            if (Root.Meshes != null)
            {
                await CreatePrimitiveContexts(Root);
            }

#if KTX
            if(m_KtxLoadContextsBuffer!=null) {
                await ProcessKtxLoadContexts();
            }
#endif // KTX_UNITY

            if (m_ImageCreateContexts != null)
            {
                var imageCreateContextsLeft = true;
                while (imageCreateContextsLeft)
                {
                    var loadedAny = false;
                    for (int i = m_ImageCreateContexts.Count - 1; i >= 0; i--)
                    {
                        var jh = m_ImageCreateContexts[i];
                        if (jh.jobHandle.IsCompleted)
                        {
                            jh.jobHandle.Complete(); //complete the memcopy job 

                            //Debug.Log($"Loading Texture {CacheID}:{jh.imageIndex}");
#if UNITY_IMAGECONVERSION
                            //m_Images[jh.imageIndex].LoadImage(jh.buffer,!m_ImageReadable[jh.imageIndex]);
                            LoadImage(jh, m_Images[jh.imageIndex]);
#endif
                            jh.gcHandle.Free();
                            m_ImageCreateContexts.RemoveAt(i);
                            loadedAny = true;
                            await m_DeferAgent.BreakPoint();
                        }
                    }
                    imageCreateContextsLeft = m_ImageCreateContexts.Count > 0;
                    if (!loadedAny && imageCreateContextsLeft)
                    {
                        await Task.Yield();
                    }
                }
                m_ImageCreateContexts = null;
            }

            if (m_Images != null && Root.Textures != null)
            {
                SamplerKey defaultKey = new SamplerKey(new Sampler());
                m_Textures = new Texture2D[Root.Textures.Count];
                var imageVariants = new Dictionary<SamplerKey, Texture2D>[m_Images.Length];
                for (int textureIndex = 0; textureIndex < Root.Textures.Count; textureIndex++)
                {
                    var txt = Root.Textures[textureIndex];
                    SamplerKey key;
                    Sampler sampler = null;
                    if (txt.sampler >= 0)
                    {
                        sampler = Root.Samplers[txt.sampler];
                        key = new SamplerKey(sampler);
                    }
                    else
                    {
                        key = defaultKey;
                    }

                    var imageIndex = txt.GetImageIndex();
                    var img = m_Images[imageIndex];
                    if (imageVariants[imageIndex] == null)
                    {
                        if (txt.sampler >= 0)
                        {
                            sampler.Apply(img, m_Settings.DefaultMinFilterMode, m_Settings.DefaultMagFilterMode);
                        }
                        imageVariants[imageIndex] = new Dictionary<SamplerKey, Texture2D>();
                        imageVariants[imageIndex][key] = img;
                        m_Textures[textureIndex] = img;
                    }
                    else
                    {
                        if (imageVariants[imageIndex].TryGetValue(key, out var imgVariant))
                        {
                            m_Textures[textureIndex] = imgVariant;
                        }
                        else
                        {
                            var newImg = UnityEngine.Object.Instantiate(img);
                            m_Resources.Add(newImg);
#if DEBUG
                            newImg.name = $"{img.name}_sampler{txt.sampler}";
                            m_Logger?.Warning(LogCode.ImageMultipleSamplers,imageIndex.ToString());
#endif
                            sampler?.Apply(newImg, m_Settings.DefaultMinFilterMode, m_Settings.DefaultMagFilterMode);
                            imageVariants[imageIndex][key] = newImg;
                            m_Textures[textureIndex] = newImg;
                        }
                    }
                }
            }

            if (Root.Materials != null)
            {
                m_Materials = new UnityEngine.Material[Root.Materials.Count];
                for (var i = 0; i < m_Materials.Length; i++)
                {
                    await m_DeferAgent.BreakPoint(.0001f);
                    Profiler.BeginSample("GenerateMaterial");
                    m_MaterialGenerator.SetLogger(m_Logger);
                    var pointsSupport = GetMaterialPointsSupport(i);
                    var material = m_MaterialGenerator.GenerateMaterial(
                        Root.Materials[i],
                        this,
                        pointsSupport
                    );
                    m_Materials[i] = material;
                    m_MaterialGenerator.SetLogger(null);
                    Profiler.EndSample();
                }
            }
            await m_DeferAgent.BreakPoint();

            if (m_PrimitiveContexts != null)
            {
                for (int i = 0; i < m_PrimitiveContexts.Length; i++)
                {
                    var primitiveContext = m_PrimitiveContexts[i];
                    if (primitiveContext == null) continue;
                    while (!primitiveContext.IsCompleted)
                    {
                        await Task.Yield();
                    }
                }
                await m_DeferAgent.BreakPoint();

                await AssignAllAccessorData(Root);

                for (int i = 0; i < m_PrimitiveContexts.Length; i++)
                {
                    var primitiveContext = m_PrimitiveContexts[i];
                    while (!primitiveContext.IsCompleted)
                    {
                        await Task.Yield();
                    }
                    var primitive = await primitiveContext.CreatePrimitive();
                    // The import failed :\
                    // await defaultDeferAgent.BreakPoint();

                    if (primitive.HasValue)
                    {
                        m_Primitives[primitiveContext.PrimitiveIndex] = primitive.Value;
                        m_Resources.Add(primitive.Value.mesh);
                    }
                    else
                    {
                        success = false;
                        break;
                    }

                    await m_DeferAgent.BreakPoint();
                }
            }

#if UNITY_ANIMATION
            if (Root.HasAnimation) {
                if (m_Settings.NodeNameMethod != NameImportMethod.OriginalUnique) {
                    m_Logger?.Info(LogCode.NamingOverride);
                    m_Settings.NodeNameMethod = NameImportMethod.OriginalUnique;
                }
            }
#endif

            int[] parentIndex = null;

            var skeletonMissing = Root.IsASkeletonMissing();

            if (Root.Nodes != null && Root.Nodes.Count > 0)
            {
                if (m_Settings.NodeNameMethod == NameImportMethod.OriginalUnique)
                {
                    parentIndex = CreateUniqueNames();
                }
                else if (skeletonMissing)
                {
                    parentIndex = GetParentIndices();
                }
                if (skeletonMissing)
                {
                    for (int skinId = 0; skinId < Root.Skins.Count; skinId++)
                    {
                        var skin = Root.Skins[skinId];
                        if (skin.skeleton < 0)
                        {
                            skin.skeleton = GetLowestCommonAncestorNode(skin.joints, parentIndex);
                        }
                    }
                }
            }

#if UNITY_ANIMATION
            if (Root.HasAnimation && m_Settings.AnimationMethod != AnimationMethod.None) {

                m_AnimationClips = new AnimationClip[Root.Animations.Count];
                for (var i = 0; i < Root.Animations.Count; i++) {
                    var animation = Root.Animations[i];
                    m_AnimationClips[i] = new AnimationClip();
                    m_AnimationClips[i].name = animation.name ?? $"Clip_{i}";

                    // Legacy Animation requirement
                    m_AnimationClips[i].legacy = m_Settings.AnimationMethod == AnimationMethod.Legacy;
                    m_AnimationClips[i].wrapMode = WrapMode.Loop;

                    for (int j = 0; j < animation.Channels.Count; j++) {
                        var channel = animation.Channels[j];
                        if (channel.sampler < 0 || channel.sampler >= animation.Samplers.Count) {
                            m_Logger?.Error(LogCode.AnimationChannelSamplerInvalid, j.ToString());
                            continue;
                        }
                        var sampler = animation.Samplers[channel.sampler];
                        if (channel.Target.node < 0 || channel.Target.node >= Root.Nodes.Count) {
                            m_Logger?.Error(LogCode.AnimationChannelNodeInvalid, j.ToString());
                            continue;
                        }

                        var path = AnimationUtils.CreateAnimationPath(channel.Target.node,m_NodeNames,parentIndex);

                        var times = ((AccessorNativeData<float>) m_AccessorData[sampler.input]).data;

                        switch (channel.Target.GetPath()) {
                            case AnimationChannel.Path.Translation: {
                                var values= ((AccessorNativeData<Vector3>) m_AccessorData[sampler.output]).data;
                                AnimationUtils.AddTranslationCurves(m_AnimationClips[i], path, times, values, sampler.GetInterpolationType());
                                break;
                            }
                            case AnimationChannel.Path.Rotation: {
                                var values= ((AccessorNativeData<Quaternion>) m_AccessorData[sampler.output]).data;
                                AnimationUtils.AddRotationCurves(m_AnimationClips[i], path, times, values, sampler.GetInterpolationType());
                                break;
                            }
                            case AnimationChannel.Path.Scale: {
                                var values= ((AccessorNativeData<Vector3>) m_AccessorData[sampler.output]).data;
                                AnimationUtils.AddScaleCurves(m_AnimationClips[i], path, times, values, sampler.GetInterpolationType());
                                break;
                            }
                            case AnimationChannel.Path.Weights: {
                                var values= ((AccessorNativeData<float>) m_AccessorData[sampler.output]).data;
                                var node = Root.Nodes[channel.Target.node];
                                if (node.mesh < 0 || node.mesh >= Root.Meshes.Count) {
                                    break;
                                }
                                var mesh = Root.Meshes[node.mesh];
                                AnimationUtils.AddMorphTargetWeightCurves(
                                    m_AnimationClips[i],
                                    path,
                                    times,
                                    values,
                                    sampler.GetInterpolationType(),
                                    mesh.Extras?.targetNames
                                    );

                                // HACK BEGIN:
                                // Since meshes with multiple primitives that are not using
                                // identical vertex buffers are split up into separate Unity
                                // Meshes. Because of this, we have to duplicate the animation
                                // curves, so that all primitives are animated.
                                // TODO: Refactor primitive sub-meshing and remove this hack
                                // https://github.com/atteneder/glTFast/issues/153
                                var meshName = string.IsNullOrEmpty(mesh.name) ? k_PrimitiveName : mesh.name;
                                var primitiveCount = m_MeshPrimitiveIndex[node.mesh + 1] - m_MeshPrimitiveIndex[node.mesh];
                                for (var k = 1; k < primitiveCount; k++) {
                                    var primitiveName = $"{meshName}_{k}";
                                    AnimationUtils.AddMorphTargetWeightCurves(
                                        m_AnimationClips[i],
                                        $"{path}/{primitiveName}",
                                        times,
                                        values,
                                        sampler.GetInterpolationType(),
                                        mesh.Extras?.targetNames
                                    );
                                }
                                // HACK END
                                break;
                            }
                            case AnimationChannel.Path.Pointer:
                                m_Logger?.Warning(LogCode.AnimationTargetPathUnsupported,channel.Target.GetPath().ToString());
                                break;
                            default:
                                m_Logger?.Error(LogCode.AnimationTargetPathUnsupported,channel.Target.GetPath().ToString());
                                break;
                        }
                    }
                }
            }
#endif

            // Dispose all accessor data buffers, except the ones needed for instantiation
            if (m_AccessorData != null)
            {
                for (var index = 0; index < m_AccessorData.Length; index++)
                {
                    if ((m_AccessorUsage[index] & AccessorUsage.RequiredForInstantiation) == 0)
                    {
                        m_AccessorData[index]?.Dispose();
                        m_AccessorData[index] = null;
                    }
                }
            }
            return success;
        }

        private string GetTextureCachePath(int imageIndex)
        {
            var texCachePath = Path.Combine(Application.persistentDataPath, "TextureCache", $"{CacheID}_{imageIndex}.bin");
            return texCachePath;
        }

        bool LoadCachedTexture(int imageIndex, Texture2D tex)
        {
            var texCachePath = GetTextureCachePath(imageIndex);

            Directory.CreateDirectory(Path.GetDirectoryName(texCachePath));

            return LoadTextureCache(tex, texCachePath);                
        }

        void LoadImage(ImageCreateContext jh, Texture2D tex)
        {
            if (LoadCachedTexture(jh.imageIndex, tex))
                return;

            tex.LoadImage(jh.buffer, false);
            tex.Compress(true);
            //Debug.Log(tex.format);

            SaveTextureCache(tex, GetTextureCachePath(jh.imageIndex));
        }

        public static void SaveTextureCache(Texture2D tex, string path)
        {
            var rawTexData = tex.GetRawTextureData();

            using var stream = new FileStream(path, FileMode.CreateNew);
            using var writer = new BinaryWriter(stream);

            writer.Write("TXCACHEV01");
            writer.Write(tex.width);
            writer.Write(tex.height);
            writer.Write(tex.mipmapCount);
            writer.Write((int)tex.format);
            writer.Write(rawTexData.Length);
            writer.Write(rawTexData);
        }

        //private static byte[] _buffer;

        //public static void ReleaseBuffer()
        //{
        //    _buffer = null;
        //}

        public static bool LoadTextureCache(Texture2D tex, string path)
        {
            try
            {
                if (!File.Exists(path))
                    return false;

                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64);
                using var reader = new BinaryReader(stream);

                var header = reader.ReadString();
                if (header != "TXCACHEV01")
                    return false;

                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                int mipCount = reader.ReadInt32();
                TextureFormat format = (TextureFormat)reader.ReadInt32();
                int length = reader.ReadInt32();

                //if (_buffer == null || _buffer.Length < length)
                //{
                //    _buffer = new byte[Math.Max(length, 100 * 1024 * 1024)];
                //}

                //var rawTexData = reader.ReadBytes(length);
                using var buffer = new NativeArray<byte>(length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var bytesRead = stream.Read(buffer.AsSpan());

                //var rawTexData = new byte[length];
                //var bytesRead = await stream.ReadAsync(rawTexData, 0, length);

                if (bytesRead != length)
                {
                    Debug.LogError($"Error reading texture cache data for {path}");
                    return false;
                }

                tex.Reinitialize(width, height, format, mipCount > 1 ? true : false);
                //tex.LoadRawTextureData(rawTexData);
                tex.LoadRawTextureData<byte>(buffer);
                //tex.LoadRawTextureData(_buffer, )
                tex.Apply();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error reading texture cache {path} : {ex.Message}");
                return false;
            }

            return true;

        }

        void SetMaterialPointsSupport(int materialIndex)
        {
            Assert.IsNotNull(Root?.Materials);
            Assert.IsTrue(materialIndex >= 0);
            Assert.IsTrue(materialIndex < Root.Materials.Count);
            if (m_MaterialPointsSupport == null)
            {
                m_MaterialPointsSupport = new HashSet<int>();
            }
            m_MaterialPointsSupport.Add(materialIndex);
        }

        bool GetMaterialPointsSupport(int materialIndex)
        {
            if (m_MaterialPointsSupport != null)
            {
                Assert.IsNotNull(Root?.Materials);
                Assert.IsTrue(materialIndex >= 0);
                Assert.IsTrue(materialIndex < Root.Materials.Count);
                return m_MaterialPointsSupport.Contains(materialIndex);
            }
            return false;
        }

        /// <summary>
        /// glTF nodes have no requirement to be named or have specific names.
        /// Some Unity systems like animation and importers require unique
        /// names for Nodes with the same parent. For each node this method creates
        /// names that are:
        /// - Not empty
        /// - Unique amongst nodes with identical parent node
        /// </summary>
        /// <returns>Array containing each node's parent node index (or -1 for root nodes)</returns>
        int[] CreateUniqueNames()
        {
            m_NodeNames = new string[Root.Nodes.Count];
            var parentIndex = new int[Root.Nodes.Count];

            for (var nodeIndex = 0; nodeIndex < Root.Nodes.Count; nodeIndex++)
            {
                parentIndex[nodeIndex] = -1;
            }

            var childNames = new HashSet<string>();

            for (var nodeIndex = 0; nodeIndex < Root.Nodes.Count; nodeIndex++)
            {
                var node = Root.Nodes[nodeIndex];
                if (node.children != null)
                {
                    childNames.Clear();
                    foreach (var child in node.children)
                    {
                        parentIndex[child] = nodeIndex;
                        m_NodeNames[child] = GetUniqueNodeName(Root, child, childNames);
                    }
                }
            }

            for (int sceneId = 0; sceneId < Root.Scenes.Count; sceneId++)
            {
                childNames.Clear();
                var scene = Root.Scenes[sceneId];
                if (scene.nodes != null)
                {
                    foreach (var nodeIndex in scene.nodes)
                    {
                        m_NodeNames[nodeIndex] = GetUniqueNodeName(Root, nodeIndex, childNames);
                    }
                }
            }

            return parentIndex;
        }

        static string GetUniqueNodeName(RootBase gltf, uint index, ICollection<string> excludeNames)
        {
            if (gltf.Nodes == null || index >= gltf.Nodes.Count) return null;
            var name = gltf.Nodes[(int)index].name;
            if (string.IsNullOrWhiteSpace(name))
            {
                var meshIndex = gltf.Nodes[(int)index].mesh;
                if (meshIndex >= 0)
                {
                    name = gltf.Meshes[meshIndex].name;
                }
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"Node-{index}";
            }

            if (excludeNames != null)
            {
                if (excludeNames.Contains(name))
                {
                    var i = 0;
                    string extName;
                    do
                    {
                        extName = $"{name}_{i++}";
                    } while (excludeNames.Contains(extName));
                    excludeNames.Add(extName);
                    return extName;
                }
                excludeNames.Add(name);
            }
            return name;
        }

        /// <summary>
        /// Free up volatile loading resources
        /// </summary>
        void DisposeVolatileData()
        {

            if (m_VertexAttributes != null)
            {
                foreach (var vac in m_VertexAttributes.Values)
                {
                    vac.Dispose();
                }
            }
            m_VertexAttributes = null;

            // Unpin managed buffer arrays
            if (m_BufferHandles != null)
            {
                foreach (var t in m_BufferHandles)
                {
                    t?.Free();
                }
            }
            m_BufferHandles = null;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if(m_NativeBuffers!=null) {
                foreach (var nativeBuffer in m_NativeBuffers)
                {
                    if(nativeBuffer.IsCreated) {
                        var safetyHandle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(nativeBuffer);
                        AtomicSafetyHandle.Release(safetyHandle);
                    }
                }
            }
#endif
            m_NativeBuffers = null;

            m_Buffers = null;

            m_BinChunks = null;

            m_DownloadTasks = null;
            m_TextureDownloadTasks = null;

            m_AccessorUsage = null;
            m_PrimitiveContexts = null;
            m_MeshPrimitiveCluster = null;
            m_ImageCreateContexts = null;
            m_Images = null;
            m_ImageFormats = null;
            m_ImageReadable = null;
            m_ImageGamma = null;
            m_GlbBinChunk = null;
            m_MaterialPointsSupport = null;

#if MESHOPT
            if(m_MeshoptBufferViews!=null) {
                foreach (var nativeBuffer in m_MeshoptBufferViews.Values) {
                    nativeBuffer.Dispose();
                }
                m_MeshoptBufferViews = null;
            }
            if (m_MeshoptReturnValues.IsCreated) {
                m_MeshoptReturnValues.Dispose();
            }
#endif
        }

        async Task InstantiateSceneInternal(RootBase gltf, IInstantiator instantiator, int sceneId)
        {
            if (m_ImportInstances != null)
            {
                foreach (var extension in m_ImportInstances)
                {
                    extension.Value.Inject(instantiator);
                }
            }

            async Task IterateNodes(uint nodeIndex, uint? parentIndex, Action<uint, uint?> callback)
            {
                var node = Root.Nodes[(int)nodeIndex];
                callback(nodeIndex, parentIndex);
                await m_DeferAgent.BreakPoint();
                if (node.children != null)
                {
                    foreach (var child in node.children)
                    {
                        await IterateNodes(child, nodeIndex, callback);
                    }
                }
            }

            void CreateHierarchy(uint nodeIndex, uint? parentIndex)
            {

                Profiler.BeginSample("CreateHierarchy");
                var node = Root.Nodes[(int)nodeIndex];
                node.GetTransform(out var position, out var rotation, out var scale);
                instantiator.CreateNode(nodeIndex, parentIndex, position, rotation, scale);
                Profiler.EndSample();
            }

            void PopulateHierarchy(uint nodeIndex, uint? parentIndex)
            {

                Profiler.BeginSample("PopulateHierarchy");
                var node = Root.Nodes[(int)nodeIndex];
                var goName = m_NodeNames == null ? node.name : m_NodeNames[nodeIndex];

                if (node.mesh >= 0)
                {
                    var end = m_MeshPrimitiveIndex[node.mesh + 1];
                    var primitiveCount = 0;
                    for (var i = m_MeshPrimitiveIndex[node.mesh]; i < end; i++)
                    {
                        var primitive = m_Primitives[i];
                        var mesh = primitive.mesh;
                        var meshName = string.IsNullOrEmpty(mesh.name) ? null : mesh.name;
                        // Fallback name for Node is first valid Mesh name
                        goName = goName ?? meshName;
                        uint[] joints = null;
                        uint? rootJoint = null;

                        if (mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.BlendIndices))
                        {
                            if (node.skin >= 0)
                            {
                                var skin = gltf.Skins[node.skin];
                                // TODO: see if this can be moved to mesh creation phase / before instantiation
                                mesh.bindposes = GetBindPoses(node.skin);
                                if (skin.skeleton >= 0)
                                {
                                    rootJoint = (uint)skin.skeleton;
                                }
                                joints = skin.joints;
                            }
                            else
                            {
                                m_Logger?.Warning(LogCode.SkinMissing);
                            }
                        }

                        var meshInstancing = node.Extensions?.EXT_mesh_gpu_instancing;

                        var primitiveName =
                            primitiveCount > 0
                                ? $"{meshName ?? k_PrimitiveName}_{primitiveCount}"
                                : meshName ?? k_PrimitiveName;

                        if (meshInstancing == null)
                        {
                            instantiator.AddPrimitive(
                                nodeIndex,
                                primitiveName,
                                primitive,
                                joints,
                                rootJoint,
                                gltf.Meshes[node.mesh].weights,
                                primitiveCount
                            );
                        }
                        else
                        {

                            var hasTranslations = meshInstancing.attributes.TRANSLATION > -1;
                            var hasRotations = meshInstancing.attributes.ROTATION > -1;
                            var hasScales = meshInstancing.attributes.SCALE > -1;

                            NativeArray<Vector3>? positions = null;
                            NativeArray<Quaternion>? rotations = null;
                            NativeArray<Vector3>? scales = null;
                            uint instanceCount = 0;

                            if (hasTranslations)
                            {
                                positions = ((AccessorNativeData<Vector3>)m_AccessorData[meshInstancing.attributes.TRANSLATION]).data;
                                instanceCount = (uint)positions.Value.Length;
                            }

                            if (hasRotations)
                            {
                                rotations = ((AccessorNativeData<Quaternion>)m_AccessorData[meshInstancing.attributes.ROTATION]).data;
                                instanceCount = (uint)rotations.Value.Length;
                            }

                            if (hasScales)
                            {
                                scales = ((AccessorNativeData<Vector3>)m_AccessorData[meshInstancing.attributes.SCALE]).data;
                                instanceCount = (uint)scales.Value.Length;
                            }

                            instantiator.AddPrimitiveInstanced(
                                nodeIndex,
                                primitiveName,
                                m_Primitives[i],
                                instanceCount,
                                positions,
                                rotations,
                                scales,
                                primitiveCount
                            );
                        }

                        primitiveCount++;
                    }
                }

                instantiator.SetNodeName(nodeIndex, goName);

                if (node.camera >= 0
                    && gltf.Cameras != null
                    && node.camera < gltf.Cameras.Count
                    )
                {
                    instantiator.AddCamera(nodeIndex, (uint)node.camera);
                }

                if (node.Extensions?.KHR_lights_punctual != null && gltf.Extensions?.KHR_lights_punctual?.lights != null)
                {
                    var lightIndex = node.Extensions.KHR_lights_punctual.light;
                    if (lightIndex < gltf.Extensions.KHR_lights_punctual.lights.Length)
                    {
                        instantiator.AddLightPunctual(nodeIndex, (uint)lightIndex);
                    }
                }

                Profiler.EndSample();
            }

            var scene = Root.Scenes[sceneId];

            instantiator.BeginScene(scene.name, scene.nodes);
#if UNITY_ANIMATION
            instantiator.AddAnimation(m_AnimationClips);
#endif

            if (scene.nodes != null)
            {
                foreach (var nodeId in scene.nodes)
                {
                    await IterateNodes(nodeId, null, CreateHierarchy);
                }
                foreach (var nodeId in scene.nodes)
                {
                    await IterateNodes(nodeId, null, PopulateHierarchy);
                }
            }

            instantiator.EndScene(scene.nodes);
        }

        /// <summary>
        /// Given a set of nodes in a hierarchy, this method finds the
        /// lowest common ancestor node.
        /// </summary>
        /// <param name="nodes">Set of nodes</param>
        /// <param name="parentIndex">Dictionary of nodes' parent indices</param>
        /// <returns>Lowest common ancestor node of all provided nodes. -1 if it was not found</returns>
        static int GetLowestCommonAncestorNode(IEnumerable<uint> nodes, IReadOnlyList<int> parentIndex)
        {

            List<int> chain = null;
            var commonAncestor = -1;

            bool CompareTo(int nodeId)
            {
                var nodeChain = new List<int>();

                var currNodeId = nodeId;

                while (currNodeId >= 0)
                {
                    if (currNodeId == commonAncestor)
                    {
                        return true;
                    }
                    nodeChain.Insert(0, currNodeId);
                    currNodeId = parentIndex[currNodeId];
                }

                if (chain == null)
                {
                    chain = nodeChain;
                }
                else
                {
                    var depth = math.min(chain.Count, nodeChain.Count);
                    for (var i = 0; i < depth; i++)
                    {
                        if (chain[i] != nodeChain[i])
                        {
                            if (i > 0)
                            {
                                chain.RemoveRange(i, chain.Count - i);
                                break;
                            }
                            return false;
                        }
                    }
                }

                commonAncestor = chain[chain.Count - 1];
                return true;
            }

            foreach (var nodeId in nodes)
            {
                if (!CompareTo((int)nodeId))
                {
                    return -1;
                }
            }

            // foreach (var nodeId in nodes) {
            //     if (commonAncestor == nodeId) {
            //         // A joint cannot be the root, so use its parent instead
            //         commonAncestor = parentIndex[commonAncestor];
            //         break;
            //     }
            // }

            return commonAncestor;
        }

        int[] GetParentIndices()
        {
            var parentIndex = new int[Root.Nodes.Count];
            for (var i = 0; i < parentIndex.Length; i++)
            {
                parentIndex[i] = -1;
            }

            for (var i = 0; i < Root.Nodes.Count; i++)
            {
                if (Root.Nodes[i].children != null)
                {
                    foreach (var child in Root.Nodes[i].children)
                    {
                        parentIndex[child] = i;
                    }
                }
            }

            return parentIndex;
        }

        /// <summary>
        /// Reinterprets a NativeSlice&lt;byte&gt; to another type of NativeArray.
        /// TODO: Remove once Unity.Collections supports this for NativeSlice (NativeArray only atm)
        /// </summary>
        /// <param name="slice"></param>
        /// <param name="count">Target type element count</param>
        /// <param name="offset">Byte offset into the slice</param>
        /// <typeparam name="T">Target type</typeparam>
        /// <returns></returns>
        static unsafe NativeArray<T> Reinterpret<T>(NativeSlice<byte> slice, int count, int offset = 0) where T : struct
        {
            var address = (byte*)slice.GetUnsafeReadOnlyPtr();
            var result = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(address + offset, count, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var safetyHandle = AtomicSafetyHandle.Create();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(array: ref result, safetyHandle);
#endif
            return result;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        static void ReleaseReinterpret<T>(NativeArray<T> array) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var safetyHandle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(array);
            AtomicSafetyHandle.Release(safetyHandle);
#endif
        }

#if KTX
        async Task
#else
        void
#endif
        CreateTexturesFromBuffers(
            IReadOnlyList<Image> srcImages,
            IReadOnlyList<BufferViewBase> bufferViews,
            ICollection<ImageCreateContext> contexts
        )
        {
            for (int i = 0; i < m_Images.Length; i++)
            {
                Profiler.BeginSample("CreateTexturesFromBuffers.ImageFormat");
                if (m_Images[i] != null)
                {
                    m_Resources.Add(m_Images[i]);
                }
                var img = srcImages[i];
                ImageFormat imgFormat = m_ImageFormats[i];
                if (imgFormat == ImageFormat.Unknown)
                {
                    imgFormat = string.IsNullOrEmpty(img.mimeType)
                        // Image is missing mime type
                        // try to determine type by file extension
                        ? UriHelper.GetImageFormatFromUri(img.uri)
                        : GetImageFormatFromMimeType(img.mimeType);
                }
                Profiler.EndSample();

                if (imgFormat != ImageFormat.Unknown)
                {
                    if (img.bufferView >= 0)
                    {
                        if (imgFormat == ImageFormat.Ktx)
                        {
#if KTX
                            Profiler.BeginSample("CreateTexturesFromBuffers.KtxLoadNativeContext");
                            if(m_KtxLoadContextsBuffer==null) {
                                m_KtxLoadContextsBuffer = new List<KtxLoadContextBase>();
                            }
                            var ktxContext = new KtxLoadNativeContext(i,GetBufferView(img.bufferView));
                            m_KtxLoadContextsBuffer.Add(ktxContext);
                            Profiler.EndSample();
                            await m_DeferAgent.BreakPoint();
#else
                            m_Logger?.Error(LogCode.PackageMissing, "KTX for Unity", ExtensionName.TextureBasisUniversal);
#endif // KTX_UNITY
                        }
                        else
                        {
                            Profiler.BeginSample("CreateTexturesFromBuffers.ExtractBuffer");
                            bool forceSampleLinear = m_ImageGamma != null && !m_ImageGamma[i];
                            var txt = CreateEmptyTexture(img, i, forceSampleLinear);

                            if (!LoadCachedTexture(i, txt))
                            {
                                var bufferView = bufferViews[img.bufferView];
                                var buffer = GetBuffer(bufferView.buffer);
                                var chunk = m_BinChunks[bufferView.buffer];

                                var icc = new ImageCreateContext();
                                icc.imageIndex = i;
                                icc.buffer = new byte[bufferView.byteLength];
                                icc.gcHandle = GCHandle.Alloc(icc.buffer, GCHandleType.Pinned);

                                var job = CreateMemCopyJob(bufferView, buffer, chunk, icc);
                                icc.jobHandle = job.Schedule();

                                contexts.Add(icc);
                            }

                            m_Images[i] = txt;
                            m_Resources.Add(txt);
                            Profiler.EndSample();
                        }
                    }
                }
            }
        }

        static unsafe MemCopyJob CreateMemCopyJob(
            BufferViewBase bufferView,
            byte[] buffer,
            GlbBinChunk chunk,
            ImageCreateContext icc
            )
        {
            var job = new MemCopyJob();
            job.bufferSize = bufferView.byteLength;
            fixed (void* src = &(buffer[bufferView.byteOffset + chunk.Start]), dst = &(icc.buffer[0]))
            {
                job.input = src;
                job.result = dst;
            }

            return job;
        }

        Texture2D CreateEmptyTexture(Image img, int index, bool forceSampleLinear)
        {
#if UNITY_2022_1_OR_NEWER
            var textureCreationFlags = TextureCreationFlags.DontUploadUponCreate | TextureCreationFlags.DontInitializePixels;
#else
            var textureCreationFlags = TextureCreationFlags.None;
#endif
            if (m_Settings.GenerateMipMaps)
            {
                textureCreationFlags |= TextureCreationFlags.MipChain;
            }
            var txt = new Texture2D(
                4, 4,
                forceSampleLinear
                    ? GraphicsFormat.R8G8B8A8_UNorm
                    : GraphicsFormat.R8G8B8A8_SRGB,
                textureCreationFlags
            )
            {
                anisoLevel = m_Settings.AnisotropicFilterLevel,
                name = GetImageName(img, index)
            };
            return txt;
        }

        static string GetImageName(Image img, int index)
        {
            return string.IsNullOrEmpty(img.name) ? $"image_{index}" : img.name;
        }

        static void SafeDestroy(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEngine.Object.DestroyImmediate(obj);
            }
            else
#endif
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        /// <summary>Is called when retrieving data from accessors should be performed/started.</summary>
        public event Action LoadAccessorDataEvent;

        /// <summary>Is called when a mesh and its primitives are assigned to a <see cref="MeshResult"/> and
        /// sub-meshes. Parameters are MeshResult index, mesh index and per sub-mesh primitive index</summary>
        public event Action<int, int, int[]> MeshResultAssigned;

        async Task<bool> LoadAccessorData(RootBase gltf)
        {

            Profiler.BeginSample("LoadAccessorData.Init");

            var mainBufferTypes = new Dictionary<MeshPrimitiveBase, MainBufferType>();
            var meshCount = gltf.Meshes?.Count ?? 0;
            m_MeshPrimitiveCluster = gltf.Meshes == null
                ? null
                : new Dictionary<MeshPrimitiveBase, List<(int MeshIndex, MeshPrimitiveBase Primitive)>>[meshCount];
            Dictionary<MeshPrimitiveBase, MorphTargetsContext> morphTargetsContexts = null;
#if DEBUG
            var perAttributeMeshCollection = new Dictionary<Attributes,HashSet<int>>();
#endif

            // Iterate all primitive vertex attributes and remember the accessors usage.
            m_AccessorUsage = new AccessorUsage[gltf.Accessors.Count];

            LoadAccessorDataEvent?.Invoke();

            int totalPrimitives = 0;
            for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                var mesh = gltf.Meshes[meshIndex];
                m_MeshPrimitiveIndex[meshIndex] = totalPrimitives;
                var cluster = new Dictionary<MeshPrimitiveBase, List<(int MeshIndex, MeshPrimitiveBase Primitive)>>();

                for (var primIndex = 0; primIndex < mesh.Primitives.Count; primIndex++)
                {
                    var primitive = mesh.Primitives[primIndex];
                    if (!cluster.ContainsKey(primitive))
                    {
                        cluster[primitive] = new List<(int MeshIndex, MeshPrimitiveBase Primitive)>();
                    }
                    cluster[primitive].Add((primIndex, primitive));

                    if (primitive.targets != null)
                    {
                        if (morphTargetsContexts == null)
                        {
                            morphTargetsContexts = new Dictionary<MeshPrimitiveBase, MorphTargetsContext>();
                        }
                        else if (morphTargetsContexts.ContainsKey(primitive))
                        {
                            continue;
                        }

                        var morphTargetsContext = CreateMorphTargetsContext(primitive, mesh.Extras?.targetNames);
                        morphTargetsContexts[primitive] = morphTargetsContext;
                    }
#if DRACO_UNITY
                    var isDraco = primitive.IsDracoCompressed;
                    if (isDraco) {
                        continue;
                    }
#else
                    var isDraco = false;
#endif
                    var att = primitive.attributes;
                    if (primitive.indices >= 0)
                    {
                        var usage = (
                            primitive.mode == DrawMode.Triangles
                            || primitive.mode == DrawMode.TriangleStrip
                            || primitive.mode == DrawMode.TriangleFan
                            )
                        ? AccessorUsage.IndexFlipped
                        : AccessorUsage.Index;
                        SetAccessorUsage(primitive.indices, isDraco ? AccessorUsage.Ignore : usage);
                    }

                    if (!mainBufferTypes.TryGetValue(primitive, out var mainBufferType))
                    {
                        if (att.TANGENT >= 0)
                        {
                            mainBufferType = MainBufferType.PosNormTan;
                        }
                        else
                        if (att.NORMAL >= 0)
                        {
                            mainBufferType = MainBufferType.PosNorm;
                        }
                        else
                        {
                            mainBufferType = MainBufferType.Position;
                        }
                    }
                    if (primitive.mode == DrawMode.Triangles || primitive.mode == DrawMode.TriangleFan ||
                        primitive.mode == DrawMode.TriangleStrip)
                    {
                        if (primitive.material < 0 || gltf.Materials[primitive.material].RequiresNormals)
                        {
                            mainBufferType |= MainBufferType.Normal;
                        }
                        if (primitive.material >= 0 && gltf.Materials[primitive.material].RequiresTangents)
                        {
                            mainBufferType |= MainBufferType.Tangent;
                        }
                    }
                    mainBufferTypes[primitive] = mainBufferType;

#if DEBUG
                    if(!perAttributeMeshCollection.TryGetValue(att, out var attributeMesh)) {
                        attributeMesh = new HashSet<int>();
                        perAttributeMeshCollection[att] = attributeMesh;
                    }
                    attributeMesh.Add(meshIndex);
#endif

                    if (primitive.material >= 0)
                    {
                        if (gltf.Materials != null && primitive.mode == DrawMode.Points)
                        {
                            SetMaterialPointsSupport(primitive.material);
                        }
                    }
                    else
                    {
                        m_DefaultMaterialPointsSupport |= primitive.mode == DrawMode.Points;
                    }
                }
                m_MeshPrimitiveCluster[meshIndex] = cluster;
                totalPrimitives += cluster.Count;
            }

            if (gltf.Skins != null)
            {
                m_SkinsInverseBindMatrices = new Matrix4x4[gltf.Skins.Count][];
                foreach (var skin in gltf.Skins)
                {
                    if (skin.inverseBindMatrices >= 0)
                    {
                        SetAccessorUsage(skin.inverseBindMatrices, AccessorUsage.InverseBindMatrix);
                    }
                }
            }

            if (gltf.Nodes != null)
            {
                foreach (var node in gltf.Nodes)
                {
                    var attr = node.Extensions?.EXT_mesh_gpu_instancing?.attributes;
                    if (attr != null)
                    {
                        if (attr.TRANSLATION >= 0)
                        {
                            SetAccessorUsage(attr.TRANSLATION, AccessorUsage.Translation | AccessorUsage.RequiredForInstantiation);
                        }
                        if (attr.ROTATION >= 0)
                        {
                            SetAccessorUsage(attr.ROTATION, AccessorUsage.Rotation | AccessorUsage.RequiredForInstantiation);
                        }
                        if (attr.SCALE >= 0)
                        {
                            SetAccessorUsage(attr.SCALE, AccessorUsage.Scale | AccessorUsage.RequiredForInstantiation);
                        }
                    }
                }
            }

            if (m_MeshPrimitiveIndex != null)
            {
                m_MeshPrimitiveIndex[meshCount] = totalPrimitives;
            }
            m_Primitives = new MeshResult[totalPrimitives];
            m_PrimitiveContexts = new PrimitiveCreateContextBase[totalPrimitives];
            var tmpList = new List<JobHandle>(mainBufferTypes.Count);
            m_VertexAttributes = new Dictionary<MeshPrimitiveBase, VertexBufferConfigBase>(mainBufferTypes.Count);
#if DEBUG
            foreach (var perAttributeMeshes in perAttributeMeshCollection) {
                if(perAttributeMeshes.Value.Count>1) {
                    m_Logger?.Warning(LogCode.AccessorsShared);
                    break;
                }
            }
#endif
            Profiler.EndSample();

            var success = true;

            foreach (var mainBufferType in mainBufferTypes)
            {

                Profiler.BeginSample("LoadAccessorData.ScheduleVertexJob");

                var primitive = mainBufferType.Key;
                var att = primitive.attributes;

                bool hasNormals = att.NORMAL >= 0;
                bool hasTangents = att.TANGENT >= 0;

                int[] uvInputs = null;
                if (att.TEXCOORD_0 >= 0)
                {
                    int uvCount = 1;
                    if (att.TEXCOORD_1 >= 0) uvCount++;
                    if (att.TEXCOORD_2 >= 0) uvCount++;
                    if (att.TEXCOORD_3 >= 0) uvCount++;
                    if (att.TEXCOORD_4 >= 0) uvCount++;
                    if (att.TEXCOORD_5 >= 0) uvCount++;
                    if (att.TEXCOORD_6 >= 0) uvCount++;
                    if (att.TEXCOORD_7 >= 0) uvCount++;
                    uvInputs = new int[uvCount];
                    uvInputs[0] = att.TEXCOORD_0;
                    if (att.TEXCOORD_1 >= 0)
                    {
                        uvInputs[1] = att.TEXCOORD_1;
                    }
                    if (att.TEXCOORD_2 >= 0)
                    {
                        uvInputs[2] = att.TEXCOORD_2;
                    }
                    if (att.TEXCOORD_3 >= 0)
                    {
                        uvInputs[3] = att.TEXCOORD_3;
                    }
                    if (att.TEXCOORD_4 >= 0)
                    {
                        uvInputs[4] = att.TEXCOORD_4;
                    }
                    if (att.TEXCOORD_5 >= 0)
                    {
                        uvInputs[5] = att.TEXCOORD_5;
                    }
                    if (att.TEXCOORD_6 >= 0)
                    {
                        uvInputs[6] = att.TEXCOORD_6;
                    }
                    if (att.TEXCOORD_7 >= 0)
                    {
                        uvInputs[7] = att.TEXCOORD_7;
                    }
                    if (att.TEXCOORD_8 >= 0)
                    {
                        m_Logger?.Warning(LogCode.UVLimit);
                    }
                }

                VertexBufferConfigBase config;
                switch (mainBufferType.Value)
                {
                    case MainBufferType.Position:
                        config = new VertexBufferConfig<Vertex.VPos>(m_Logger);
                        break;
                    case MainBufferType.PosNorm:
                        config = new VertexBufferConfig<Vertex.VPosNorm>(m_Logger);
                        break;
                    case MainBufferType.PosNormTan:
                        config = new VertexBufferConfig<Vertex.VPosNormTan>(m_Logger);
                        break;
                    default:
                        m_Logger?.Error(LogCode.BufferMainInvalidType, mainBufferType.ToString());
                        return false;
                }
                config.calculateNormals = !hasNormals && (mainBufferType.Value & MainBufferType.Normal) > 0;
                config.calculateTangents = !hasTangents && (mainBufferType.Value & MainBufferType.Tangent) > 0;
                m_VertexAttributes[primitive] = config;

                var jh = config.ScheduleVertexJobs(
                    this,
                    att.POSITION,
                    att.NORMAL,
                    att.TANGENT,
                    uvInputs,
                    att.COLOR_0,
                    att.WEIGHTS_0,
                    att.JOINTS_0
                );

                if (jh.HasValue)
                {
                    tmpList.Add(jh.Value);
                }
                else
                {
                    success = false;
                    break;
                }

                Profiler.EndSample();

                await m_DeferAgent.BreakPoint();
            }

            if (!success)
            {
                return false;
            }

            if (morphTargetsContexts != null)
            {
                foreach (var morphTargetsContext in morphTargetsContexts)
                {
                    var jobHandle = morphTargetsContext.Value.GetJobHandle();
                    tmpList.Add(jobHandle);
                }
            }

#if UNITY_ANIMATION
            if (gltf.HasAnimation) {
                for (int i = 0; i < gltf.Animations.Count; i++) {
                    var animation = gltf.Animations[i];
                    foreach (var sampler in animation.Samplers) {
                        SetAccessorUsage(sampler.input,AccessorUsage.AnimationTimes);
                    }

                    foreach (var channel in animation.Channels) {
                        var accessorIndex = animation.Samplers[channel.sampler].output;
                        switch (channel.Target.GetPath()) {
                            case AnimationChannel.Path.Translation:
                                SetAccessorUsage(accessorIndex,AccessorUsage.Translation);
                                break;
                            case AnimationChannel.Path.Rotation:
                                SetAccessorUsage(accessorIndex,AccessorUsage.Rotation);
                                break;
                            case AnimationChannel.Path.Scale:
                                SetAccessorUsage(accessorIndex,AccessorUsage.Scale);
                                break;
                            case AnimationChannel.Path.Weights:
                                SetAccessorUsage(accessorIndex,AccessorUsage.Weight);
                                break;
                        }
                    }
                }
            }
#endif

            // Retrieve indices data jobified
            m_AccessorData = new AccessorDataBase[gltf.Accessors.Count];

            for (int i = 0; i < m_AccessorData.Length; i++)
            {
                Profiler.BeginSample("LoadAccessorData.IndicesMatrixJob");
                var acc = gltf.Accessors[i];
                if (acc.bufferView < 0)
                {
                    // Not actual accessor to data
                    // Common for draco meshes
                    // the accessor only holds meta information
                    continue;
                }
                switch (acc.GetAttributeType())
                {
                    case GltfAccessorAttributeType.SCALAR when m_AccessorUsage[i] == AccessorUsage.IndexFlipped ||
                        m_AccessorUsage[i] == AccessorUsage.Index:
                        {
                            var ads = new AccessorData<int>();
                            GetIndicesJob(gltf, i, out ads.data, out var jh, out ads.gcHandle, m_AccessorUsage[i] == AccessorUsage.IndexFlipped);
                            tmpList.Add(jh.Value);
                            m_AccessorData[i] = ads;
                            break;
                        }
                    case GltfAccessorAttributeType.MAT4 when m_AccessorUsage[i] == AccessorUsage.InverseBindMatrix:
                        {
                            // TODO: Maybe use AccessorData, since Mesh.bindposes only accepts C# arrays.
                            var ads = new AccessorNativeData<Matrix4x4>();
                            GetMatricesJob(gltf, i, out ads.data, out var jh);
                            tmpList.Add(jh.Value);
                            m_AccessorData[i] = ads;
                            break;
                        }
                    case GltfAccessorAttributeType.VEC3 when (m_AccessorUsage[i] & AccessorUsage.Translation) != 0:
                        {
                            var ads = new AccessorNativeData<Vector3>();
                            GetVector3Job(gltf, i, out ads.data, out var jh, true);
                            tmpList.Add(jh.Value);
                            m_AccessorData[i] = ads;
                            break;
                        }
                    case GltfAccessorAttributeType.VEC4 when (m_AccessorUsage[i] & AccessorUsage.Rotation) != 0:
                        {
                            var ads = new AccessorNativeData<Quaternion>();
                            GetVector4Job(gltf, i, out ads.data, out var jh);
                            tmpList.Add(jh.Value);
                            m_AccessorData[i] = ads;
                            break;
                        }
                    case GltfAccessorAttributeType.VEC3 when (m_AccessorUsage[i] & AccessorUsage.Scale) != 0:
                        {
                            var ads = new AccessorNativeData<Vector3>();
                            GetVector3Job(gltf, i, out ads.data, out var jh, false);
                            tmpList.Add(jh.Value);
                            m_AccessorData[i] = ads;
                            break;
                        }
#if UNITY_ANIMATION
                    case GltfAccessorAttributeType.SCALAR when m_AccessorUsage[i]==AccessorUsage.AnimationTimes || m_AccessorUsage[i]==AccessorUsage.Weight:
                    {
                        // JobHandle? jh;
                        var ads = new  AccessorNativeData<float>();
                        GetScalarJob(gltf, i, out var times, out var jh);
                        if (times.HasValue) {
                            ads.data = times.Value;
                        }
                        if (jh.HasValue) {
                            tmpList.Add(jh.Value);
                        }
                        m_AccessorData[i] = ads;
                        break;
                    }
#endif
                }
                Profiler.EndSample();
                await m_DeferAgent.BreakPoint();
            }

            Profiler.BeginSample("LoadAccessorData.PrimitiveCreateContexts");
            int primitiveIndex = 0;
            for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                var mesh = gltf.Meshes[meshIndex];
                foreach (var cluster in m_MeshPrimitiveCluster[meshIndex].Values)
                {

                    PrimitiveCreateContextBase context = null;

                    for (int primIndex = 0; primIndex < cluster.Count; primIndex++)
                    {
                        var (gltfPrimitiveIndex, primitive) = cluster[primIndex];
#if DRACO_UNITY
                        if (primitive.IsDracoCompressed) {
                            Bounds? bounds = null;
                            var posAccessorIndex = primitive?.attributes.POSITION ?? -1;
                            if (posAccessorIndex >= 0 && posAccessorIndex < gltf.Accessors.Count) {
                                var posAccessor = gltf.Accessors[posAccessorIndex];
                                bounds = posAccessor.TryGetBounds();
                            }

                            if (!bounds.HasValue) {
                                m_Logger.Error(LogCode.MeshBoundsMissing, meshIndex.ToString());
                            }
                            var dracoContext = new PrimitiveDracoCreateContext(
                                meshIndex,
                                primitiveIndex,
                                1,
                                primitive.material<0 || gltf.Materials[primitive.material].RequiresNormals,
                                primitive.material>=0 && gltf.Materials[primitive.material].RequiresTangents,
                                mesh.name,
                                bounds
                                );
                            context = dracoContext;
                        }
                        else
#endif
                        {
                            PrimitiveCreateContext c;
                            if (context == null)
                            {
                                c = new PrimitiveCreateContext(
                                    meshIndex,
                                    primitiveIndex,
                                    cluster.Count,
                                    mesh.name
                                    );
                            }
                            else
                            {
                                c = context as PrimitiveCreateContext;
                            }
                            // PreparePrimitiveIndices(gltf,primitive,ref c,primIndex);
                            c.SetPrimitiveIndex(primIndex, gltfPrimitiveIndex);
                            context = c;
                        }

                        if (primitive.targets != null)
                        {
                            context.morphTargetsContext = morphTargetsContexts[primitive];
                        }

                        context.SetMaterial(primIndex, primitive.material);
                    }

                    m_PrimitiveContexts[primitiveIndex] = context;
                    primitiveIndex++;
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("LoadAccessorData.Schedule");
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(tmpList.ToArray(), Allocator.Persistent);
            m_AccessorJobsHandle = JobHandle.CombineDependencies(jobHandles);
            jobHandles.Dispose();
            JobHandle.ScheduleBatchedJobs();

            Profiler.EndSample();
            return success;
        }

        MorphTargetsContext CreateMorphTargetsContext(MeshPrimitiveBase primitive, string[] meshTargetNames)
        {
            var morphTargetsContext = new MorphTargetsContext(primitive.targets.Length, meshTargetNames, m_DeferAgent);
            foreach (var morphTarget in primitive.targets)
            {
                var success = morphTargetsContext.AddMorphTarget(
                    this,
                    morphTarget.POSITION,
                    morphTarget.NORMAL,
                    morphTarget.TANGENT,
                    m_Logger
                );
                if (!success)
                {
                    m_Logger.Error(LogCode.MorphTargetContextFail);
                    break;
                }
            }

            return morphTargetsContext;
        }

        void SetAccessorUsage(int index, AccessorUsage newUsage)
        {
#if DEBUG
            if(m_AccessorUsage[index]!=AccessorUsage.Unknown && newUsage!=m_AccessorUsage[index]) {
                m_Logger?.Error(LogCode.AccessorInconsistentUsage, m_AccessorUsage[index].ToString(), newUsage.ToString());
            }
#endif
            m_AccessorUsage[index] = newUsage;
        }

        async Task CreatePrimitiveContexts(RootBase gltf)
        {
            int i = 0;
            bool schedule = false;
            for (int meshIndex = 0; meshIndex < gltf.Meshes.Count; meshIndex++)
            {
                foreach (var kvp in m_MeshPrimitiveCluster[meshIndex])
                {
                    var cluster = kvp.Value;

                    PrimitiveCreateContextBase context = m_PrimitiveContexts[i];

                    if (MeshResultAssigned != null)
                    {
                        var primitiveIndices = new int[cluster.Count];
                        for (var subMeshIndex = 0; subMeshIndex < cluster.Count; subMeshIndex++)
                        {
                            var subMesh = cluster[subMeshIndex];
                            primitiveIndices[subMeshIndex] = subMesh.Item1;

                            MeshResultAssigned?.Invoke(
                                m_MeshPrimitiveIndex[meshIndex], // MeshResult index
                                meshIndex, // glTF mesh index
                                primitiveIndices
                            );
                        }
                    }

                    for (int primIndex = 0; primIndex < cluster.Count; primIndex++)
                    {
                        var primitiveTuple = cluster[primIndex];
                        var primitive = primitiveTuple.Item2;
#if DRACO_UNITY
                        if( primitive.IsDracoCompressed ) {
                            var c = (PrimitiveDracoCreateContext) context;
                            await m_DeferAgent.BreakPoint();
                            Profiler.BeginSample( "CreatePrimitiveContext");
                            PreparePrimitiveDraco(gltf,primitive,ref c);
                            Profiler.EndSample();
                            schedule = true;
                        } else
#endif
                        {
                            PrimitiveCreateContext c = (PrimitiveCreateContext)context;
                            c.vertexData = m_VertexAttributes[kvp.Key];
                            Profiler.BeginSample("CreatePrimitiveContext");
                            PreparePrimitiveIndices(gltf, primitive, ref c, primIndex);
                            Profiler.EndSample();
                        }
                    }
                    await m_DeferAgent.BreakPoint();
                    i++;
                }
            }
            // TODO: not necessary with ECS
            // https://docs.unity3d.com/Manual/JobSystemTroubleshooting.html
            if (schedule)
            {
                JobHandle.ScheduleBatchedJobs();
            }
        }

        async Task AssignAllAccessorData(RootBase gltf)
        {
            if (gltf.Skins != null)
            {
                for (int s = 0; s < gltf.Skins.Count; s++)
                {
                    Profiler.BeginSample("AssignAllAccessorData.Skin");
                    var skin = gltf.Skins[s];
                    if (skin.inverseBindMatrices >= 0)
                    {
                        m_SkinsInverseBindMatrices[s] = ((AccessorNativeData<Matrix4x4>)m_AccessorData[skin.inverseBindMatrices]).data.ToArray();
                    }
                    Profiler.EndSample();
                    await m_DeferAgent.BreakPoint();
                }
            }
        }

        void PreparePrimitiveIndices(RootBase gltf, MeshPrimitiveBase primitive, ref PrimitiveCreateContext c, int subMesh = 0)
        {
            Profiler.BeginSample("PreparePrimitiveIndices");
            switch (primitive.mode)
            {
                case DrawMode.Triangles:
                    c.topology = MeshTopology.Triangles;
                    break;
                case DrawMode.Points:
                    c.topology = MeshTopology.Points;
                    break;
                case DrawMode.Lines:
                    c.topology = MeshTopology.Lines;
                    break;
                case DrawMode.LineLoop:
                    m_Logger?.Error(LogCode.PrimitiveModeUnsupported, primitive.mode.ToString());
                    c.topology = MeshTopology.LineStrip;
                    break;
                case DrawMode.LineStrip:
                    c.topology = MeshTopology.LineStrip;
                    break;
                case DrawMode.TriangleStrip:
                case DrawMode.TriangleFan:
                default:
                    m_Logger?.Error(LogCode.PrimitiveModeUnsupported, primitive.mode.ToString());
                    c.topology = MeshTopology.Triangles;
                    break;
            }

            if (primitive.indices >= 0)
            {
                c.SetIndices(subMesh, ((AccessorData<int>)m_AccessorData[primitive.indices]).data);
            }
            else
            {
                int vertexCount = gltf.Accessors[primitive.attributes.POSITION].count;
                CalculateIndicesJob(primitive, vertexCount, c.topology, out var indices, out c.jobHandle, out c.calculatedIndicesHandle);
                c.SetIndices(subMesh, indices);
            }
            Profiler.EndSample();
        }

#if DRACO_UNITY
        void PreparePrimitiveDraco( RootBase gltf, MeshPrimitiveBase primitive, ref PrimitiveDracoCreateContext c ) {
            var dracoExt = primitive.Extensions.KHR_draco_mesh_compression;

            var bufferView = gltf.BufferViews[dracoExt.bufferView];
            var buffer = GetBufferViewSlice(bufferView);

            c.StartDecode(buffer, dracoExt.attributes);
        }
#endif

        static unsafe void CalculateIndicesJob(
            MeshPrimitiveBase primitive,
            int vertexCount,
            MeshTopology topology,
            out int[] indices,
            out JobHandle jobHandle,
            out GCHandle resultHandle
            )
        {
            Profiler.BeginSample("CalculateIndicesJob");
            // No indices: calculate them
            bool lineLoop = primitive.mode == DrawMode.LineLoop;
            // extra index (first vertex again) for closing line loop
            indices = new int[vertexCount + (lineLoop ? 1 : 0)];
            resultHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            if (topology == MeshTopology.Triangles)
            {
                var job8 = new CreateIndicesInt32FlippedJob();
                fixed (void* dst = &(indices[0]))
                {
                    job8.result = (int*)dst;
                }
                jobHandle = job8.Schedule(indices.Length, DefaultBatchCount);
            }
            else
            {
                var job8 = new CreateIndicesInt32Job();
                if (lineLoop)
                {
                    // Set the last index to the first vertex
                    indices[vertexCount] = 0;
                }
                fixed (void* dst = &(indices[0]))
                {
                    job8.result = (int*)dst;
                }
                jobHandle = job8.Schedule(vertexCount, DefaultBatchCount);
            }
            Profiler.EndSample();
        }

        unsafe void GetIndicesJob(RootBase gltf, int accessorIndex, out int[] indices, out JobHandle? jobHandle, out GCHandle resultHandle, bool flip)
        {
            Profiler.BeginSample("PrepareGetIndicesJob");
            // index
            var accessor = gltf.Accessors[accessorIndex];
            var bufferView = GetBufferView(accessor.bufferView, accessor.byteOffset);

            Profiler.BeginSample("Alloc");
            indices = new int[accessor.count];
            Profiler.EndSample();
            Profiler.BeginSample("Pin");
            resultHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            Profiler.EndSample();

            Assert.AreEqual(accessor.GetAttributeType(), GltfAccessorAttributeType.SCALAR);
            //Assert.AreEqual(accessor.count * GetLength(accessor.typeEnum) * 4 , (int) chunk.length);
            if (accessor.IsSparse)
            {
                m_Logger.Error(LogCode.SparseAccessor, "indices");
            }

            Profiler.BeginSample("CreateJob");
            switch (accessor.componentType)
            {
                case GltfComponentType.UnsignedByte:
                    if (flip)
                    {
                        var job8 = new ConvertIndicesUInt8ToInt32FlippedJob();
                        fixed (void* dst = &(indices[0]))
                        {
                            job8.input = (byte*)bufferView.GetUnsafeReadOnlyPtr();
                            job8.result = (int3*)dst;
                        }
                        jobHandle = job8.Schedule(accessor.count / 3, DefaultBatchCount);
                    }
                    else
                    {
                        var job8 = new ConvertIndicesUInt8ToInt32Job();
                        fixed (void* dst = &(indices[0]))
                        {
                            job8.input = (byte*)bufferView.GetUnsafeReadOnlyPtr();
                            job8.result = (int*)dst;
                        }
                        jobHandle = job8.Schedule(accessor.count, DefaultBatchCount);
                    }
                    break;
                case GltfComponentType.UnsignedShort:
                    if (flip)
                    {
                        var job16 = new ConvertIndicesUInt16ToInt32FlippedJob();
                        fixed (void* dst = &(indices[0]))
                        {
                            job16.input = (ushort*)bufferView.GetUnsafeReadOnlyPtr();
                            job16.result = (int3*)dst;
                        }
                        jobHandle = job16.Schedule(accessor.count / 3, DefaultBatchCount);
                    }
                    else
                    {
                        var job16 = new ConvertIndicesUInt16ToInt32Job();
                        fixed (void* dst = &(indices[0]))
                        {
                            job16.input = (ushort*)bufferView.GetUnsafeReadOnlyPtr();
                            job16.result = (int*)dst;
                        }
                        jobHandle = job16.Schedule(accessor.count, DefaultBatchCount);
                    }
                    break;
                case GltfComponentType.UnsignedInt:
                    if (flip)
                    {
                        var job32 = new ConvertIndicesUInt32ToInt32FlippedJob();
                        fixed (void* dst = &(indices[0]))
                        {
                            job32.input = (uint*)bufferView.GetUnsafeReadOnlyPtr();
                            job32.result = (int3*)dst;
                        }
                        jobHandle = job32.Schedule(accessor.count / 3, DefaultBatchCount);
                    }
                    else
                    {
                        var job32 = new ConvertIndicesUInt32ToInt32Job();
                        fixed (void* dst = &(indices[0]))
                        {
                            job32.input = (uint*)bufferView.GetUnsafeReadOnlyPtr();
                            job32.result = (int*)dst;
                        }
                        jobHandle = job32.Schedule(accessor.count, DefaultBatchCount);
                    }
                    break;
                default:
                    m_Logger?.Error(LogCode.IndexFormatInvalid, accessor.componentType.ToString());
                    jobHandle = null;
                    break;
            }
            Profiler.EndSample();
            Profiler.EndSample();
        }

        unsafe void GetMatricesJob(RootBase gltf, int accessorIndex, out NativeArray<Matrix4x4> matrices, out JobHandle? jobHandle)
        {
            Profiler.BeginSample("GetMatricesJob");
            // index
            var accessor = gltf.Accessors[accessorIndex];
            var bufferView = GetBufferView(accessor.bufferView, accessor.byteOffset);

            Profiler.BeginSample("Alloc");
            matrices = new NativeArray<Matrix4x4>(accessor.count, Allocator.Persistent);
            Profiler.EndSample();

            Assert.AreEqual(accessor.GetAttributeType(), GltfAccessorAttributeType.MAT4);
            //Assert.AreEqual(accessor.count * GetLength(accessor.typeEnum) * 4 , (int) chunk.length);
            if (accessor.IsSparse)
            {
                m_Logger.Error(LogCode.SparseAccessor, "Matrix");
            }

            Profiler.BeginSample("CreateJob");
            switch (accessor.componentType)
            {
                case GltfComponentType.Float:
                    var job32 = new ConvertMatricesJob
                    {
                        input = (float4x4*)bufferView.GetUnsafeReadOnlyPtr(),
                        result = (float4x4*)matrices.GetUnsafePtr()
                    };
                    jobHandle = job32.Schedule(accessor.count, DefaultBatchCount);
                    break;
                default:
                    m_Logger?.Error(LogCode.IndexFormatInvalid, accessor.componentType.ToString());
                    jobHandle = null;
                    break;
            }
            Profiler.EndSample();
            Profiler.EndSample();
        }

        unsafe void GetVector3Job(RootBase gltf, int accessorIndex, out NativeArray<Vector3> vectors, out JobHandle? jobHandle, bool flip)
        {
            Profiler.BeginSample("GetVector3Job");
            var accessor = gltf.Accessors[accessorIndex];
            var bufferView = GetBufferView(accessor.bufferView, accessor.byteOffset);

            Profiler.BeginSample("Alloc");
            vectors = new NativeArray<Vector3>(accessor.count, Allocator.Persistent);
            Profiler.EndSample();

            Assert.AreEqual(accessor.GetAttributeType(), GltfAccessorAttributeType.VEC3);
            if (accessor.IsSparse)
            {
                m_Logger.Error(LogCode.SparseAccessor, "Vector3");
            }

            Profiler.BeginSample("CreateJob");
            switch (accessor.componentType)
            {
                case GltfComponentType.Float:
                    {
                        if (flip)
                        {
                            var job = new ConvertVector3FloatToFloatJob
                            {
                                input = (float3*)bufferView.GetUnsafeReadOnlyPtr(),
                                result = (float3*)vectors.GetUnsafePtr()
                            };
                            jobHandle = job.Schedule(accessor.count, DefaultBatchCount);
                        }
                        else
                        {
                            var job = new MemCopyJob
                            {
                                input = (float*)bufferView.GetUnsafeReadOnlyPtr(),
                                bufferSize = accessor.count * 12,
                                result = (float*)vectors.GetUnsafePtr()
                            };
                            jobHandle = job.Schedule();
                        }
                        break;
                    }
                default:
                    m_Logger?.Error(LogCode.IndexFormatInvalid, accessor.componentType.ToString());
                    jobHandle = null;
                    break;
            }
            Profiler.EndSample();
            Profiler.EndSample();
        }

        unsafe void GetVector4Job(RootBase gltf, int accessorIndex, out NativeArray<Quaternion> vectors, out JobHandle? jobHandle)
        {
            Profiler.BeginSample("GetVector4Job");
            // index
            var accessor = gltf.Accessors[accessorIndex];
            var bufferView = GetBufferView(accessor.bufferView, accessor.byteOffset);

            Profiler.BeginSample("Alloc");
            vectors = new NativeArray<Quaternion>(accessor.count, Allocator.Persistent);
            Profiler.EndSample();

            Assert.AreEqual(accessor.GetAttributeType(), GltfAccessorAttributeType.VEC4);
            if (accessor.IsSparse)
            {
                m_Logger.Error(LogCode.SparseAccessor, "Vector4");
            }

            Profiler.BeginSample("CreateJob");
            switch (accessor.componentType)
            {
                case GltfComponentType.Float:
                    {
                        var job = new ConvertRotationsFloatToFloatJob
                        {
                            input = (float4*)bufferView.GetUnsafeReadOnlyPtr(),
                            result = (float4*)vectors.GetUnsafePtr()
                        };
                        jobHandle = job.Schedule(accessor.count, DefaultBatchCount);
                        break;
                    }
                case GltfComponentType.Short:
                    {
                        var job = new ConvertRotationsInt16ToFloatJob
                        {
                            input = (short*)bufferView.GetUnsafeReadOnlyPtr(),
                            result = (float*)vectors.GetUnsafePtr()
                        };
                        jobHandle = job.Schedule(accessor.count, DefaultBatchCount);
                        break;
                    }
                case GltfComponentType.Byte:
                    {
                        var job = new ConvertRotationsInt8ToFloatJob
                        {
                            input = (sbyte*)bufferView.GetUnsafeReadOnlyPtr(),
                            result = (float*)vectors.GetUnsafePtr()
                        };
                        jobHandle = job.Schedule(accessor.count, DefaultBatchCount);
                        break;
                    }
                default:
                    m_Logger?.Error(LogCode.IndexFormatInvalid, accessor.componentType.ToString());
                    jobHandle = null;
                    break;
            }
            Profiler.EndSample();
            Profiler.EndSample();
        }

#if UNITY_ANIMATION
        unsafe void GetScalarJob(RootBase gltf, int accessorIndex, out NativeArray<float>? scalars, out JobHandle? jobHandle) {
            Profiler.BeginSample("GetScalarJob");
            scalars = null;
            jobHandle = null;
            var accessor = gltf.Accessors[accessorIndex];
            var buffer = GetBufferView(accessor.bufferView,accessor.byteOffset);

            Assert.AreEqual(accessor.GetAttributeType(), GltfAccessorAttributeType.SCALAR);
            if (accessor.IsSparse) {
                m_Logger.Error(LogCode.SparseAccessor,"scalars");
            }

            if (accessor.componentType == GltfComponentType.Float) {
                Profiler.BeginSample("CopyAnimationTimes");
                // TODO: For long animations with lots of times, threading this just like everything else maybe makes sense.
                var bufferTimes = Reinterpret<float>(buffer, accessor.count);
                // Copy values
                scalars = new NativeArray<float>(bufferTimes, Allocator.Persistent);
                ReleaseReinterpret(bufferTimes);
                Profiler.EndSample();
            } else
            if( accessor.normalized ) {
                Profiler.BeginSample("Alloc");
                scalars = new NativeArray<float>(accessor.count,Allocator.Persistent);
                Profiler.EndSample();

                switch( accessor.componentType ) {
                    case GltfComponentType.Byte: {
                        var job = new ConvertScalarInt8ToFloatNormalizedJob {
                            input = (sbyte*)buffer.GetUnsafeReadOnlyPtr(),
                            result = scalars.Value
                        };
                        jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                        break;
                    }
                    case GltfComponentType.UnsignedByte: {
                        var job = new ConvertScalarUInt8ToFloatNormalizedJob {
                            input = (byte*)buffer.GetUnsafeReadOnlyPtr(),
                            result = scalars.Value
                        };
                        jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                        break;
                    }
                    case GltfComponentType.Short: {
                        var job = new ConvertScalarInt16ToFloatNormalizedJob {
                            input = (short*) ((byte*)buffer.GetUnsafeReadOnlyPtr()),
                            result = scalars.Value
                        };
                        jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                        break;
                    }
                    case GltfComponentType.UnsignedShort: {
                        var job = new ConvertScalarUInt16ToFloatNormalizedJob {
                            input = (ushort*) ((byte*)buffer.GetUnsafeReadOnlyPtr()),
                            result = scalars.Value
                        };
                        jobHandle = job.Schedule(accessor.count,DefaultBatchCount);
                        break;
                    }
                    default:
                        m_Logger?.Error(LogCode.AnimationFormatInvalid, accessor.componentType.ToString());
                        break;
                }
            } else {
                // Non-normalized
                m_Logger?.Error(LogCode.AnimationFormatInvalid, accessor.componentType.ToString());
            }
            Profiler.EndSample();
        }

#endif // UNITY_ANIMATION

        /// <summary>
        /// Get glTF accessor and its raw data
        /// </summary>
        /// <param name="index">glTF accessor index</param>
        /// <param name="accessor">De-serialized glTF accessor</param>
        /// <param name="data">Pointer to accessor's data in memory</param>
        /// <param name="byteStride">Element byte stride</param>
        unsafe void IGltfBuffers.GetAccessor(int index, out AccessorBase accessor, out void* data, out int byteStride)
        {
            accessor = Root.Accessors[index];
            if (accessor.bufferView < 0 || accessor.bufferView >= Root.BufferViews.Count)
            {
                data = null;
                byteStride = 0;
                return;
            }
            var bufferView = Root.BufferViews[accessor.bufferView];
#if MESHOPT
            var meshopt = bufferView.Extensions?.EXT_meshopt_compression;
            if (meshopt != null) {
                byteStride = meshopt.byteStride;
                data = (byte*)m_MeshoptBufferViews[accessor.bufferView].GetUnsafeReadOnlyPtr() + accessor.byteOffset;
            } else
#endif
            {
                byteStride = bufferView.byteStride;
                var bufferIndex = bufferView.buffer;
                var buffer = GetBuffer(bufferIndex);
                fixed (void* src = &(buffer[accessor.byteOffset + bufferView.byteOffset + m_BinChunks[bufferIndex].Start]))
                {
                    data = src;
                }
            }

            // // Alternative that uses NativeArray/Slice
            // var bufferViewData = GetBufferView(bufferView);
            // data =  (byte*)bufferViewData.GetUnsafeReadOnlyPtr() + accessor.byteOffset;
        }

        /// <summary>
        /// Get sparse indices raw data
        /// </summary>
        /// <param name="sparseIndices">glTF sparse indices accessor</param>
        /// <param name="data">Pointer to accessor's data in memory</param>
        public unsafe void GetAccessorSparseIndices(AccessorSparseIndices sparseIndices, out void* data)
        {
            var bufferView = Root.BufferViews[(int)sparseIndices.bufferView];
#if MESHOPT
            var meshopt = bufferView.Extensions?.EXT_meshopt_compression;
            if (meshopt != null) {
                data = (byte*)m_MeshoptBufferViews[(int)sparseIndices.bufferView].GetUnsafeReadOnlyPtr() + sparseIndices.byteOffset;
            }
            else
#endif
            {
                var bufferIndex = bufferView.buffer;
                var buffer = GetBuffer(bufferIndex);
                fixed (void* src = &(buffer[sparseIndices.byteOffset + bufferView.byteOffset + m_BinChunks[bufferIndex].Start]))
                {
                    data = src;
                }
            }
        }

        /// <summary>
        /// Get sparse value raw data
        /// </summary>
        /// <param name="sparseValues">glTF sparse values accessor</param>
        /// <param name="data">Pointer to accessor's data in memory</param>
        public unsafe void GetAccessorSparseValues(AccessorSparseValues sparseValues, out void* data)
        {
            var bufferView = Root.BufferViews[(int)sparseValues.bufferView];
#if MESHOPT
            var meshopt = bufferView.Extensions?.EXT_meshopt_compression;
            if (meshopt != null) {
                data = (byte*)m_MeshoptBufferViews[(int)sparseValues.bufferView].GetUnsafeReadOnlyPtr() + sparseValues.byteOffset;
            }
            else
#endif
            {
                var bufferIndex = bufferView.buffer;
                var buffer = GetBuffer(bufferIndex);
                fixed (void* src = &(buffer[sparseValues.byteOffset + bufferView.byteOffset + m_BinChunks[bufferIndex].Start]))
                {
                    data = src;
                }
            }
        }

        static ImageFormat GetImageFormatFromMimeType(string mimeType)
        {
            if (!mimeType.StartsWith("image/")) return ImageFormat.Unknown;
            var sub = mimeType.Substring(6);
            switch (sub)
            {
                case "jpeg":
                    return ImageFormat.Jpeg;
                case "png":
                    return ImageFormat.PNG;
                case "ktx":
                case "ktx2":
                    return ImageFormat.Ktx;
                default:
                    return ImageFormat.Unknown;
            }
        }

#if KTX
        struct KtxTranscodeTaskWrapper {
            public int index;
            public TextureResult result;
        }

        static async Task<KtxTranscodeTaskWrapper> KtxLoadAndTranscode(int index, KtxLoadContextBase ktx, bool linear) {
            return new KtxTranscodeTaskWrapper {
                index = index,
                result = await ktx.LoadTexture2D(linear)
            };
        }

        async Task ProcessKtxLoadContexts() {
            var maxCount = SystemInfo.processorCount+1;

            var totalCount = m_KtxLoadContextsBuffer.Count;
            var startedCount = 0;
            var ktxTasks = new List<Task<KtxTranscodeTaskWrapper>>(maxCount);

            while (startedCount < totalCount || ktxTasks.Count>0) {
                while (ktxTasks.Count < maxCount && startedCount < totalCount) {
                    var ktx = m_KtxLoadContextsBuffer[startedCount];
                    var forceSampleLinear = m_ImageGamma != null && !m_ImageGamma[ktx.imageIndex];
                    ktxTasks.Add(KtxLoadAndTranscode(startedCount, ktx, forceSampleLinear));
                    startedCount++;
                    await m_DeferAgent.BreakPoint();
                }

                var kTask = await Task.WhenAny(ktxTasks);
                var i = kTask.Result.index;
                if (kTask.Result.result.errorCode == ErrorCode.Success) {
                    var ktx = m_KtxLoadContextsBuffer[i];
                    m_Images[ktx.imageIndex] = kTask.Result.result.texture;
                    if (!kTask.Result.result.orientation.IsYFlipped())
                    {
                        m_NonFlippedYTextureIndices ??= new HashSet<int>();
                        m_NonFlippedYTextureIndices.Add(ktx.imageIndex);
                    }
                    await m_DeferAgent.BreakPoint();
                }
                ktxTasks.Remove(kTask);
            }

            m_KtxLoadContextsBuffer.Clear();
        }
#endif // KTX

#if UNITY_EDITOR
        /// <summary>
        /// Returns true if this import is for an asset, in contrast to
        /// runtime loading.
        /// </summary>
        static bool IsEditorImport => !EditorApplication.isPlaying;
#endif // UNITY_EDITOR
    }
}
