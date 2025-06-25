using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class DeformableShell
{
    [HideInInspector]
    public float[] ShellMap;
    public int Width;
    public int Height;

    public float MaxValue = 0;

    //[System.NonSerialized]
    //public DynamicMesh VisMesh;

    //private Vector3[] _vertices;
    //private int[] _triangles;

    public DeformableShell(int width, int height)
    {
        ShellMap = new float[height * width];
        Width = width;
        Height = height;
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(MaxValue);

        int len = ShellMap.Length;
        writer.Write(len);			
        for (int i = 0; i < len; i++)
        {
            writer.Write(ShellMap[i]);
        }
    }

    public void Deserialize(BinaryReader reader)
    {
        Width = reader.ReadInt32();
        Height = reader.ReadInt32();
        MaxValue = reader.ReadSingle();

        int len = reader.ReadInt32();

        if (ShellMap == null || ShellMap.Length != len)
        {
            ShellMap = new float[len];
        }

        for (int i = 0; i < len; i++)
        {
            ShellMap[i] = reader.ReadSingle();
        }
    }

    public void SetValue(int x, int y, float value)
    {
        ShellMap[y * Width + x] = value;
        if (value > MaxValue)
            MaxValue = value;
    }

    public float GetDist(Vector2 coord)
    {
        return GetValue((int)coord.x, (int)coord.y);
    }

    public float GetValue(int x, int y)
    {
        if (y >= Height)
            y = Height - 1;

        if (x >= Width)
            x = Width - 1;

        int index = y * Width + x;
        //if (index >= ShellMap.Length || index < 0)
        //{
        //	//Debug.Log(StackTraceUtility.ExtractStackTrace());
        //	//Debug.LogFormat("x: {0} y: {1}", x, y);
        //	return 0;
        //})
        return ShellMap[index];
    }

    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                ShellMap[y * Width + x] = 0;
            }
        }

        MaxValue = 0;
    }

    public void ConvertToTexture(ref Texture2D tex, Gradient colorGradient)
    {
        if (tex == null || tex.width != Width || tex.height != Height)
        {
            tex = new Texture2D(Width, Height, TextureFormat.ARGB32, false);
        }

        Color c = Color.white;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float value = GetValue(x,y);
                float ratio = value / MaxValue;

                c = colorGradient.Evaluate(ratio);
                tex.SetPixel(x, y, c);
            }
        }

        
    }

    public void DeformArea(Vector2 coord, float radius, float peakAmplitude)
    {
        int originX = Mathf.RoundToInt(coord.x);
        int originY = Mathf.RoundToInt(coord.y);

        int startX = originX - (int)radius;
        int startY = originY - (int)radius;

        int endX = originX + (int)radius;
        int endY = originY + (int)radius;


        //Debug.LogFormat("{0},{1}  {2},{3},  {4},{5}", originX, originY, startX, startY, endX, endY);

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                int wx = x;
                int wy = y;

                //wrap negatives / out of bounds around to the other side
                WrapCoordinate(ref wx, ref wy);

                //don't wrap y value, just discard - wrapping y value puts you on the opposite side
                if (y < 0 || y >= Height)
                    continue;

                float xdist = (float)(x - originX);
                float ydist = (float)(y - originY);
                float dist = xdist * xdist + ydist * ydist;
                dist = Mathf.Sqrt(dist);

                float scale = 1.0f - (dist / radius);
                scale = Mathf.Clamp(scale, 0, 1);

                float curVal = GetValue(wx, wy);

                curVal += peakAmplitude * scale;

                if (curVal < 0.00001)
                    curVal = 0.00001f;

                SetValue(wx, wy, curVal);

                //Debug.LogFormat("Changing {0},{1} by {2}", wx, wy, peakAmplitude * scale);
            }
        }
    }

    public void WrapCoordinate(ref int x, ref int y)
    {
        if (x < 0)
            x += Width;
        else if (x >= Width)
            x -= Width;

        /*
        if (y < 0)
            y += Height;
        else if (y >= Height)
            y -= Height;
            */
    }
    

    /*
    public void UpdateMesh()
    {
        if (VisMesh == null)
            VisMesh = new DynamicMesh();

        VisMesh.Clear();

        for (int y = 0; y < Height; y += 3)
        {
            for (int x = 0; x < Width; x += 3)
            {
                float value = GetValue(x, y);
                Vector3 v = DeformableProxSystem.CoordinateToVector(new Vector2(x, y));
                Vector3 pos = v * value;

                VisMesh.AddMarker(pos, 0.02f);
            }
        }

        VisMesh.UpdateMesh();
    }

    /*
    public void UpdateMesh()
    {
        if (VisMesh == null)
            VisMesh = new Mesh();

        int numVertices = Width * Height * 5;
        int numTriangles = Width * Height * 6;
        float size = 0.05f;

        if (_vertices == null ||  _vertices.Length != numVertices)
        {
            _vertices = new Vector3[numTriangles];
        }

        
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float value = GetValue(x, y);				
                Vector3 v = DeformableProxSystem.CoordinateToVector(new Vector2(x, y));

                v = v * value;

                int voffset = x * y * 5;

                _vertices[voffset + 0] = v + new Vector3(-size, -size, -size);
                _vertices[voffset + 1] = v + new Vector3(-size, -size,  size);
                _vertices[voffset + 2] = v + new Vector3( size, -size,  size);
                _vertices[voffset + 3] = v + new Vector3( size, -size, -size);
                _vertices[voffset + 4] = v + new Vector3(0, size, 0);
            }			
        }

        VisMesh.vertices = _vertices;

        if (_triangles == null || _triangles.Length != numTriangles * 3)
        {
            _triangles = new int[numTriangles * 3];

            for (int i = 0; i < Width * Height; i++)
            {
                int index = i * 6 * 3; //index in triangle array
                int voffset = i * 5; //starting offset into vertex array

                _triangles[index + 0] = voffset + 0;
                _triangles[index + 1] = voffset + 1;
                _triangles[index + 2] = voffset + 2;

                _triangles[index + 3] = voffset + 2;
                _triangles[index + 4] = voffset + 3;
                _triangles[index + 5] = voffset + 0;

                _triangles[index + 6] = voffset + 0;
                _triangles[index + 7] = voffset + 4;
                _triangles[index + 8] = voffset + 3;

                _triangles[index + 9] = voffset + 3;
                _triangles[index + 10] = voffset + 4;
                _triangles[index + 11] = voffset + 2;

                _triangles[index + 12] = voffset + 2;
                _triangles[index + 13] = voffset + 4;
                _triangles[index + 14] = voffset + 3;

                _triangles[index + 15] = voffset + 1;
                _triangles[index + 16] = voffset + 4;
                _triangles[index + 17] = voffset + 0;
            }

            VisMesh.triangles = _triangles;
        }

        VisMesh.RecalculateBounds();
        VisMesh.RecalculateNormals();
    } */
}
