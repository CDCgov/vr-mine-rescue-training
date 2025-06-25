using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIPointCloudCanvas : MonoBehaviour
{
    private class PointData
    {
        public Vector2 Pos;
        public Color Color;
        public GameObject Prefab;
    }

    private class LineData
    {
        public Vector2 P1;
        public Vector2 P2;
        public Color Color;
    }

    public GameObject PointPrefab;
    public string UnitsLabel = "mm";
    public TMP_Text XLabel;
    public TMP_Text YLabel;

    private RectTransform _rt;
    private List<PointData> _points;
    private List<LineData> _lines;

    private float _xMin, _xMax;
    private float _yMin, _yMax;
    private float _width, _height;

    private bool _rebuildPoints = false;

    private SVGImage _svgImage;
    private LineSpriteGenerator _lineSprite;

    Vector2 _origin = Vector2.zero;

    private void Awake()
    {
        _points = new List<PointData>();
        _lines = new List<LineData>();
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        _rt = GetComponent<RectTransform>();

        _svgImage = GetComponent<SVGImage>();
        if (_svgImage != null)
        {
            _lineSprite = new LineSpriteGenerator();// gameObject.AddComponent<LineSpriteGenerator>();


            System.Span<Vector2> points = stackalloc Vector2[2];

            _lineSprite.Clear();

            points[0] = new Vector2(0, 10);
            points[1] = new Vector2(0, -10);
            _lineSprite.AddPath(points, Color.green, 0.25f);

            points[0] = new Vector2(10, 0);
            points[1] = new Vector2(-10, 0);
            _lineSprite.AddPath(points, Color.blue, 0.25f);

            _lineSprite.UpdateSprite(gameObject);

        }

        //var rect = _rt.rect;
        //SpawnTestPoints(rect);
        //SpawnGlobalTestPoints();
    }

    public void SetAxesOrigin(Vector2 pt)
    {
        _origin = pt;
        _rebuildPoints = true;
    }

    public void AddPoint(float x, float y, Color color)
    {
        AddPoint(x, y, color, PointPrefab);
    }

    public void AddPoint(float x, float y, Color color, GameObject prefab)
    {
        PointData data = new PointData();
        data.Pos = new Vector2(x, y);
        data.Color = color;
        data.Prefab = prefab;

        _points.Add(data);

        //SpawnPoints();
        _rebuildPoints = true;
    }

    public void AddLine(Vector2 p1, Vector2 p2, Color color)
    {
        _lines.Add(new LineData
        {
            P1 = p1,
            P2 = p2,
            Color = color,
        });
    }

    private void DrawLine(Vector2 pt1, Vector2 pt2, Color color)
    {
        System.Span<Vector2> points = stackalloc Vector2[2];

        points[0] = pt1;
        points[1] = pt2;
        _lineSprite.AddPath(points, color, 1.0f);
    }

    private void UpdateAxes()
    {
        if (_lineSprite == null)
            return;

        Vector2[] points = new Vector2[2];

        _lineSprite.Clear();

        var origin = TransformPoint(_origin);

        //points[0] = origin + new Vector2(0, 10);
        //points[1] = origin - new Vector2(0, -10);
        //_lineSprite.AddPath(points, Color.green, 0.25f);

        //points[0] = origin + new Vector2(10, 0);
        //points[1] = origin - new Vector2(-10, 0);
        //_lineSprite.AddPath(points, Color.blue, 0.25f);

        //points[0] = new Vector2(_rt.rect.xMin, _rt.rect.yMin);
        //points[1] = new Vector2(_rt.rect.xMax, _rt.rect.yMin);
        //_lineSprite.AddPath(points, Color.cyan, 0.25f);

        //points[0] = new Vector2(origin.x, 100);
        //points[1] = new Vector2(origin.x, -100);
        //_lineSprite.AddPath(points, Color.green, 0.25f);

        //points[0] = new Vector2(_rt.rect.xMin, 0);
        //points[1] = new Vector2(_rt.rect.xMax, 0);
        //_lineSprite.AddPath(points, Color.blue, 0.25f);

        DrawLine(
            new Vector2(_rt.rect.xMin, _rt.rect.yMin),
            new Vector2(_rt.rect.xMax, _rt.rect.yMin),
            Color.clear);

        DrawLine(
            new Vector2(_rt.rect.xMin, _rt.rect.yMax),
            new Vector2(_rt.rect.xMax, _rt.rect.yMax),
            Color.clear);

        DrawLine(
            new Vector2(_rt.rect.xMin, _rt.rect.yMin),
            new Vector2(_rt.rect.xMin, _rt.rect.yMax),
            Color.clear);

        DrawLine(
            new Vector2(_rt.rect.xMax, _rt.rect.yMin),
            new Vector2(_rt.rect.xMax, _rt.rect.yMax),
            Color.clear);

        DrawLine(
            new Vector2(origin.x, _rt.rect.yMin),
            new Vector2(origin.x, _rt.rect.yMax),
            Color.white);

        DrawLine(
            new Vector2(_rt.rect.xMin, origin.y),
            new Vector2(_rt.rect.xMax, origin.y),
            Color.white);

        _lineSprite.UpdateSprite(gameObject);
    }

    public void ClearPoints()
    {
        _points.Clear();
        _lines.Clear();
        DespawnPoints();
    }

    private void SpawnPoints()
    {
        DespawnPoints();

        if (_points == null || _points.Count <= 0)
            return;

        ResetPointTransform();

        //make sure the origin is in bounds
        EncapsulatePoint(_origin); 
               
        //calculate point bounds
        foreach (var data in _points)
        {
            EncapsulatePoint(data.Pos);
        }

        foreach (var line in _lines)
        {
            EncapsulatePoint(line.P1);
            EncapsulatePoint(line.P2);
        }

        //expand slightly
        ExpandScaling(10);

        //make aspect match window
        FixScalingAspect();

        //spawn transformed points
        foreach (var data in _points)
        {
            var v = TransformPoint(data.Pos);
            SpawnPoint(new Vector3(v.x, v.y, 0), data.Color, data.Prefab);
        }

        //update labels
        if (XLabel != null)
            XLabel.text = $"{_xMax - _xMin:F0} {UnitsLabel}";
        if (YLabel != null)
            YLabel.text = $"{_yMax - _yMin:F0} {UnitsLabel}";
    }

    private void DrawLines()
    {
        foreach (var line in _lines)
        {
            var p1 = TransformPoint(line.P1);
            var p2 = TransformPoint(line.P2);
            DrawLine(p1, p2, line.Color);
        }

        _lineSprite.UpdateSprite(gameObject);
    }

    private void ResetPointTransform()
    {
        _xMin = _yMin = float.MaxValue;
        _yMax = _xMax = float.MinValue;
    }

    private void EncapsulatePoint(Vector2 v)
    {
        var x = v.x;
        var y = v.y;

        if (x > _xMax)
            _xMax = x;
        if (x < _xMin)
            _xMin = x;
        if (y > _yMax)
            _yMax = y;
        if (y < _yMin)
            _yMin = y;

        _width = _xMax - _xMin;
        _height = _yMax - _yMin;

        //avoid divide by zero
        if (Mathf.Abs(_width) < 0.001f)
            _width = 1;
        if (Mathf.Abs(_height) < 0.001f)
            _height = 1;
    }

    private void ExpandScaling(float amount)
    {
        _xMax += amount;
        _xMin -= amount;
        _yMax += amount;
        _yMin -= amount;

        _width = _xMax - _xMin;
        _height = _yMax - _yMin;
    }

    private void FixScalingAspect()
    {
        float a1 = _width / _height;
        float a2 = _rt.rect.width / _rt.rect.height;

        if (a1 > a2)
        {
            //our aspect ratio is wider than the window
            //expand vertical window to make aspect ratio match
            // _width / (_height + x) = a2; (_width / a2) - _height = x

            float expand = (_width / a2) - _height;

            _yMin -= expand / 2.0f;
            _yMax += expand / 2.0f;

            _height = _yMax - _yMin;

            Debug.Log($"UIPointCloud: Expanding height by {expand} a1:{(_width / _height):F2} a2:{a2:F2}");
        }
        else
        {
            //our aspect ratio is narrower than the window
            //expand horizontally to match aspect
            // (_width + x) / _height = a2; a2 * height - _width = x

            float expand = a2 * _height - _width;

            _xMin -= expand / 2.0f;
            _xMax += expand / 2.0f;

            _width = _xMax - _xMin;
            Debug.Log($"UIPointCloud: Expanding width by {expand} a1:{(_width / _height):F2} a2:{a2:F2}");
        }
    }

    private Vector2 TransformPoint(Vector2 v)
    {
        //normalize to 0 -> 1
        v.x = (v.x - _xMin) / _width;
        v.y = (v.y - _yMin) / _height;

        //scale to rect transform's rect
        v.x = _rt.rect.xMin + _rt.rect.width * v.x;
        v.y = _rt.rect.yMin + _rt.rect.height * v.y;

        return v;
    }

    void SpawnTestPoints(Rect rt)
    {
        DespawnPoints();
        
        for (int i = 0; i < 100; i++)
        {

            var x = rt.xMin + rt.width * Random.value;
            var y = rt.yMin + rt.height * Random.value;

            Vector3 v = new Vector3(x, y, 0);
            SpawnPoint(v, Color.green, PointPrefab);
        }
    }

    void SpawnGlobalTestPoints()
    {
        for (int i = 0; i < 100; i++)
        {
            var v = Random.insideUnitCircle;

            //v.x = Mathf.Abs(v.x);
            v += new Vector2(0.5f, 0.5f);

            AddPoint(v.x * Random.value * 1000.0f,
                v.y * Random.value * 1000.0f,
                Color.blue);
        }

        SetAxesOrigin(new Vector2(500, 500));
    }

    void DespawnPoints()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
    }

    void SpawnPoint(Vector3 pos, Color color, GameObject prefab)
    {
        if (prefab == null)
            return;

        var obj = GameObject.Instantiate<GameObject>(prefab, transform, false);
        if (obj == null)
            return;

        //obj.transform.SetParent(transform);
        obj.transform.localPosition = pos;

        var graphic = obj.GetComponent<Graphic>();
        if (graphic != null)
            graphic.color = color;
    }

    private void FixedUpdate()
    {
        if (_rebuildPoints)
        {
            SpawnPoints();
            UpdateAxes();
            DrawLines();

            _rebuildPoints = false;
        }
    }
}
