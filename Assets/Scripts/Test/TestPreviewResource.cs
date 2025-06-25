using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestPreviewResource : MonoBehaviour 
{
    public string ResourceName;

    private string _previewedResourceName;
    private MeshPreviewData _previewData;

    void OnDrawGizmos()
    {
        if (_previewedResourceName == null || _previewedResourceName != ResourceName)
        {
            _previewData = null;

            GameObject obj = Resources.Load<GameObject>(ResourceName);
            if (obj != null)
            {
                _previewData = Util.BuildPreviewData(obj);
            }

            if (_previewData != null)
            {
                Util.DrawPreview(transform, _previewData);
            }
        }
    }
    
}