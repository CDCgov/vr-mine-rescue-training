using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using System.Text;
using UnityEngine.UI;

public class ExternalMaterialBenchmark : MonoBehaviour
{
    public MaterialManager MaterialManager;

    public Transform RendererParentTransform;

    public TMP_Text InfoTextBox;
    public TMP_Text FramerateTextBox;

    public GameObject ButtonPrefab;
    public Transform ButtonContainer;

    public CinemachineDollyCart DollyCart;

    System.Diagnostics.Stopwatch _stopwatch;
    private int _startFrame;
    private List<MaterialMetadata> _materials;

    private List<Renderer> _renderers;

    private IEnumerator Start()
    {
        _stopwatch = new System.Diagnostics.Stopwatch();

        if (MaterialManager == null)
            MaterialManager = MaterialManager.GetDefault(gameObject);

        InfoTextBox.text = "";
        FramerateTextBox.text = "Loading...";

        DollyCart.m_Position = 0;
        DollyCart.m_Speed = 0;

        yield return MaterialManager.LoadBuiltInMaterials();
        yield return MaterialManager.LoadExternalMaterials();

        _materials = new List<MaterialMetadata>();
        foreach (var mat in MaterialManager.GetAllMaterials())
        {
            if (mat == null || mat.LoadedMaterial == null)
                continue;
            
            _materials.Add(mat);
        }

        _materials.Sort((a, b) => {
            return string.Compare(a.MaterialID, b.MaterialID);
        });

        StringBuilder sb = new StringBuilder();

        GitVersion.ReadVersion();
        sb.AppendLine(GitVersion.Version);

        for (int i = 0; i < _materials.Count; i++)
        {
            sb.AppendFormat("{0,5}: {1}\n", i + 1, _materials[i].MaterialID);
            AddMaterialButton(_materials[i]);
        }

        _renderers = new List<Renderer>(100);
        RendererParentTransform.GetComponentsInChildren<Renderer>(_renderers);

        sb.AppendFormat("{0} renderers", _renderers.Count);

        InfoTextBox.text = sb.ToString();
        

        RestartTiming();
    } 

    public void RestartTiming()
    {
        _stopwatch.Reset();
        _stopwatch.Start();
        _startFrame = Time.frameCount;

        DollyCart.m_Position = 0;
        DollyCart.m_Speed = 2.5f;
    }

    private void AddMaterialButton(MaterialMetadata mat)
    {
        if (ButtonPrefab == null || ButtonContainer == null)
        {
            Debug.LogError($"Can't create material button, missing prefab and/or container transform");
            return;
        }

        var btnObj = Instantiate<GameObject>(ButtonPrefab);
        btnObj.transform.SetParent(ButtonContainer, false);

        var txt = btnObj.GetComponentInChildren<TMP_Text>();
        if (txt != null)
            txt.text = mat.MaterialID;

        if (btnObj.TryGetComponent<Button>(out var button))
        {
            button.onClick.AddListener(() =>
            {
                SetMaterial(mat);
            });
        }

        btnObj.SetActive(true);
    }

    private void SetMaterial(MaterialMetadata mat)
    {
        Debug.Log($"Switching to material {mat.MaterialID}");

        if (_renderers != null)
        {
            foreach (var rend in _renderers)
            {
                if (rend == null)
                    continue;

                var mats = rend.sharedMaterials;
                if (mats == null || mats.Length <= 0)
                    continue;

                for (int i = 0; i < mats.Length; i++)
                    mats[i] = mat.LoadedMaterial;

                rend.sharedMaterials = mats;
            }
        }
        

        RestartTiming();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            RestartTiming();

        UpdateText();
    }

    private void UpdateText()
    {
        if (!_stopwatch.IsRunning)
            return;

        double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        int elapsedFrames = Time.frameCount - _startFrame;

        float fps = (float)((double)elapsedFrames / elapsedSeconds);

        FramerateTextBox.text = string.Format("{0:F2}", fps);
    }
}
