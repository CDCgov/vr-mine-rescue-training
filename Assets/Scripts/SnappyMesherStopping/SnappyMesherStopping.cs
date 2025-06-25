using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnappyMesherStopping : MonoBehaviour
{
    // Public classes for stopping, stopping with forward facing man door, and stopping with backward facing man door.
    public GameObject stopping;
    public GameObject stoppingManDoor;
    public GameObject stoppingManDoorReverse;
    // Method returns foam options class with perimeter vertices added.


    private Mesh CopyTileMesh(GameObject tileObject)
    {
        var lodGroup = tileObject.GetComponent<LODGroup>();
        Mesh tileMesh = null;

        if (lodGroup != null)
        {
            var lod0Obj = lodGroup.GetLODs()[0].renderers[0].gameObject;
            tileMesh = Instantiate<Mesh>(lod0Obj.GetComponent<MeshFilter>().sharedMesh);
        }
        else
        {
            //tileMesh = Instantiate(curtHit.collider.gameObject.GetComponent<MeshFilter>().sharedMesh);
            tileMesh = Instantiate(tileObject.GetComponent<MeshFilter>().sharedMesh);
        }

        return tileMesh;
    }

    public FoamOptions SnapStop(bool frontFoamOpt, bool backFoamOpt, int manDoorOpt, RaycastHit stopHit, Transform stopPos)
    {
        // Create gameobjects for the straight tile found in the raycast and for the stopping. The stopping instantiated depends upon the man door option selected.
        //GameObject tile = Instantiate(stopHit.collider.gameObject, stopHit.collider.gameObject.transform.position, stopHit.collider.gameObject.transform.rotation);
        var tile = stopHit.collider.gameObject;

        GameObject stop = new GameObject();
        // Will be assigned to index of child containing stopping mesh in the prefab.
        int stopIndex = 0;
        //int hingeIndex = 0;
        // Number of vertices on perimeter of one side of stopping. 
        int tarVerts = 64;
        // Create stopping mesh and temporary mesh used for editing vertices.
        Mesh stopMesh = new Mesh();
        Mesh tempMesh = new Mesh();
        // Vector array for stopping vertices (all)
        Vector3[] stopVerts;
        // Vector list for new vertices as created.
        List<Vector3> newStoppingVerts = new List<Vector3>();
        // Check mandoor user selection.
        // If no man door needed, instantiate copy of stopping prefab and copies of mesh.
        if (manDoorOpt == 0)
        {
            stop = Instantiate(stopping, stopPos.position, stopPos.rotation);
            stopMesh = Mesh.Instantiate(stop.GetComponent<MeshFilter>().sharedMesh);
            tempMesh = Mesh.Instantiate(stop.GetComponent<MeshFilter>().sharedMesh);
            newStoppingVerts = tempMesh.vertices.ToList();
        }
        // Option for creating stopping with forward facing mandoor. Instantiates prefab and finds child object containing stopping mesh.
        else if (manDoorOpt == 1)
        {
            stop = Instantiate(stoppingManDoor, stopPos.position, stopPos.rotation);
            for (int i = 0; i < stop.transform.childCount; i++)
            {
                if (stop.transform.GetChild(i).transform.name == "stopping")
                {
                    stopIndex = i;
                    stopMesh = Mesh.Instantiate(stop.transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh);
                    tempMesh = Mesh.Instantiate(stop.transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh);
                    newStoppingVerts = tempMesh.vertices.ToList();
                    break;
                }
                else if (stop.transform.GetChild(i).transform.name == "ManDoorHinge")
                {
                    //hingeIndex = i;
                }
            }
        }
        // Option for backwards facing man door. Instantiates backwards facing man door and finds child containing stopping mesh.
        else
        {
            stop = Instantiate(stoppingManDoorReverse, stopPos.position, stopPos.rotation);
            for (int i = 0; i < stop.transform.childCount; i++)
            {
                if (stop.transform.GetChild(i).transform.name == "stopping")
                {
                    stopIndex = i;
                    stopMesh = Mesh.Instantiate(stop.transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh);
                    tempMesh = Mesh.Instantiate(stop.transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh);
                    newStoppingVerts = tempMesh.vertices.ToList();
                    break;
                }
            }
        }
        // Assign stopping vertices array.
        stopVerts = stopMesh.vertices;
        // Create list of unique vertices. This removes doubles at edges and triples at corners.
        var distStopVerts = (stopVerts.Distinct()).ToList();
        // Stopping min/max along z axis.
        var allStopMinZ = distStopVerts.Min(Vector3 => Vector3.z);
        var allStopMaxZ = distStopVerts.Max(Vector3 => Vector3.z);
        // Lists for vertices along stopping front, back, and center.
        var stopFront = new List<Vector3>();
        var stopCenter = new List<Vector3>();
        var stopBack = new List<Vector3>();

        for (var i = 0; i < distStopVerts.Count; i++)
        {
            if (distStopVerts[i].z < allStopMinZ + 0.01)
            {
                stopFront.Add(distStopVerts[i]);
            }
            if (distStopVerts[i].z > allStopMaxZ - 0.01)
            {
                stopBack.Add(distStopVerts[i]);
            }
            if (distStopVerts[i].z >= allStopMinZ + 0.01 && distStopVerts[i].z <= allStopMaxZ - 0.01)
            {
                stopCenter.Add(distStopVerts[i]);
            }
        }
        var sortedStopFront = new SortedVerts();
        var sortedStopCenter = new SortedVerts();
        var sortedStopBack = new SortedVerts();
        // Minima/maxima for front/back/center vertices.
        var frontMM = new VertExtr(stopFront);
        var backMM = new VertExtr(stopBack);
        var centerMM = new VertExtr(stopCenter);
        for (var i = 0; i < stopFront.Count; i++)
        {
            if (stopFront[i].x < frontMM.minX + 0.01)
            {
                sortedStopFront.le.Add(stopFront[i]);
            }
            if (stopFront[i].x > frontMM.maxX - 0.01)
            {
                sortedStopFront.ri.Add(stopFront[i]);
            }
            if (stopFront[i].y < frontMM.minY + 0.01)
            {
                sortedStopFront.bo.Add(stopFront[i]);
            }
            if (stopFront[i].y > frontMM.maxY - 0.01)
            {
                sortedStopFront.to.Add(stopFront[i]);
            }
        }
        sortedStopFront.le = (sortedStopFront.le.OrderBy(Vector3 => Vector3.y)).ToList();
        sortedStopFront.ri = (sortedStopFront.ri.OrderBy(Vector3 => Vector3.y)).ToList();
        sortedStopFront.to = (sortedStopFront.to.OrderBy(Vector3 => Vector3.x)).ToList();
        sortedStopFront.bo = (sortedStopFront.bo.OrderBy(Vector3 => Vector3.x)).ToList();
        for (var i = 0; i < stopBack.Count; i++)
        {
            if (stopBack[i].x < backMM.minX + 0.01)
            {
                sortedStopBack.le.Add(stopBack[i]);
            }
            if (stopBack[i].x > backMM.maxX - 0.01)
            {
                sortedStopBack.ri.Add(stopBack[i]);
            }
            if (stopBack[i].y < backMM.minY + 0.01)
            {
                sortedStopBack.bo.Add(stopBack[i]);
            }
            if (stopBack[i].y > backMM.maxY - 0.01)
            {
                sortedStopBack.to.Add(stopBack[i]);
            }
        }
        sortedStopBack.le = (sortedStopBack.le.OrderBy(Vector3 => Vector3.y)).ToList();
        sortedStopBack.ri = (sortedStopBack.ri.OrderBy(Vector3 => Vector3.y)).ToList();
        sortedStopBack.to = (sortedStopBack.to.OrderBy(Vector3 => Vector3.x)).ToList();
        sortedStopBack.bo = (sortedStopBack.bo.OrderBy(Vector3 => Vector3.x)).ToList();
        for (var i = 0; i < stopCenter.Count; i++)
        {
            if (stopCenter[i].x < centerMM.minX + 0.01)
            {
                sortedStopCenter.le.Add(stopCenter[i]);
            }
            if (stopCenter[i].x > centerMM.maxX - 0.01)
            {
                sortedStopCenter.ri.Add(stopCenter[i]);
            }
            if (stopCenter[i].y < centerMM.minY + 0.01)
            {
                sortedStopCenter.bo.Add(stopCenter[i]);
            }
            if (stopCenter[i].y > centerMM.maxY - 0.01)
            {
                sortedStopCenter.to.Add(stopCenter[i]);
            }
        }
        sortedStopCenter.le = (sortedStopCenter.le.OrderBy(Vector3 => Vector3.y)).ToList();
        sortedStopCenter.ri = (sortedStopCenter.ri.OrderBy(Vector3 => Vector3.y)).ToList();
        sortedStopCenter.to = (sortedStopCenter.to.OrderBy(Vector3 => Vector3.x)).ToList();
        sortedStopCenter.bo = (sortedStopCenter.bo.OrderBy(Vector3 => Vector3.x)).ToList();
        //var tempTileMesh = Mesh.Instantiate(tile.GetComponent<MeshFilter>().sharedMesh);
        var tempTileMesh = CopyTileMesh(tile);
        var tileVerts = tempTileMesh.vertices;
        // Any tile verts that are inside reference space surrounding stopping.
        List<Vector3> matchedTileVerts = new List<Vector3>();
        var worldTileVerts = new Vector3[tileVerts.Length];
        for (var i = 0; i < worldTileVerts.Length; i++)
        {
            worldTileVerts[i] = tile.transform.TransformPoint(tileVerts[i]);
        }
        var stopTileVerts = new Vector3[worldTileVerts.Length];
        for (var i = 0; i < worldTileVerts.Length; i++)
        {
            stopTileVerts[i] = stop.transform.InverseTransformPoint(worldTileVerts[i]);
        }
        // Find tile verts that match stopping space  by comparing reference  min/max z to stopping vertices z values.
        var centerZ = (frontMM.maxZ + backMM.minZ) / 2;
        for (int i = 0; i < stopTileVerts.Length; i++)
        {
            if (stopTileVerts[i].z >= centerZ - 1f && stopTileVerts[i].z <= centerZ + 1f)
            {
                matchedTileVerts.Add(stopTileVerts[i]);
            }
        }
        // Using distinct() cuts verts to unique values (removes verts added for normals).
        IEnumerable<Vector3> distinctVerts = matchedTileVerts.Distinct();
        List<Vector3> distinctMatchedVerts = new List<Vector3>();
        List<Vector3> frontMatchedVerts = new List<Vector3>();
        List<Vector3> backMatchedVerts = new List<Vector3>();
        List<Vector3> centerMatchedVerts = new List<Vector3>();
        foreach (var item in distinctVerts)
        {
            distinctMatchedVerts.Add(item);
        }
        // Fixed z values for stopping.
        var stoppingZOne = -0.071f;
        var stoppingZTwo = 0.071f;
        for (int i = 0; i < distinctMatchedVerts.Count; i++)
        {
            if (distinctMatchedVerts[i].z > stoppingZOne && distinctMatchedVerts[i].z < stoppingZTwo)
            {
                centerMatchedVerts.Add(distinctMatchedVerts[i]);
            }
            else if (distinctMatchedVerts[i].z <= stoppingZOne)
            {
                frontMatchedVerts.Add(distinctMatchedVerts[i]);
            }
            else if (distinctMatchedVerts[i].z >= stoppingZTwo)
            {
                backMatchedVerts.Add(distinctMatchedVerts[i]);
            }
        }
        var backTileVerts = new SortedVerts();
        var centerTileVerts = new SortedVerts();
        var frontTileVerts = new SortedVerts();
        var initBackTileVerts = new List<Vector3>();
        var initFrontTileVerts = new List<Vector3>();
        initBackTileVerts = (backMatchedVerts.OrderBy(Vector3 => Vector3.z).ToList()).GetRange(0, tarVerts);
        initFrontTileVerts = (frontMatchedVerts.OrderByDescending(Vector3 => Vector3.z).ToList()).GetRange(0, tarVerts);
        frontTileVerts.ri = ((initFrontTileVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(tarVerts - 9, 9)).OrderBy(Vector3 => Vector3.y).ToList();
        frontTileVerts.le = ((initFrontTileVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(0, 9)).OrderBy(Vector3 => Vector3.y).ToList();
        frontTileVerts.to = ((initFrontTileVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(tarVerts - 25, 25)).OrderBy(Vector3 => Vector3.x).ToList();
        frontTileVerts.bo = ((initFrontTileVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(0, 25)).OrderBy(Vector3 => Vector3.x).ToList();
        backTileVerts.ri = ((initBackTileVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(tarVerts - 9, 9)).OrderBy(Vector3 => Vector3.y).ToList();
        backTileVerts.le = ((initBackTileVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(0, 9)).OrderBy(Vector3 => Vector3.y).ToList();
        backTileVerts.to = ((initBackTileVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(tarVerts - 25, 25)).OrderBy(Vector3 => Vector3.x).ToList();
        backTileVerts.bo = ((initBackTileVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(0, 25)).OrderBy(Vector3 => Vector3.x).ToList();
        try
        {
            centerTileVerts.ri = ((centerMatchedVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(tarVerts - 9, 9)).OrderBy(Vector3 => Vector3.y).ToList();
            centerTileVerts.le = ((centerMatchedVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(0, 9)).OrderBy(Vector3 => Vector3.y).ToList();
            centerTileVerts.to = ((centerMatchedVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(tarVerts - 25, 25)).OrderBy(Vector3 => Vector3.x).ToList();
            centerTileVerts.bo = ((centerMatchedVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(0, 25)).OrderBy(Vector3 => Vector3.x).ToList();
        }
        catch
        {
            centerTileVerts.ri.Clear();
            centerTileVerts.le.Clear();
            centerTileVerts.to.Clear();
            centerTileVerts.bo.Clear();
            for (var i = 0; i < backTileVerts.le.Count; i++)
            {
                centerTileVerts.le.Add(new Vector3(frontTileVerts.le[i].x + ((backTileVerts.le[i].x - frontTileVerts.le[i].x) * (-frontTileVerts.le[i].z)) / (backTileVerts.le[i].z - frontTileVerts.le[i].z), frontTileVerts.le[i].y, 0));
            }
            for (var i = 0; i < backTileVerts.ri.Count; i++)
            {
                centerTileVerts.ri.Add(new Vector3(frontTileVerts.ri[i].x + ((backTileVerts.ri[i].x - frontTileVerts.ri[i].x) * (-frontTileVerts.ri[i].z)) / (backTileVerts.ri[i].z - frontTileVerts.ri[i].z), frontTileVerts.ri[i].y, 0));
            }
            for (var i = 0; i < backTileVerts.to.Count; i++)
            {
                centerTileVerts.to.Add(new Vector3(frontTileVerts.to[i].x, frontTileVerts.to[i].y + ((backTileVerts.to[i].y - frontTileVerts.to[i].y) * (-frontTileVerts.to[i].z)) / (backTileVerts.to[i].z - frontTileVerts.to[i].z), 0));
            }
            for (var i = 0; i < backTileVerts.bo.Count; i++)
            {
                centerTileVerts.bo.Add(new Vector3(frontTileVerts.bo[i].x, frontTileVerts.bo[i].y + ((backTileVerts.bo[i].y - frontTileVerts.bo[i].y) * (-frontTileVerts.bo[i].z)) / (backTileVerts.bo[i].z - frontTileVerts.bo[i].z), 0));
            }
            centerTileVerts.le = centerTileVerts.le.OrderBy(Vector3 => Vector3.y).ToList();
            centerTileVerts.ri = centerTileVerts.ri.OrderBy(Vector3 => Vector3.y).ToList();
            centerTileVerts.to = centerTileVerts.to.OrderBy(Vector3 => Vector3.x).ToList();
            centerTileVerts.bo = centerTileVerts.bo.OrderBy(Vector3 => Vector3.x).ToList();
        }
        var tileLeftTrans = new List<Vector3>();
        var tileRightTrans = new List<Vector3>();
        var tileTopTrans = new List<Vector3>();
        var backLeftTrans = new List<Vector3>();
        var backRightTrans = new List<Vector3>();
        var backTopTrans = new List<Vector3>();
        int tempIndex;
        int tempIndexTwo;
        // Replace center (z) verts.
        for (var i = 0; i < stopVerts.Length; i++)
        {
            try
            {
                if (stopVerts[i].z < 0.01 && stopVerts[i].z > -0.01)
                {
                    // Bottom edge.
                    if (stopVerts[i].y < centerMM.minY + 0.01 && stopVerts[i].x > centerMM.minX + 0.01 && stopVerts[i].x < centerMM.maxX - 0.01)
                    {
                        tempIndex = sortedStopCenter.bo.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = centerTileVerts.bo[tempIndex];
                    }
                    // Top edge.
                    if (stopVerts[i].y > centerMM.maxY - 0.01 && stopVerts[i].x > centerMM.minX + 0.01 && stopVerts[i].x < centerMM.maxX - 0.01)
                    {
                        tempIndex = sortedStopCenter.to.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = centerTileVerts.to[tempIndex];
                    }
                    // Left edge and corners.
                    if (stopVerts[i].x < centerMM.minX + 0.01 && stopVerts[i].y < centerMM.minY + 0.01)
                    {
                        tempIndex = sortedStopCenter.le.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopCenter.bo.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.le[tempIndex].x, centerTileVerts.bo[tempIndexTwo].y, centerTileVerts.le[tempIndex].z);
                    }
                    else if (stopVerts[i].x < centerMM.minX + 0.01 && stopVerts[i].y > centerMM.maxY - 0.01)
                    {
                        tempIndex = sortedStopCenter.le.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopCenter.to.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.le[tempIndex].x, centerTileVerts.to[tempIndexTwo].y, centerTileVerts.le[tempIndex].z);
                    }
                    else if (stopVerts[i].x < centerMM.minX + 0.01)
                    {
                        tempIndex = sortedStopCenter.le.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = centerTileVerts.le[tempIndex];
                    }
                    //right edge and corners
                    if (stopVerts[i].x > centerMM.maxX - 0.01 && stopVerts[i].y < centerMM.minY + 0.01)
                    {
                        tempIndex = sortedStopCenter.ri.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopCenter.bo.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.ri[tempIndex].x, centerTileVerts.bo[tempIndexTwo].y, centerTileVerts.ri[tempIndex].z);
                    }
                    else if (stopVerts[i].x > centerMM.maxX - 0.01 && stopVerts[i].y > centerMM.maxY - 0.01)
                    {

                        tempIndex = sortedStopCenter.ri.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopCenter.to.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.ri[tempIndex].x, centerTileVerts.to[tempIndexTwo].y, centerTileVerts.ri[tempIndex].z);
                    }
                    else if (stopVerts[i].x > centerMM.maxX - 0.01)
                    {
                        tempIndex = sortedStopCenter.ri.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = centerTileVerts.ri[tempIndex];
                    }
                }
                // Replace front and back (z) verts. 
                // Replace front Verts.
                if (stopVerts[i].z < frontMM.minZ + 0.01)
                {
                    // Bottom edge.
                    if (stopVerts[i].y < frontMM.minY + 0.01 && stopVerts[i].x > frontMM.minX + 0.01 && stopVerts[i].x < frontMM.maxX - 0.01)
                    {
                        tempIndex = sortedStopFront.bo.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(frontTileVerts.bo[tempIndex].x, centerTileVerts.bo[tempIndex].y - (centerTileVerts.bo[tempIndex].y - frontTileVerts.bo[tempIndex].y) * (centerTileVerts.bo[tempIndex].z + 0.071f) / (centerTileVerts.bo[tempIndex].z - frontTileVerts.bo[tempIndex].z), -0.071f);
                    }
                    // Top edge.
                    if (stopVerts[i].y > frontMM.maxY - 0.01 && stopVerts[i].x > frontMM.minX + 0.01 && stopVerts[i].x < frontMM.maxX - 0.01)
                    {
                        tempIndex = sortedStopFront.to.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(frontTileVerts.to[tempIndex].x, centerTileVerts.to[tempIndex].y - (centerTileVerts.to[tempIndex].y - frontTileVerts.to[tempIndex].y) * (centerTileVerts.to[tempIndex].z + 0.071f) / (centerTileVerts.to[tempIndex].z - frontTileVerts.to[tempIndex].z), -0.071f);
                        tileTopTrans.Add(newStoppingVerts[i]);
                    }
                    // Left edge and corners.
                    // Front left bottom corner.
                    if (stopVerts[i].x < frontMM.minX + 0.01 && stopVerts[i].y < frontMM.minY + 0.01)
                    {
                        tempIndex = sortedStopFront.le.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopFront.bo.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.le[tempIndex].x + (frontTileVerts.le[tempIndex].x - centerTileVerts.le[tempIndex].x) * (-0.071f - centerTileVerts.le[tempIndex].z) / (frontTileVerts.le[tempIndex].z - centerTileVerts.le[tempIndex].z), centerTileVerts.bo[tempIndexTwo].y - (centerTileVerts.bo[tempIndexTwo].y - frontTileVerts.bo[tempIndexTwo].y) * (centerTileVerts.bo[tempIndexTwo].z + 0.071f) / (centerTileVerts.bo[tempIndexTwo].z - frontTileVerts.bo[tempIndexTwo].z), -0.071f);
                        tileLeftTrans.Add(newStoppingVerts[i]);
                    }
                    // Front left top corner.
                    else if (stopVerts[i].x < frontMM.minX + 0.01 && stopVerts[i].y > frontMM.maxY - 0.01)
                    {
                        tempIndex = sortedStopFront.le.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopFront.to.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.le[tempIndex].x + (frontTileVerts.le[tempIndex].x - centerTileVerts.le[tempIndex].x) * (-0.071f - centerTileVerts.le[tempIndex].z) / (frontTileVerts.le[tempIndex].z - centerTileVerts.le[tempIndex].z), centerTileVerts.to[tempIndexTwo].y - (centerTileVerts.to[tempIndexTwo].y - frontTileVerts.to[tempIndexTwo].y) * (centerTileVerts.to[tempIndexTwo].z + 0.071f) / (centerTileVerts.to[tempIndexTwo].z - frontTileVerts.to[tempIndexTwo].z), -0.071f);
                        tileTopTrans.Add(newStoppingVerts[i]);
                    }
                    // Remaining front left verts.
                    else if (stopVerts[i].x < frontMM.minX + 0.01)
                    {
                        tempIndex = sortedStopFront.le.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.le[tempIndex].x + (frontTileVerts.le[tempIndex].x - centerTileVerts.le[tempIndex].x) * (-0.071f - centerTileVerts.le[tempIndex].z) / (frontTileVerts.le[tempIndex].z - centerTileVerts.le[tempIndex].z), frontTileVerts.le[tempIndex].y, -0.071f);
                        tileLeftTrans.Add(newStoppingVerts[i]);
                    }
                    // Right edge and corners.
                    // Front right bottom corner.
                    if (stopVerts[i].x > frontMM.maxX - 0.01 && stopVerts[i].y < frontMM.minY + 0.01)
                    {
                        tempIndex = sortedStopFront.ri.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopFront.bo.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.ri[tempIndex].x - (centerTileVerts.ri[tempIndex].x - frontTileVerts.ri[tempIndex].x) * (centerTileVerts.ri[tempIndex].z + 0.071f) / (centerTileVerts.ri[tempIndex].z - frontTileVerts.ri[tempIndex].z), centerTileVerts.bo[tempIndexTwo].y - (centerTileVerts.bo[tempIndexTwo].y - frontTileVerts.bo[tempIndexTwo].y) * (centerTileVerts.bo[tempIndexTwo].z + 0.071f) / (centerTileVerts.bo[tempIndexTwo].z - frontTileVerts.bo[tempIndexTwo].z), -0.071f);
                        tileRightTrans.Add(newStoppingVerts[i]);
                    }
                    // Front right top corner.
                    else if (stopVerts[i].x > frontMM.maxX - 0.01 && stopVerts[i].y > frontMM.maxY - 0.01)
                    {
                        tempIndex = sortedStopFront.ri.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopFront.to.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.ri[tempIndex].x - (centerTileVerts.ri[tempIndex].x - frontTileVerts.ri[tempIndex].x) * (centerTileVerts.ri[tempIndex].z + 0.071f) / (centerTileVerts.ri[tempIndex].z - frontTileVerts.ri[tempIndex].z), centerTileVerts.to[tempIndexTwo].y - (centerTileVerts.to[tempIndexTwo].y - frontTileVerts.to[tempIndexTwo].y) * (centerTileVerts.to[tempIndexTwo].z + 0.071f) / (centerTileVerts.to[tempIndexTwo].z - frontTileVerts.to[tempIndexTwo].z), -0.071f);
                        tileTopTrans.Add(newStoppingVerts[i]);
                    }
                    // Remaining front right verts.
                    else if (stopVerts[i].x > frontMM.maxX - 0.01)
                    {
                        tempIndex = sortedStopFront.ri.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.ri[tempIndex].x - (centerTileVerts.ri[tempIndex].x - frontTileVerts.ri[tempIndex].x) * (centerTileVerts.ri[tempIndex].z + 0.071f) / (centerTileVerts.ri[tempIndex].z - frontTileVerts.ri[tempIndex].z), frontTileVerts.ri[tempIndex].y, -0.071f);
                        tileRightTrans.Add(newStoppingVerts[i]);
                    }
                }
                // Replace Back Verts.
                if (stopVerts[i].z > backMM.maxZ - 0.01)
                {
                    // Bottom edge.
                    if (stopVerts[i].y < backMM.minY + 0.01 && stopVerts[i].x > backMM.minX + 0.01 && stopVerts[i].x < backMM.maxX - 0.01)
                    {
                        tempIndex = sortedStopBack.bo.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(backTileVerts.bo[tempIndex].x, centerTileVerts.bo[tempIndex].y + ((backTileVerts.bo[tempIndex].y - centerTileVerts.bo[tempIndex].y) * (0.071f - centerTileVerts.bo[tempIndex].z)) / (backTileVerts.bo[tempIndex].z - centerTileVerts.bo[tempIndex].z), 0.071f);
                    }
                    // Top edge.
                    if (stopVerts[i].y > backMM.maxY - 0.01 && stopVerts[i].x > backMM.minX + 0.01 && stopVerts[i].x < backMM.maxX - 0.01)
                    {
                        tempIndex = sortedStopBack.to.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(backTileVerts.to[tempIndex].x, centerTileVerts.to[tempIndex].y + ((backTileVerts.to[tempIndex].y - centerTileVerts.to[tempIndex].y) * (0.071f - centerTileVerts.to[tempIndex].z)) / (backTileVerts.to[tempIndex].z - centerTileVerts.to[tempIndex].z), 0.071f);
                        backTopTrans.Add(newStoppingVerts[i]);
                    }
                    // Left edge and corners.
                    // Back lower left corner.
                    if (stopVerts[i].x < backMM.minX + 0.01 && stopVerts[i].y < backMM.minY + 0.01)
                    {
                        tempIndex = sortedStopBack.le.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopBack.bo.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.le[tempIndex].x + (backTileVerts.le[tempIndex].x - centerTileVerts.le[tempIndex].x) * (0.071f - centerTileVerts.le[tempIndex].z) / (backTileVerts.le[tempIndex].z - centerTileVerts.le[tempIndex].z), centerTileVerts.bo[tempIndexTwo].y + ((backTileVerts.bo[tempIndexTwo].y - centerTileVerts.bo[tempIndexTwo].y) * (0.071f - centerTileVerts.bo[tempIndexTwo].z)) / (backTileVerts.bo[tempIndexTwo].z - centerTileVerts.bo[tempIndexTwo].z), 0.071f);
                        backLeftTrans.Add(newStoppingVerts[i]);
                    }
                    // Back upper left corner.
                    else if (stopVerts[i].x < backMM.minX + 0.01 && stopVerts[i].y > backMM.maxY - 0.01)
                    {
                        tempIndex = sortedStopBack.le.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopBack.to.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.le[tempIndex].x + (backTileVerts.le[tempIndex].x - centerTileVerts.le[tempIndex].x) * (0.071f - centerTileVerts.le[tempIndex].z) / (backTileVerts.le[tempIndex].z - centerTileVerts.le[tempIndex].z), centerTileVerts.to[tempIndexTwo].y + ((backTileVerts.to[tempIndexTwo].y - centerTileVerts.to[tempIndexTwo].y) * (0.071f - centerTileVerts.to[tempIndexTwo].z)) / (backTileVerts.to[tempIndexTwo].z - centerTileVerts.to[tempIndexTwo].z), 0.071f);
                        backTopTrans.Add(newStoppingVerts[i]);
                    }
                    // Remaining left edge.
                    else if (stopVerts[i].x < backMM.minX + 0.01)
                    {
                        tempIndex = sortedStopBack.le.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.le[tempIndex].x + ((backTileVerts.le[tempIndex].x - centerTileVerts.le[tempIndex].x) * (0.071f - centerTileVerts.le[tempIndex].z)) / (backTileVerts.le[tempIndex].z - centerTileVerts.le[tempIndex].z), backTileVerts.le[tempIndex].y, 0.071f);
                        backLeftTrans.Add(newStoppingVerts[i]);
                    }

                    // Right edge and corners.
                    // Back lower right corner.
                    if (stopVerts[i].x > backMM.maxX - 0.01 && stopVerts[i].y < backMM.minY + 0.01)
                    {
                        tempIndex = sortedStopBack.ri.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopBack.bo.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.ri[tempIndex].x - (centerTileVerts.ri[tempIndex].x - backTileVerts.ri[tempIndex].x) * (centerTileVerts.ri[tempIndex].z - 0.071f) / (centerTileVerts.ri[tempIndex].z - backTileVerts.ri[tempIndex].z), centerTileVerts.bo[tempIndexTwo].y + ((backTileVerts.bo[tempIndexTwo].y - centerTileVerts.bo[tempIndexTwo].y) * (0.071f - centerTileVerts.bo[tempIndexTwo].z)) / (backTileVerts.bo[tempIndexTwo].z - centerTileVerts.bo[tempIndexTwo].z), 0.071f);
                        backRightTrans.Add(newStoppingVerts[i]);
                    }
                    //Back upper right corner
                    else if (stopVerts[i].x > backMM.maxX - 0.01 && stopVerts[i].y > backMM.maxY - 0.01)
                    {
                        tempIndex = sortedStopBack.ri.FindIndex(v => v == stopVerts[i]);
                        tempIndexTwo = sortedStopBack.to.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.ri[tempIndex].x - (centerTileVerts.ri[tempIndex].x - backTileVerts.ri[tempIndex].x) * (centerTileVerts.ri[tempIndex].z - 0.071f) / (centerTileVerts.ri[tempIndex].z - backTileVerts.ri[tempIndex].z), centerTileVerts.to[tempIndexTwo].y + ((backTileVerts.to[tempIndexTwo].y - centerTileVerts.to[tempIndexTwo].y) * (0.071f - centerTileVerts.to[tempIndexTwo].z)) / (backTileVerts.to[tempIndexTwo].z - centerTileVerts.to[tempIndexTwo].z), 0.071f);
                        backTopTrans.Add(newStoppingVerts[i]);
                    }
                    // Remaining right edge
                    else if (stopVerts[i].x > backMM.maxX - 0.01)
                    {
                        tempIndex = sortedStopBack.ri.FindIndex(v => v == stopVerts[i]);
                        newStoppingVerts[i] = new Vector3(centerTileVerts.ri[tempIndex].x - (centerTileVerts.ri[tempIndex].x - backTileVerts.ri[tempIndex].x) * (centerTileVerts.ri[tempIndex].z - 0.071f) / (centerTileVerts.ri[tempIndex].z - backTileVerts.ri[tempIndex].z), backTileVerts.ri[tempIndex].y, 0.071f);
                        backRightTrans.Add(newStoppingVerts[i]);
                    }
                }
            }
            catch
            {
                Debug.Log("Unable to match vertex" + stopVerts[i]);
                Debug.Log("In world space:" + stop.transform.TransformPoint(stopVerts[i]));
            }
        }
        tileLeftTrans = (tileLeftTrans.Distinct()).ToList();
        tileRightTrans = (tileRightTrans.Distinct()).ToList();
        tileTopTrans = (tileTopTrans.Distinct()).ToList();
        tileLeftTrans = tileLeftTrans.OrderBy(Vector3 => Vector3.y).ToList();
        tileRightTrans = tileRightTrans.OrderBy(Vector3 => Vector3.y).ToList();
        tileTopTrans = tileTopTrans.OrderBy(Vector3 => Vector3.x).ToList();
        backLeftTrans = (backLeftTrans.Distinct()).ToList();
        backRightTrans = (backRightTrans.Distinct()).ToList();
        backTopTrans = (backTopTrans.Distinct()).ToList();
        backLeftTrans = backLeftTrans.OrderBy(Vector3 => Vector3.y).ToList();
        backRightTrans = backRightTrans.OrderBy(Vector3 => Vector3.y).ToList();
        backTopTrans = backTopTrans.OrderBy(Vector3 => Vector3.x).ToList();
        Vector3[] stoppingNormals = stopMesh.normals;
        var newExtrem = new VertExtr(newStoppingVerts);
        float xOffset = 0;
        float yOffset = 0;
        var textureDim = 1.5848f;
        if (manDoorOpt == 0)
        {
            var stoppingColl = stop.AddComponent<BoxCollider>();
            stoppingColl.center = new Vector3((newExtrem.maxX + newExtrem.minX) / 2, (newExtrem.maxY + newExtrem.minY) / 2, (newExtrem.maxZ + newExtrem.minZ) / 2);
            stoppingColl.size = new Vector3(newExtrem.maxX - newExtrem.minX, newExtrem.maxY - newExtrem.minY, newExtrem.maxZ - newExtrem.minZ);
        }
        else
        {
            var doorMesh = Mesh.Instantiate(stop.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh);
            var doorVerts = doorMesh.vertices;
            var doorExtrem = new VertExtr(doorVerts);
            var doorTrans = stop.transform.GetChild(0);
            var doorColl = doorTrans.gameObject.AddComponent<BoxCollider>();
            doorColl.center = new Vector3((doorExtrem.minX + doorExtrem.maxX) / 2, (doorExtrem.minY + doorExtrem.maxY) / 2, (doorExtrem.minZ + doorExtrem.maxZ) / 2);
            doorColl.size = new Vector3(doorExtrem.maxX - doorExtrem.minX, doorExtrem.maxY - doorExtrem.minY, doorExtrem.maxZ - doorExtrem.minZ);
            var localDoorMin = stop.transform.InverseTransformPoint(doorTrans.transform.TransformPoint(new Vector3(doorExtrem.minX, doorExtrem.minY, doorExtrem.minZ)));
            var localDoorMax = stop.transform.InverseTransformPoint(doorTrans.transform.TransformPoint(new Vector3(doorExtrem.maxX, doorExtrem.maxY, doorExtrem.maxZ)));
            var stoppingTrans = stop.transform.GetChild(3);
            var stoppingCollLeft = stoppingTrans.gameObject.AddComponent<BoxCollider>();
            stoppingCollLeft.center = new Vector3((localDoorMin.x + newExtrem.minX) / 2, (newExtrem.minY + newExtrem.maxY) / 2, (newExtrem.minZ + newExtrem.maxZ) / 2);
            stoppingCollLeft.size = new Vector3(localDoorMin.x - newExtrem.minX, newExtrem.maxY - newExtrem.minY, newExtrem.maxZ - newExtrem.minZ);
            var stoppingCollRight = stoppingTrans.gameObject.AddComponent<BoxCollider>();
            stoppingCollRight.center = new Vector3((newExtrem.maxX + localDoorMax.x) / 2, (newExtrem.minY + newExtrem.maxY) / 2, (newExtrem.minZ + newExtrem.maxZ) / 2);
            stoppingCollRight.size = new Vector3(newExtrem.maxX - localDoorMax.x, newExtrem.maxY - newExtrem.minY, newExtrem.maxZ - newExtrem.minZ);
            var stoppingCollTop = stoppingTrans.gameObject.AddComponent<BoxCollider>();
            stoppingCollTop.center = new Vector3((localDoorMin.x + localDoorMax.x) / 2, (localDoorMax.y + newExtrem.maxY) / 2, (newExtrem.minZ + newExtrem.maxZ) / 2);
            stoppingCollTop.size = new Vector3(localDoorMax.x - localDoorMin.x, newExtrem.maxY - localDoorMax.y, newExtrem.maxZ - newExtrem.minZ);
            var stoppingCollBottom = stoppingTrans.gameObject.AddComponent<BoxCollider>();
            stoppingCollBottom.center = new Vector3((localDoorMin.x + localDoorMax.x) / 2, (newExtrem.minY + localDoorMin.y) / 2, (newExtrem.minZ + newExtrem.maxZ) / 2);
            stoppingCollBottom.size = new Vector3(localDoorMax.x - localDoorMin.x, localDoorMin.y - newExtrem.minY, newExtrem.maxZ - newExtrem.minZ);
            //Texture offset
            xOffset = (((stop.transform.GetChild(3).transform.InverseTransformPoint(stop.transform.TransformPoint(localDoorMin))).x - newExtrem.minX) % textureDim) / textureDim;
            yOffset = ((newExtrem.maxY - (stop.transform.GetChild(3).transform.InverseTransformPoint(stop.transform.TransformPoint(localDoorMax))).y) % textureDim) / textureDim; 
        }


        var lengthX = newExtrem.maxX - newExtrem.minX;
        var lengthY = newExtrem.maxY - newExtrem.minY;
        var lengthZ = newExtrem.maxZ - newExtrem.minZ;
        
        Vector2[] newUVs = new Vector2[newStoppingVerts.Count];
        for (var i = 0; i < newStoppingVerts.Count; i++)
        {
            if (stoppingNormals[i] == Vector3.up || stoppingNormals[i] == Vector3.down)
            {
                newUVs[i] = new Vector2((newStoppingVerts[i].x - newExtrem.minX) / lengthX, (newStoppingVerts[i].z - newExtrem.minZ) / lengthY);
            }
            else if (stoppingNormals[i] == Vector3.left || stoppingNormals[i] == Vector3.right)
            {
                newUVs[i] = new Vector2((newStoppingVerts[i].z - newExtrem.minZ) / lengthX, (newStoppingVerts[i].y - newExtrem.minY) / lengthY);
            }
            else
            {
                newUVs[i] = new Vector2((newStoppingVerts[i].x - newExtrem.minX) / lengthX, (newStoppingVerts[i].y - newExtrem.minY) / lengthY);
            }
        }

        if (manDoorOpt == 0)
        {
            stopMesh.vertices = newStoppingVerts.ToArray();
            stopMesh.uv = newUVs;
            stopMesh.RecalculateNormals();
            stop.GetComponent<MeshFilter>().mesh = stopMesh;
            var stoppingRenderer = stop.GetComponent<MeshRenderer>();
            var stopMatTemp = stop.GetComponent<MeshRenderer>().sharedMaterial;
            stopMatTemp.mainTextureScale = new Vector2(lengthX / (textureDim), lengthY / (textureDim));
            stop.GetComponent<MeshRenderer>().material = stopMatTemp;
        }
        else
        {
            stopMesh.vertices = newStoppingVerts.ToArray();
            stopMesh.uv = newUVs;
            stop.transform.GetChild(stopIndex).GetComponent<MeshFilter>().mesh = stopMesh;
            var stopMatTemp = stop.transform.GetChild(stopIndex).GetComponent<MeshRenderer>().sharedMaterial;
            stopMatTemp.mainTextureScale = new Vector2(lengthX / (textureDim), lengthY / (textureDim));
            stopMatTemp.mainTextureOffset= new Vector2(xOffset-0.0101f,yOffset-0.0251f);
            stop.transform.GetChild(stopIndex).GetComponent<MeshRenderer>().material = stopMatTemp;
        }
       
        var foamOpt = new FoamOptions();
        foamOpt.stopTrans = stop.transform;
        foamOpt.frontFoamLeft = tileLeftTrans;
        foamOpt.frontFoamTop = tileTopTrans;
        foamOpt.frontFoamRight = tileRightTrans;
        foamOpt.backFoamLeft = backLeftTrans;
        foamOpt.backFoamTop = backTopTrans;
        foamOpt.backFoamRight = backRightTrans;
        //DestroyImmediate(tile);
        return foamOpt;
    }   
}