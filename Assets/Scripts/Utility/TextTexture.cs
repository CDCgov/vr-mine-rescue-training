using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.AddressableAssets;

public class TextTexture : MonoBehaviour
{
    public RenderTexture RenderTexture;
    public int TexSize = 256;
    public GameObject TextRendererPrefab;
    public string Text;
    public Color TextColor;
    public TMP_FontAsset Font;
    public float MinTextSize = 1;
    public float MaxTextSize = 8;
    public FontStyles FontStyle = FontStyles.Normal;
    public bool EnableWordWrapping = true;
    public float Margin = 0;
    public Material DecalMaterial;

    private float _opacity = 1.0f;

    public float Opacity
    {
        get
        {
            if (_decalProjector == null)
                return _opacity;

            return _decalProjector.fadeFactor;
        }

        set
        {
            _opacity = Mathf.Clamp(value, 0, 1); 

            if (_decalProjector != null)
            {
                _decalProjector.fadeFactor = _opacity;
            }
        }
    }

    private GameObject _textMeshPrefab;
    private DecalProjector _decalProjector;

    void Start()
    {
        _textMeshPrefab = Resources.Load<GameObject>("TextTextureTextMesh");
     

        if (RenderTexture == null)
        {
            //var rt = new RenderTexture(TexSize, TexSize, 16, RenderTextureFormat.ARGBHalf);
            var rt = new RenderTexture(TexSize, TexSize, 16, RenderTextureFormat.Default);
            //rt.volumeDepth = 1;
            rt.Create();
            rt.name = $"{Text}_{gameObject.name}_RT";
            rt.vrUsage = VRTextureUsage.None;
            RenderTexture = rt;
        }

        UpdateTexture();

        if (TryGetComponent<DecalProjector>(out _decalProjector))
        {
            Material mat = new Material(DecalMaterial);

            mat.SetTexture("_BaseColorMap", RenderTexture);

            //_textMaterial = mat;
            _decalProjector.material = mat;
            _decalProjector.fadeFactor = _opacity;
        }
     

    }

    public void UpdateTexture()
    {
        if (_textMeshPrefab == null)
            return;
        
        CommandBuffer cb = new CommandBuffer();
       
        Vector3 camPos = Vector3.zero;
        Vector3 camForward = Vector3.forward;
        Vector3 camUp = Vector3.up;

        //Generate view matrix from unity docs example:
        // Matrix that looks from camera's position, along the forward axis.
        var lookMatrix = Matrix4x4.LookAt(camPos, camPos + camForward, camUp);
        // Matrix that mirrors along Z axis, to match the camera space convention.
        var scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
        // Final view matrix is inverse of the LookAt matrix, and then mirrored along Z.
        var viewMatrix = scaleMatrix * lookMatrix.inverse;

        float extent = Margin + 2.5f;
        var projMatrix = Matrix4x4.Ortho(extent * -1, extent, extent * -1, extent, 0.1f, 5);

        cb.Clear();
        cb.SetRenderTarget(RenderTexture);
        cb.SetViewProjectionMatrices(viewMatrix, projMatrix);
        cb.DisableShaderKeyword("STEREO_INSTANCING_ON");
        cb.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
        //cb.ClearRenderTarget(true, true, Color.HSVToRGB(Random.value, 1, 1));

        var textMeshObj = Instantiate<GameObject>(_textMeshPrefab);
        var textMesh = textMeshObj.GetComponent<TextMeshPro>();
        DrawTextMesh(textMesh, cb);        

        Graphics.ExecuteCommandBuffer(cb);
        cb.Release();

        Destroy(textMeshObj);
    }

    private void DrawTextMesh(TextMeshPro tmp, CommandBuffer cb)
    {
        tmp.gameObject.SetActive(true);

        tmp.text = Text;
        tmp.color = TextColor;
        if (Font != null)
            tmp.font = Font;

        if (MinTextSize == MaxTextSize)
        {
            tmp.fontSize = MaxTextSize;
            tmp.enableAutoSizing = false;
        }
        else
        {
            tmp.fontSizeMin = MinTextSize;
            tmp.fontSizeMax = MaxTextSize;
            tmp.enableAutoSizing = true;
        }

        tmp.enableWordWrapping = EnableWordWrapping;
        tmp.fontStyle = FontStyle;
        
        tmp.ForceMeshUpdate(true, true);

        if (!tmp.TryGetComponent<MeshFilter>(out var filter))
            return;
        if (!tmp.TryGetComponent<MeshRenderer>(out var rend))
            return;

        //cb.DrawMesh(filter.mesh, Matrix4x4.TRS(_textMeshRend.transform.position, _textMeshRend.transform.rotation, _textMeshRend.transform.lossyScale), _textMeshRend.sharedMaterial);
        cb.DrawMesh(filter.mesh, Matrix4x4.TRS(new Vector3(0, 0, 1), Quaternion.identity, Vector3.one), rend.sharedMaterial);

        tmp.gameObject.SetActive(false);
    }
}
