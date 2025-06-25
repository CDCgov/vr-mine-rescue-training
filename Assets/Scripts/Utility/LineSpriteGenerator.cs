using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VectorGraphics;
using g3;
using System.Runtime.Serialization;
using System.Text;

public class LineSpriteGenerator// : MonoBehaviour
{
    public struct LineSpritePath
    {
        public BezierPathSegment[] BezierSegments;
        public Color Color;
        public float LineWidth;

        public void Write(BinaryWriter writer)
        {
            if (BezierSegments == null || BezierSegments.Length == 0)
            {
                writer.Write((int)0);
            }
            else
            {
                writer.Write((int)BezierSegments.Length);

                for (int i = 0; i < BezierSegments.Length; i++)
                {
                    var seg = BezierSegments[i];

                    writer.WriteVector(seg.P0);
                    writer.WriteVector(seg.P1);
                    writer.WriteVector(seg.P2);
                }
            }

            //change to not serialize color & line width
            //writer.Write((float)Color.r);
            //writer.Write((float)Color.g);
            //writer.Write((float)Color.b);

            //writer.Write((float)LineWidth);
        }

        public void Read(BinaryReader reader)
        {
            var numSegs = reader.ReadInt32();

            if (numSegs == 0)
            {
                BezierSegments = null;
            }
            else
            {
                BezierSegments = new BezierPathSegment[numSegs];

                for (int i = 0; i < numSegs; i++)
                {
                    var seg = new BezierPathSegment();

                    seg.P0 = reader.ReadVector2();
                    seg.P1 = reader.ReadVector2();
                    seg.P2 = reader.ReadVector2();

                    BezierSegments[i] = seg;
                }
            }

            //var r = reader.ReadSingle();
            //var g = reader.ReadSingle();
            //var b = reader.ReadSingle();
            //Color = new Color(r, g, b);

            //LineWidth = reader.ReadSingle();

            //just set default color & line width
            Color = Color.black;
            LineWidth = 0.3f;
        }

    }

    public int PathCount
    {
        get
        {
            if (_paths == null)
                return 0;

            return _paths.Count;
        }
    }

    private List<LineSpritePath> _paths;

    //private SpriteRenderer _renderer;
    //private SVGImage _svgImage;
    private int _pointCount = 0;

    private DGraph2 _dgraph2 = null;
    private Color _segColor;
    private float _segLineWidth;
    private Scene _scene;

    public void AddPath(ICollection<Vector2> points, Color color, float lineWidth)
    {
        LineSpritePath path = new LineSpritePath();
        path.BezierSegments = BuildPath(points);
        path.Color = color;
        path.LineWidth = lineWidth;

        _pointCount += points.Count;

        _paths.Add(path);
    }

    public void AddPath(System.Span<Vector2> points, Color color, float lineWidth)
    {
        LineSpritePath path = new LineSpritePath();
        path.BezierSegments = BuildPath(points);
        path.Color = color;
        path.LineWidth = lineWidth;

        _pointCount += points.Length;

        _paths.Add(path);
    }

    public void AddSmoothPath(IList<Vector2> points, Color color, float lineWidth)
    {
        LineSpritePath path = new LineSpritePath();
        path.BezierSegments = BuildSmoothPath(points);
        path.Color = color;
        path.LineWidth = lineWidth;

        _pointCount += points.Count;

        _paths.Add(path);
    }

    public void AddSegment(Vector2 p1, Vector2 p2, Color color, float lineWidth)
    {
        if (_dgraph2 == null)
            _dgraph2 = new DGraph2();

        _segColor = color;
        _segLineWidth = lineWidth;

        Vector2d v1 = new Vector2d(p1);
        Vector2d v2 = new Vector2d(p2);

        int i1 = -1;
        int i2 = -2;

        for (int i = 0; i < _dgraph2.VertexCount; i++)
        {
            var v = _dgraph2.GetVertex(i);
            if (i1 < 0)
            {
                if (v.Distance(v1) < 0.1)
                {
                    i1 = i;
                }
            }

            if (i2 < 0)
            {
                if (v.Distance(v2) < 0.1)
                {
                    i2 = i;
                }
            }

            if (i1 >= 0 && i2 >= 0)
                break;
        }

        if (i1 < 0)
            i1 = _dgraph2.AppendVertex(v1);
        if (i2 < 0)
            i2 = _dgraph2.AppendVertex(v2);

        _dgraph2.AppendEdge(i1, i2);

        //Vector2[] pts = new Vector2[4];
        //pts[0] = p1;
        //pts[1] = p1;
        //pts[2] = p2;
        //pts[3] = p2;

        Vector2[] pts = new Vector2[2];
        pts[0] = p1;
        pts[1] = p2;


       // AddPath(pts, color, lineWidth);
    }

