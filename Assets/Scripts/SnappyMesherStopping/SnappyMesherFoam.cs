using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnappyMesherFoam : MonoBehaviour
{
    public GameObject foam;
    public void SnapFoam(float stopZed, float rotFoam, Transform stopTrans, Transform foamTrans, List<Vector3> transLeft, List<Vector3> transUp, List<Vector3> transRight)
    {
        //Get "foam" object, mesh and local verts
        var testFoam = Instantiate(foam, new Vector3(stopTrans.position.x, stopTrans.position.y, stopTrans.TransformPoint(0, 0, stopZed).z), foamTrans.rotation, stopTrans);
        testFoam.transform.Rotate(0, rotFoam, 0);
        Mesh foamMesh = Mesh.Instantiate(testFoam.GetComponent<MeshFilter>().sharedMesh);
        Vector3[] foamVerts = foamMesh.vertices;
        Mesh tempFoamMesh = Mesh.Instantiate(testFoam.GetComponent<MeshFilter>().sharedMesh);
        var newFoamVerts = tempFoamMesh.vertices;
        //Get distinct vertices from foam (removes doubled vertices at edges/corners)
        var foamDistinctVerts = (foamVerts.Distinct()).ToList();
        var foamTileVerts = new List<Vector3>();
        //Convert distinct foam verts from foam local space to stopping local space
        for (var i = 0; i < foamDistinctVerts.Count; i++)
        {
            foamTileVerts.Add(stopTrans.InverseTransformPoint(testFoam.transform.TransformPoint(foamDistinctVerts[i])));
        }
        //Get foam verts flush with stopping (~same z values), in foam local space
        var flushFoamVerts = (foamDistinctVerts.OrderByDescending(Vector3 => Vector3.z).ToList()).GetRange(0, 82);
        //Foam verts flush with stopping, in tile local space
        var flushTileVerts = new List<Vector3>();
        //Convert flush verts from foam local space to tile local space
        for (var i = 0; i < flushFoamVerts.Count; i++)
        {
            flushTileVerts.Add(stopTrans.InverseTransformPoint(testFoam.transform.TransformPoint(flushFoamVerts[i])));
        }
        //Min/Max for foam, in foam local space
        var foamMinX = flushFoamVerts.Min(Vector3 => Vector3.x);
        var foamMaxX = flushFoamVerts.Max(Vector3 => Vector3.x);
        var foamMaxY = flushFoamVerts.Max(Vector3 => Vector3.y);
        //Left column, right column, top row verts in foam local space. Left/Right point should have roughly equal x values in local space,Top points should have equal y. 
        var flushFoamLeft = new List<Vector3>();
        var flushFoamRight = new List<Vector3>();
        var flushFoamTop = new List<Vector3>();
        //L/R/T (as previous), in Tile local space
        var flushTileLeft = new List<Vector3>();
        var flushTileRight = new List<Vector3>();
        var flushTileTop = new List<Vector3>();
        //(x,y,z) translation of verts, in tile local space 
        var translateLeft = new List<Vector3>();
        var translateRight = new List<Vector3>();
        var translateTop = new List<Vector3>();
        var foamLeftY = new List<float>();
        var foamRightY = new List<float>();
        var foamTopX = new List<float>();
        for (var i = 0; i < flushFoamVerts.Count; i++)
        {
            if (flushFoamVerts[i].y >= foamMaxY - 0.01f)
            {
                flushFoamTop.Add(flushFoamVerts[i]);
                foamTopX.Add(flushFoamVerts[i].x);
            }
            else if (flushFoamVerts[i].x <= foamMinX + 0.01f)
            {
                flushFoamLeft.Add(flushFoamVerts[i]);
                foamLeftY.Add(flushFoamVerts[i].y);
            }
            else if (flushFoamVerts[i].x >= foamMaxX - 0.01f)
            {
                flushFoamRight.Add(flushFoamVerts[i]);
                foamRightY.Add(flushFoamVerts[i].y);
            }
        }
        for (var i = 0; i < flushFoamLeft.Count; i++)
        {
            flushTileLeft.Add(foamTrans.InverseTransformPoint(testFoam.transform.TransformPoint(flushFoamLeft[i])));
        }
        for (var i = 0; i < flushFoamRight.Count; i++)
        {
            flushTileRight.Add(foamTrans.InverseTransformPoint(testFoam.transform.TransformPoint(flushFoamRight[i])));
        }
        for (var i = 0; i < flushFoamTop.Count; i++)
        {
            flushTileTop.Add(foamTrans.InverseTransformPoint(testFoam.transform.TransformPoint(flushFoamTop[i])));
        }
        flushFoamLeft = flushFoamLeft.OrderBy(Vector3 => Vector3.y).ToList();
        flushFoamRight = flushFoamRight.OrderBy(Vector3 => Vector3.y).ToList();
        flushFoamTop = flushFoamTop.OrderBy(Vector3 => Vector3.x).ToList();
        flushTileLeft = flushTileLeft.OrderBy(Vector3 => Vector3.y).ToList();
        flushTileRight = flushTileRight.OrderBy(Vector3 => Vector3.y).ToList();
        flushTileTop = flushTileTop.OrderBy(Vector3 => Vector3.x).ToList();
        foamLeftY.Sort();
        foamRightY.Sort();
        foamTopX.Sort();
        try
        {
            for (var i = 0; i < flushFoamLeft.Count; i++)
            {
                translateLeft.Add(new Vector3((testFoam.transform.InverseTransformPoint(stopTrans.TransformPoint(transLeft[i]))).x - flushFoamLeft[i].x, (testFoam.transform.InverseTransformPoint(stopTrans.TransformPoint(transLeft[i]))).y - flushFoamLeft[i].y, (testFoam.transform.InverseTransformPoint(stopTrans.TransformPoint(transLeft[i]))).z - flushFoamLeft[i].z));
            }
            for (var i = 0; i < flushFoamRight.Count; i++)
            {
                translateRight.Add(new Vector3((testFoam.transform.InverseTransformPoint(stopTrans.TransformPoint(transRight[i]))).x - flushFoamRight[i].x, (testFoam.transform.InverseTransformPoint(stopTrans.TransformPoint(transRight[i]))).y - flushFoamRight[i].y, (testFoam.transform.InverseTransformPoint(stopTrans.TransformPoint(transRight[i]))).z - flushFoamRight[i].z));
            }
            for (var i = 0; i < flushFoamTop.Count; i++)
            {
                translateTop.Add(new Vector3((testFoam.transform.InverseTransformPoint(stopTrans.TransformPoint(transUp[i]))).x - flushFoamTop[i].x, (testFoam.transform.InverseTransformPoint(stopTrans.TransformPoint(transUp[i]))).y - flushFoamTop[i].y, (testFoam.transform.InverseTransformPoint(stopTrans.TransformPoint(transUp[i]))).z - flushFoamTop[i].z));
            }
        }
        catch
        {
            Debug.Log("incompatible foam/stopping vertices");
        }
        var maxLeftY = foamLeftY.Max();
        //var maxRightY = foamRightY.Max();
        for (var i = 0; i < foamVerts.Length; i++)
        {
            //L/R check
            if (foamVerts[i].y <= maxLeftY + 0.01)
            {
                //Left side check
                if (foamVerts[i].x < 0)
                {
                    if (foamVerts[i].y < foamLeftY[1] - 0.01)
                    {
                        newFoamVerts[i] = newFoamVerts[i] + translateLeft[0];
                    }
                    else if (foamVerts[i].y < foamLeftY[foamLeftY.Count - 1] + 0.01 && foamVerts[i].y > foamLeftY[foamLeftY.Count - 2] + 0.01)
                    {
                        newFoamVerts[i] = newFoamVerts[i] + translateLeft[foamLeftY.Count - 1];
                    }
                    else
                    {
                        for (var j = 0; j < foamLeftY.Count - 1; j++)
                        {
                            if (foamVerts[i].y < foamLeftY[j + 1] - 0.01)
                            {
                                newFoamVerts[i] = newFoamVerts[i] + translateLeft[j];
                                break;
                            }
                        }
                    }
                }
                //right side
                else
                {
                    if (foamVerts[i].y < foamRightY[1] - 0.01)
                    {
                        newFoamVerts[i] = newFoamVerts[i] + translateRight[0];
                    }
                    else if (foamVerts[i].y < foamRightY[foamRightY.Count - 1] + 0.01 && foamVerts[i].y > foamRightY[foamRightY.Count - 2] + 0.01)
                    {
                        newFoamVerts[i] = newFoamVerts[i] + translateRight[foamRightY.Count - 1];
                    }
                    else
                    {
                        for (var j = 1; j < foamRightY.Count - 1; j++)
                        {
                            if (foamVerts[i].y < foamRightY[j + 1] - 0.01)
                            {
                                newFoamVerts[i] = newFoamVerts[i] + translateRight[j];
                                break;
                            }
                        }
                    }
                }
            }
            //top row
            else
            {
                if (foamVerts[i].x < foamTopX[1] - 0.01)
                {
                    newFoamVerts[i] = newFoamVerts[i] + translateTop[0];
                }
                else
                {
                    for (var j = 1; j < foamTopX.Count - 2; j++)
                    {
                        if (foamVerts[i].x < foamTopX[j + 1] - 0.01)
                        {
                            newFoamVerts[i] = newFoamVerts[i] + translateTop[j];
                            break;
                        }
                    }
                    if (foamVerts[i].x < foamTopX[foamTopX.Count - 2] + 0.01 && foamVerts[i].x >= foamTopX[foamTopX.Count - 2] - 0.01)
                    {
                        newFoamVerts[i] = newFoamVerts[i] + translateTop[foamTopX.Count - 2];
                    }
                }
                if (foamVerts[i].x > foamTopX[foamTopX.Count - 2] + 0.01)
                {
                    newFoamVerts[i] = newFoamVerts[i] + translateTop[foamTopX.Count - 1];
                }
            }
        }
        foamMesh.vertices = newFoamVerts;
        testFoam.GetComponent<MeshFilter>().mesh = foamMesh;
    }
}