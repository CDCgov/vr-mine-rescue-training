using System.Collections.Generic;
using UnityEngine;

public class FoamOptions : MonoBehaviour
{
    //Vector3 lists for vertices along the stopping perimeter
    public List<Vector3> frontFoamLeft = new List<Vector3>();
    public List<Vector3> frontFoamTop = new List<Vector3>();
    public List<Vector3> frontFoamRight = new List<Vector3>();
    public List<Vector3> backFoamLeft = new List<Vector3>();
    public List<Vector3> backFoamTop = new List<Vector3>();
    public List<Vector3> backFoamRight = new List<Vector3>();
    public Transform stopTrans;
}