    public void Clear()
    {
        _paths = new List<LineSpritePath>();
        _dgraph2 = null;
    }

    public void Write(BinaryWriter writer)
    {
        if (_paths == null || _paths.Count <= 0)
        {
            writer.Write((int)0);
            return;
        }

        writer.Write((int)_paths.Count);

        for (int i = 0; i < _paths.Count; i++)
        {
            var path = _paths[i];

            path.Write(writer);
        }
    }

    public void Read(BinaryReader reader, Color mapWallColor, float lineWidth)
    {
        Clear();

        var numPaths = reader.ReadInt32();
        if (numPaths <= 0)
            return;

        for (int i = 0; i < numPaths; i++)
        {
            var path = new LineSpritePath();
            path.Read(reader);
            path.Color = mapWallColor;
            path.LineWidth = lineWidth;

            _paths.Add(path);
        }

        Debug.Log($"LineSpriteGenerator::Read Read {_paths.Count} paths");
    }

    public void UpdateSprite(GameObject obj)
    {
        //if (_svgImage != null)
        //{
        //    _svgImage.sprite = BuildSprite();
        //}
        //else if (_renderer != null)
        //{
        //    _renderer.sprite = BuildSprite();
        //}

        if (obj.TryGetComponent<SVGImage>(out var svgImage))
        {
            svgImage.sprite = BuildSprite();
        }
        else if (obj.TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.sprite = BuildSprite();
        }
    }

    public void UpdateSprite(float lineWidth, GameObject obj)
    {
        //if (_svgImage != null)
        //{
        //    _svgImage.sprite = RebuildSprite(lineWidth);
        //}
        //else if (_renderer != null)
        //{
        //    _renderer.sprite = RebuildSprite(lineWidth);
        //}

        if (obj.TryGetComponent<SVGImage>(out var svgImage))
        {
            svgImage.sprite = RebuildSprite(lineWidth);
        }
        else if (obj.TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.sprite = RebuildSprite(lineWidth);
        }
    }

    public LineSpriteGenerator()
    {
        _paths = new List<LineSpritePath>();
    }

    //void Awake()
    //{
    //    _renderer = GetComponent<SpriteRenderer>();
    //    _svgImage = GetComponent<SVGImage>();
    //}

    BezierPathSegment[] BuildPath(System.Span<Vector2> points)
    {
        var segments = new BezierPathSegment[points.Length];
        int i = 0;

        foreach (var pt in points)
        {
            segments[i] = new BezierPathSegment() { P0 = pt, P1 = pt, P2 = pt };
            i++;
        }

        return segments;
    }

    BezierPathSegment[] BuildPath(ICollection<Vector2> points)
    {
        var segments = new BezierPathSegment[points.Count];
        int i = 0;

        foreach (var pt in points)
        {
            segments[i] = new BezierPathSegment() { P0 = pt, P1 = pt, P2 = pt };
            i++;
        }

        return segments;
    }

    BezierPathSegment[] BuildSmoothPath(IList<Vector2> points)
    {
        var segments = new BezierPathSegment[points.Count];

        Vector2 pt1, pt2, pt3;
        int i;

        for (i = 0; i < points.Count-1; i++)
        {
            //    if (i > 0)
            //        pt1 = points[i - 1];
            //    else
            //        pt1 = points[i];

            //    pt2 = points[i];

            //    if (i < points.Count - 1)
            //        pt3 = points[i + 1];
            //    else
            //        pt3 = points[i];

            //    segments[i] = new BezierPathSegment() { P0 = pt1, P1 = (pt1+pt2)/2.0f, P2 = pt2 };

            pt1 = points[i];
            pt3 = points[i + 1];
            pt2 = (pt1 + pt3) * 0.5f;

            segments[i] = new BezierPathSegment() { P0 = pt1, P1 = pt2, P2 = pt3 };
        }

        i = points.Count - 1;
        pt1 = points[i];
        pt3 = points[i];
        pt2 = (pt1 + pt3) * 0.5f;

        segments[i] = new BezierPathSegment() { P0 = pt1, P1 = pt2, P2 = pt3 };
        

        return segments;
    }

