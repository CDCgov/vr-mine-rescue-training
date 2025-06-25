using System.Collections.Generic;
using UnityEngine;

public class SortedVerts : MonoBehaviour
{
    //left (-x), right(+x), bottom (-y), top (+y) (le-left, ri-right,to-top,bo-bottom)
    public List<Vector3> le = new List<Vector3>();
    public List<Vector3> ri = new List<Vector3>();
    public List<Vector3> to = new List<Vector3>();
    public List<Vector3> bo = new List<Vector3>();
}