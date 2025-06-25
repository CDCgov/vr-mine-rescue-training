using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Class for min/max along x,y,z axes.
public class VertExtr : MonoBehaviour
{        
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
        public float minZ;
        public float maxZ;
        public VertExtr(List<Vector3> _vectIn)
        {
            minX = _vectIn.Min(Vector3 => Vector3.x);
            maxX = _vectIn.Max(Vector3 => Vector3.x);
            minY = _vectIn.Min(Vector3 => Vector3.y);
            maxY = _vectIn.Max(Vector3 => Vector3.y);
            minZ = _vectIn.Min(Vector3 => Vector3.z);
            maxZ = _vectIn.Max(Vector3 => Vector3.z);
        }
        public VertExtr(Vector3[] _vectIn)
        {
            minX = _vectIn.Min(Vector3 => Vector3.x);
            maxX = _vectIn.Max(Vector3 => Vector3.x);
            minY = _vectIn.Min(Vector3 => Vector3.y);
            maxY = _vectIn.Max(Vector3 => Vector3.y);
            minZ = _vectIn.Min(Vector3 => Vector3.z);
            maxZ = _vectIn.Max(Vector3 => Vector3.z);
        }   
}