using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text;
using System.Management.Instrumentation;
using System.Linq;
using System.Data.Common;

public class ExternalAssetDS : INotifyBindablePropertyChanged
{
    public LoadableAsset LoadableAsset;
    public ExternalAssetFileData FileData;
    public ExternalAssetMetadata Metadata;

    private static StringBuilder _sb = new StringBuilder();

    [CreateProperty] public string MetadataFilename
    {
        get
        {
            if (LoadableAsset == null || LoadableAsset.ExternalAssetMetadata == null
                || LoadableAsset.ExternalAssetMetadata.SourceFile == null)
                return "Unknown";
            else
                return LoadableAsset.ExternalAssetMetadata.SourceFile;
        }
    }

    [CreateProperty] public string AssetCategory 
    { 
        get
        {
            if (LoadableAsset == null)
                return "Unknown";

            return LoadableAsset.EditorLayer.ToString();
        } 
    }

    [CreateProperty] public DisplayStyle ShowImportLogs
    {
        get
        {
            if (LoadableAsset != null && LoadableAsset.ExternalAssetMetadata != null &&
                LoadableAsset.ExternalAssetMetadata.ImportLogMessages != null &&
                LoadableAsset.ExternalAssetMetadata.ImportLogMessages.Count > 0)
                return DisplayStyle.Flex;
            else
                return DisplayStyle.None;
        }
    }

    [CreateProperty] public string ImportLogText
    {
        get
        {
            if (LoadableAsset == null || LoadableAsset.ExternalAssetMetadata == null ||
                LoadableAsset.ExternalAssetMetadata.ImportLogMessages == null ||
                LoadableAsset.ExternalAssetMetadata.ImportLogMessages.Count <= 0)
                return "None";

            _sb.Clear();
            foreach (var log in LoadableAsset.ExternalAssetMetadata.ImportLogMessages)
            {
                _sb.AppendLine(log.Message);
            }

            return _sb.ToString();
        }
    }

    [CreateProperty] public string AssetDetailText
    {
        get
        {
            if (LoadableAsset == null)
                return "Unknown";

            int count = 0;
            _sb.Clear();

            if (LoadableAsset.GeometryObject != null)
            {
                var meshFilters = LoadableAsset.GeometryObject.GetComponentsInChildren<MeshFilter>();
                _sb.AppendFormat("<style=highlight>Number of meshes:</style> {0}\n", meshFilters.Length);
                count = 0;
                foreach (var meshFilter in meshFilters)
                {
                    _sb.AppendFormat("<style=gray>Mesh:</style> {0} <style=gray>Triangles:</style> {1}\n", meshFilter.mesh.name, meshFilter.mesh.triangles.Length / 3);
                    count++;
                    if (count > 75)
                    {
                        _sb.AppendLine("...");
                        break;
                    }
                }
            }

            if (LoadableAsset.GeometryObject != null && LoadableAsset.GeometryObject.TryGetComponent<GeometryObjectInfo>(out var geomObjInfo))
            {
                if (geomObjInfo.MeshColliders != null && geomObjInfo.MeshColliders.Count > 0)
                {
                    _sb.AppendFormat("<style=highlight>Number of mesh colliders:</style> {0}\n", geomObjInfo.MeshColliders.Count);
                    count = 0;
                    foreach (var col in geomObjInfo.MeshColliders)
                    {
                        _sb.AppendFormat("<style=gray>Collider:</style> {0} <style=gray>Triangles:</style> {1}\n", col.name, col.sharedMesh.triangles.Length / 3);
                    }
                }

                if (geomObjInfo.OtherColliders != null && geomObjInfo.OtherColliders.Count > 0)
                {
                    _sb.AppendFormat("<style=highlight>Number of other colliders:</style> {0}\n", geomObjInfo.OtherColliders.Count);
                    count = 0;
                    foreach (var col in geomObjInfo.OtherColliders)
                    {
                        _sb.AppendFormat("<style=gray>Collider:</style> {0}\n", col.name);
                        count++;
                        if (count > 75)
                        {
                            _sb.AppendLine("...");
                            break;
                        }
                    }
                }
            }

            if (LoadableAsset.MeshRenderers != null)
            {
                _sb.AppendFormat("<style=highlight>Number of mesh renderers:</style> {0}\n", LoadableAsset.MeshRenderers.Length);
                count = 0;
                foreach (var rend in LoadableAsset.MeshRenderers)
                {
                    _sb.AppendFormat("<style=gray>Renderer:</style> {0}\n", rend.name);
                    count++;
                    if (count > 75)
                    {
                        _sb.AppendLine("...");
                        break;
                    }
                }
            }

            return _sb.ToString();
        }
    }

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }

    public static void Cleanup()
    {
        _sb.Clear();
        _sb.Capacity = 0;
    }
}
