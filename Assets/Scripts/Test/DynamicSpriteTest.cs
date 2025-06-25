using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VectorGraphics;


public class DynamicSpriteTest : MonoBehaviour
{
    public Sprite DynamicSprite;

    private SpriteRenderer _rend;
    private SVGImage _svgImage;
    // Start is called before the first frame update
    void Start()
    {

        _rend = GetComponent<SpriteRenderer>();
        _svgImage = GetComponent<SVGImage>();

        Scene s = new Scene();
        s.Root = new SceneNode();
        s.Root.Shapes = new List<Shape>();


        //var path = new Vector2[]
        //{ 
        //    new Vector2(0, 0), 
        //    new Vector2(100, 100),
        //    new Vector2(0, 100),
        //    new Vector2(500, 100),
        //};
        var path = new List<Vector2>(20);

        for (int i = 0; i < 20; i++)
        {
            var v = new Vector2(Mathf.Sin(i * Mathf.PI / 10.0f) * 100, Mathf.Cos(i * Mathf.PI / 10.0f) * 100);
            path.Add(v);
        }

        BezierContour[] contours = new BezierContour[1];
        contours[0].Segments = BuildPath(path);
        contours[0].Closed = false;

        var stroke = new Stroke()
        {
            Color = Color.red,
            HalfThickness = 3.0f,
        };

        PathProperties pathProp = new PathProperties()
        {
            Corners = PathCorner.Round,
            Head = PathEnding.Round,
            Tail = PathEnding.Round,
            Stroke = stroke,
        };

        Shape shape = new Shape();
        shape.Contours = contours;
        shape.Fill = new SolidFill() { Color = new Color(0,0,0,0) };
        shape.PathProps = pathProp;
        shape.IsConvex = false;

        s.Root.Shapes.Add(shape);

        var opts = new VectorUtils.TessellationOptions()
        {
            MaxCordDeviation = 0.1f,
            MaxTanAngleDeviation = 0.1f,
            SamplingStepSize = 0.1f,
            StepDistance = 0.1f,
        };

        var geom = VectorUtils.TessellateScene(s, opts);

        DynamicSprite = VectorUtils.BuildSprite(geom, 100, VectorUtils.Alignment.TopLeft, new Vector2(0, 0), 16, false);

        //_rend.sprite = DynamicSprite;
    }

    Sprite BuildSprite(ICollection<Vector2> points, Color color, float lineWidth)
    {
        Scene s = new Scene();
        s.Root = new SceneNode();
        s.Root.Shapes = new List<Shape>();      

        BezierContour[] contours = new BezierContour[1];
        contours[0].Segments = BuildPath(points);
        contours[0].Closed = false;

        var stroke = new Stroke()
        {
            Color = color,
            HalfThickness = lineWidth,
        };

        PathProperties pathProp = new PathProperties()
        {
            Corners = PathCorner.Round,
            Head = PathEnding.Round,
            Tail = PathEnding.Round,
            Stroke = stroke,
        };

        Shape shape = new Shape();
        shape.Contours = contours;
        shape.Fill = null;//new SolidFill() { Color = new Color(0, 0, 0, 0) };
        shape.PathProps = pathProp;
        shape.IsConvex = false;

        s.Root.Shapes.Add(shape);

        var opts = new VectorUtils.TessellationOptions()
        {
            MaxCordDeviation = 10.1f,
            MaxTanAngleDeviation = 10.1f,
            SamplingStepSize = 10.1f,
            StepDistance = 10.1f,
        };

        var geom = VectorUtils.TessellateScene(s, opts);

        return VectorUtils.BuildSprite(geom, 100, VectorUtils.Alignment.TopLeft, new Vector2(0, 0), 16, false);
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

    // Update is called once per frame
    void Update()
    {
        var path = new List<Vector2>(20);

        for (int i = 0; i < 20; i++)
        {
            var v = new Vector2(Mathf.Sin(i * Mathf.PI / 10.0f) * 100, Mathf.Cos(i * Mathf.PI / 10.0f) * 100);
            v.x += Random.Range(-10, 10);
            v.y += Random.Range(-10, 10);
            path.Add(v);
        }

        if (_rend != null)
            _rend.sprite = BuildSprite(path, Color.blue, 2.0f);
        else if (_svgImage != null)
            _svgImage.sprite = BuildSprite(path, Color.blue, 2.0f);
    }
}
