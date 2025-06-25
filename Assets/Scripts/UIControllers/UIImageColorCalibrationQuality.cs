using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImageColorCalibrationQuality : MonoBehaviour, ISelectedPlayerView
{
    [System.Serializable]
    public struct CalQualityBand
    {
        public float MaxDistance;
        public Color Color;
    }

    public SystemManager SystemManager;
    public PlayerManager PlayerManager;

    public List<CalQualityBand> CalibrationBands;

    private PlayerRepresentation _player;
    private Image _image;

    void Reset()
    {
        CalibrationBands = new List<CalQualityBand>();
        CalibrationBands.Add(new CalQualityBand
        {
            MaxDistance = 120.0f,
            Color = Color.green,
        });

        CalibrationBands.Add(new CalQualityBand
        {
            MaxDistance = 250.0f,
            Color = Color.yellow,
        });

        CalibrationBands.Add(new CalQualityBand
        {
            MaxDistance = -1,
            Color = Color.red,
        });
    }

    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        PlayerManager.TestPointCentroidChanged += OnTestPointCentroidChanged;

        _image = GetComponent<Image>();

        UpdateColor();
    }

    private void OnTestPointCentroidChanged()
    {
        try
        {
            UpdateColor();
        }
        catch (System.Exception) { }
    }

    void OnDestroy()
    {
        PlayerManager.TestPointCentroidChanged -= OnTestPointCentroidChanged;
        ClearPlayer();
    }

    public void ClearPlayer()
    {
        if (_player != null)
        {
            _player.CalTestPointChanged -= OnCalTestPointChanged;
            _player = null;
        }
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        ClearPlayer();

        _player = player;
        _player.CalTestPointChanged += OnCalTestPointChanged;

        UpdateColor();
    }

    private void OnCalTestPointChanged(Vector3 pt)
    {
        UpdateColor();
    }

    void UpdateColor()
    {
        if (_player == null || _image == null || CalibrationBands == null || CalibrationBands.Count <= 0)
            return;

        //var testPt = SystemManager.SystemConfig.CalibrationTestPointVec3;
        var testPt = PlayerManager.ComputeTestPointCentroid();
        var poiSpace = _player.CalTestPoint;

        float xzDist = 0;
        if (testPt == Vector3.zero)
        {
            xzDist = float.MaxValue;
        }
        else
        {
            //compute XZ plane error
            testPt.y = 0;
            poiSpace.y = 0;
            xzDist = Vector3.Distance(testPt, poiSpace) * 1000.0f;
        }

        var color = CalibrationBands[CalibrationBands.Count - 1].Color;
        for (int i = 0; i < CalibrationBands.Count - 1; i++)
        {
            if (xzDist < CalibrationBands[i].MaxDistance)
            {
                color = CalibrationBands[i].Color;
                break;
            }
        }

        _image.color = color;
    }
}