    public void BuildPaths()
    {
        if (_dgraph2 == null)
            return;

        _paths.Clear();
        
        Debug.Log($"Graph contains {_dgraph2.VertexCount} vertices and {_dgraph2.EdgeCount} edges");

        var curves = DGraph2Util.ExtractCurves(_dgraph2);

        List<Vector2> points = new List<Vector2>(1000);

        //Vector3 unityVec;
        //Vector3f g3Vec = Vector3f.Zero;
        //unityVec = g3Vec;
        //g3Vec = unityVec;

        foreach (var loop in curves.Loops)
        {
            //Debug.Log($"Loop with {loop.VertexCount} vertices");

            loop.Simplify();

            points.Clear();
            for (int i = 0; i < loop.VertexCount; i++)
            {
                points.Add(new Vector2((float)loop.Vertices[i].x, (float)loop.Vertices[i].y));
            }

            points.Add(new Vector2((float)loop.Vertices[0].x, (float)loop.Vertices[0].y));

            AddSmoothPath(points, _segColor, _segLineWidth);
        }

        foreach (var line in curves.Paths)
        {
            //Debug.Log($"Path with {line.VertexCount} vertices");

            line.Simplify();

            //AddPath(line.Vertices, _segColor, _segLineWidth);
            points.Clear();
            for (int i = 0; i < line.VertexCount; i++)
            {
                points.Add(new Vector2((float)line.Vertices[i].x, (float)line.Vertices[i].y));
            }

            AddPath(points, _segColor, _segLineWidth);
        }
        
    }

    Sprite BuildSprite()
    {
        if (_paths == null || _paths.Count <= 0)
            BuildPaths();

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        ///////////////////////////////////Serialization Test/////////////////////
        //using (MemoryStream memStream = new MemoryStream())
        //{ 
        //    using (BinaryWriter writer = new BinaryWriter(memStream, Encoding.UTF8, true)) 
        //    {
        //        sw.Start();
        //        Write(writer);
        //        sw.Stop();

        //        Debug.Log($"LineSpriteGenerator: Writing line sprite took {sw.ElapsedMilliseconds}ms");
        //    }

        //    memStream.Seek(0, SeekOrigin.Begin);

        //    using (BinaryReader reader = new BinaryReader(memStream, Encoding.UTF8, true))
        //    {
        //        sw.Reset();
        //        sw.Start();
        //        Read(reader);
        //        sw.Stop();

        //        Debug.Log($"LineSpriteGenerator: Reading line sprite took {sw.ElapsedMilliseconds}ms");
        //    }
            
            
        //}

        //////////////////////////////////////////////////////////////////////////

        sw.Restart();

        _scene = new Scene();
        _scene.Root = new SceneNode();
        _scene.Root.Shapes = new List<Shape>();

        Debug.Log($"Building sprite with {_pointCount} points");

        foreach (var path in _paths)
        {
            var stroke = new Stroke()
            {
                Color = path.Color,
                HalfThickness = path.LineWidth,
            };

            PathProperties pathProp = new PathProperties()
            {
                Corners = PathCorner.Round,
                Head = PathEnding.Round,
                Tail = PathEnding.Round,
                Stroke = stroke,
            };

            BezierContour[] contours = new BezierContour[1];
            contours[0].Segments = path.BezierSegments;
            contours[0].Closed = false;

            Shape shape = new Shape();
            shape.Contours = contours;
            shape.Fill = null;//new SolidFill() { Color = new Color(0, 0, 0, 0) };
            shape.PathProps = pathProp;
            shape.IsConvex = false; 

            _scene.Root.Shapes.Add(shape);
        }

        var sprite = BuildSprite(_scene);

        sw.Stop();
        Debug.Log($"LineSpriteGenerator: Build sprite took {sw.ElapsedMilliseconds} ms");

        return sprite;

        //var opts = new VectorUtils.TessellationOptions()
        //{
        //    MaxCordDeviation = 1.0f,
        //    MaxTanAngleDeviation = 1.1f,
        //    SamplingStepSize = 5.0f,
        //    StepDistance = 3.0f,
        //};

        //var geom = VectorUtils.TessellateScene(_scene, opts);

        //return VectorUtils.BuildSprite(geom, 100, VectorUtils.Alignment.TopLeft, new Vector2(0, 0), 16, false);
    }

    private Sprite RebuildSprite(float lineWidth)
    {
        if (_scene == null || _scene.Root == null)
            return null;

        foreach (var shape in _scene.Root.Shapes) 
        {
            shape.PathProps.Stroke.HalfThickness = lineWidth;
        }

        return BuildSprite(_scene);
    }

    private Sprite BuildSprite(Scene s)
    {
        var opts = new VectorUtils.TessellationOptions()
        {
            MaxCordDeviation = 1.0f,
            MaxTanAngleDeviation = 1.1f,
            SamplingStepSize = 5.0f,
            StepDistance = 3.0f,
        };

        var geom = VectorUtils.TessellateScene(s, opts);

        return VectorUtils.BuildSprite(geom, 100, VectorUtils.Alignment.TopLeft, new Vector2(0, 0), 16, false);
    }

}
