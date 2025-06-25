using UnityEngine;
using System.Collections;

public class CatmullRomSpline {
    
    private class CalcData
    {
        public CalcData()
        {
            data = new Vector4[4];
        }
            
        public Vector4[] data;
    }
    
    private Vector3[] _points;			
    private CalcData[] _precalc;
    private int _numPoints;	
    private int _numSegments;
    
    private static Matrix4x4 _catmullRom;
    
    static CatmullRomSpline()
    {
        _catmullRom = new Matrix4x4();
        _catmullRom.SetRow(0, new Vector4(0, 2, 0, 0));
        _catmullRom.SetRow(1, new Vector4(-1, 0, 1, 0));
        _catmullRom.SetRow(2, new Vector4(2, -5, 4, -1));
        _catmullRom.SetRow(3, new Vector4(-1, 3, -3, 1));
    }
    
    public void Init(Vector3[] points)
    {
        _points = points;
        _numPoints = _points.Length;
        _numSegments = _numPoints - 1;
        
        UpdateSpline();
    }
    
    public void UpdateSpline()
    {
        if (_precalc == null || _precalc.Length != _numSegments)
            _precalc = new CalcData[_numSegments];
        
        for (int i = 0; i < (_numPoints - 1); i++)
        {
            Vector3 p0, p1, p2, p3;
            CalcData calc;
            
            if (_precalc[i] == null)
                calc = new CalcData();
            else
                calc = _precalc[i];

            p1 = _points[i];
            p2 = _points[i + 1];

            if (i <= 0)
            {
                p0 = p1 + (p1 - p2);
            }
            else
                p0 = _points[i - 1];
            
            if (i + 2 >= _numPoints)
            {
                p3 = p2 + (p2 - p1);
            }
            else
                p3 = _points[i + 2];
            
            calc.data[0] = 2 * p1;
            calc.data[1] = -1 * p0 + p2;
            calc.data[2] = 2 * p0 - 5 * p1 + 4 * p2 - p3;
            calc.data[3] = -1 * p0 + 3 * p1 - 3 * p2 + p3;
            
            _precalc[i] = calc;
        }
    }
    
    private void ComputeSegment(float t, out int segment, out float tseg)
    {
        t *= _numSegments;
        segment = Mathf.FloorToInt(t);
        if (segment >= _numSegments)
        {
            segment = _numSegments - 1;
            tseg = 1;
        }
        else
            tseg = t - segment;
        
    }
    
    public Vector3 Evaluate(float t)
    {
        int segment;
        ComputeSegment(t, out segment, out t);
        return Evaluate(segment, t);
    }
    
    public Vector3 Evaluate(int segment, float t)
    {
        Vector3 result = Vector3.zero;
        CalcData calc = _precalc[segment];
        
        result = 
            calc.data[0] + 
            calc.data[1] * t +
            calc.data[2] * t * t +
            calc.data[3] * t * t * t;
        
        result *= 0.5f;
        
        return result;
    }
    
    public Vector3 EvaluateTangent(float t)
    {
        int segment;
        ComputeSegment(t, out segment, out t);
        return EvaluateTangent(segment, t);

    }
    
    public Vector3 EvaluateTangent(int segment, float t)
    {
        Vector3 result = Vector3.zero;
        CalcData calc = _precalc[segment];
        
        result =  
            calc.data[1] +
            calc.data[2] * 2 * t +
            calc.data[3] * 3 * t * t;
        
        result *= 0.5f;
        
        return result;
    }
}
