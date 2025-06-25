using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIPointCloudCanvas))]
public class UIPlayerTestPointCloud : MonoBehaviour
{
    public SystemManager SystemManager;
    public PlayerManager PlayerManager;
    public PlayerColorManager PlayerColorManager;

    public GameObject RefPointPrefab;

    private UIPointCloudCanvas _pointCloud;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (PlayerColorManager == null)
            PlayerColorManager = PlayerColorManager.GetDefault();

        _pointCloud = GetComponent<UIPointCloudCanvas>();

        PlayerManager.PlayerJoined += OnPlayerJoined;
        PlayerManager.PlayerLeft += OnPlayerLeft;

        SystemManager.SystemConfig.ConfigSaved += OnConfigSaved;

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            OnPlayerJoined(player);
        }

        UpdatePointCloud();

        //InvokeRepeating(nameof(AddTestPoints), 0, 5.0f);
        //AddTestPoints();
    }
    private void OnDestroy()
    {
        SystemManager.SystemConfig.ConfigSaved -= OnConfigSaved;

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            OnPlayerLeft(player);
        }
    }

    private void OnConfigSaved()
    {
        UpdatePointCloud();
    }

    private void OnEnable()
    {
        Debug.Log($"UIPlayerTestPointCloud Enabled");
        if (_pointCloud != null)
        {
            //refresh point cloud if the control is re-enabled
            UpdatePointCloud();
        }
    }


    private void OnPlayerLeft(PlayerRepresentation player)
    {
        player.CalTestPointChanged -= OnCalTestPointChanged;
    }

    private void OnPlayerJoined(PlayerRepresentation player)
    {
        player.CalTestPointChanged += OnCalTestPointChanged;
    }

    private void OnCalTestPointChanged(Vector3 obj)
    {
        UpdatePointCloud();
    }

    void AddTestPoints()
    {
        _pointCloud.ClearPoints();

        for (int i = 0; i < 100; i++)
        {
            var v = UnityEngine.Random.insideUnitCircle;
            v.y *= 0.25f;

            //v.x = Mathf.Abs(v.x);
            v += new Vector2(0.5f, 0.5f);

            _pointCloud.AddPoint(v.x * UnityEngine.Random.value * 50.0f,
                v.y * UnityEngine.Random.value * 50.0f,
                UnityEngine.Random.ColorHSV(0, 1, 1, 1, 1, 1));
        }

        Vector2 refPoint = new Vector2(25, 25);
        DrawBox(refPoint, 250);

        _pointCloud.AddPoint(refPoint.x, refPoint.y, Color.white, RefPointPrefab);
        _pointCloud.SetAxesOrigin(refPoint);
    }

    void DrawBox(Vector2 origin, float size)
    {
        var color = Color.cyan;
        var halfSize = size / 2.0f;

        var p1 = new Vector2(origin.x - halfSize, origin.y + halfSize);
        var p2 = new Vector2(origin.x + halfSize, origin.y + halfSize);
        var p3 = new Vector2(origin.x + halfSize, origin.y - halfSize);
        var p4 = new Vector2(origin.x - halfSize, origin.y - halfSize);

        _pointCloud.AddLine(p1, p2, color);
        _pointCloud.AddLine(p2, p3, color);
        _pointCloud.AddLine(p3, p4, color);
        _pointCloud.AddLine(p4, p1, color);
    }


    void UpdatePointCloud()
    {
        _pointCloud.ClearPoints();

        //add reference point location
        if (RefPointPrefab != null)
        {
            //var refPoint = SystemManager.SystemConfig.CalibrationTestPointVec3;
            var refPoint = PlayerManager.ComputeTestPointCentroid();
            refPoint *= 1000.0f; //convert to mm

            _pointCloud.AddPoint(refPoint.x, refPoint.z, Color.white, RefPointPrefab);
            _pointCloud.SetAxesOrigin(new Vector2(refPoint.x, refPoint.z));

            DrawBox(new Vector2(refPoint.x, refPoint.z), 250);
        }        

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            var pt = player.CalTestPoint;
            if (pt == Vector3.zero)
                continue;

            var color = PlayerColorManager.GetPlayerColor(player.PlayerRole);

            //convert to mm
            pt *= 1000.0f;

            _pointCloud.AddPoint(pt.x, pt.z, color);
        }
    }
}
