using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnappyMesherCurtain : MonoBehaviour
{
    public GameObject Clear_FlyPad;
    public GameObject Clear_Single_Curtain;
    public GameObject Yellow_Single_Curtain;
    public GameObject Curtain_Multi_Brattice;
    public GameObject Curtain_Single_Brattice;

    public void SnapCurt(Transform curtTrans, RaycastHit curtHit, int curtOpt, int curtOptSide, int curtOptTop)
    {
        var minTileX = new float();
        var maxTileX = new float();
        var minTileY = new float();
        var maxTileY = new float();

        SortedVerts tileVerts = FindTileVerts(curtHit, curtTrans);
        if (curtOptSide == 0)
        {
            minTileX = tileVerts.le.Min(Vector3 => Vector3.x);
            maxTileX = tileVerts.ri.Max(Vector3 => Vector3.x);
        }
        else
        {
            minTileX = tileVerts.le.Max(Vector3 => Vector3.x);
            maxTileX = tileVerts.ri.Min(Vector3 => Vector3.x);
        }
        if (curtOptTop == 0)
        {
            minTileY = tileVerts.bo.Min(Vector3 => Vector3.y);
            maxTileY = tileVerts.to.Max(Vector3 => Vector3.y);
        }
        else
        {
            minTileY = tileVerts.bo.Max(Vector3 => Vector3.y);
            maxTileY = tileVerts.to.Min(Vector3 => Vector3.y);
        }

        var buildCurt = new GameObject();
        if (curtOpt == 0 || curtOpt == 3 || curtOpt == 4)
        {
            //6.096 for single brattice
            // Original dimensions of curtain prefab.
            float origX = 6.095f;
            float origY = 2.134f;

            float scaleX = (maxTileX - minTileX) / origX;
            float scaleY = (maxTileY - minTileY) / origY;
            float newCurtX = (minTileX + maxTileX) / 2;
            var newCurtPos = curtTrans.TransformPoint(new Vector3(newCurtX, minTileY, 0));

            if (curtOpt == 0)
            {
                buildCurt = Instantiate(Clear_FlyPad, newCurtPos, curtTrans.rotation);
            }
            else if (curtOpt == 3)
            {
                buildCurt = Instantiate(Curtain_Multi_Brattice, newCurtPos, curtTrans.rotation);
            }
            else if (curtOpt == 4)
            {
                buildCurt = Instantiate(Curtain_Single_Brattice, newCurtPos, curtTrans.rotation);
            }
            for (int i = 0; i < buildCurt.transform.childCount; i++)
            {
                var tempPos = buildCurt.transform.GetChild(i).transform.localPosition;
                buildCurt.transform.GetChild(i).transform.localPosition = new Vector3(tempPos.x * scaleX, tempPos.y * scaleY, tempPos.z);
                if (buildCurt.transform.GetChild(i).transform.name.Contains("urtain") == true)
                {
                    var clothCo = buildCurt.transform.GetChild(i).GetComponent<Cloth>().coefficients;
                    var curtMesh = Instantiate(buildCurt.transform.GetChild(i).GetComponent<MeshFilter>().sharedMesh);
                    var curtVerts = curtMesh.vertices;
                    for (var j = 0; j < curtVerts.Length; j++)
                    {
                        curtVerts[j] = new Vector3(curtVerts[j].x * scaleX, curtVerts[j].y * scaleY, curtVerts[j].z);
                    }
                    curtMesh.vertices = curtVerts;
                    buildCurt.transform.GetChild(i).GetComponent<MeshFilter>().mesh = curtMesh;
                    buildCurt.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().sharedMesh = curtMesh;
                    buildCurt.transform.GetChild(i).GetComponent<Cloth>().coefficients = clothCo;
                }
            }
        }
        else if (curtOpt == 1)
        {
            float origX = 6.095f;
            float origY = 2.134f;
            float scaleX = (maxTileX - minTileX) / origX;
            float scaleY = (maxTileY - minTileY) / origY;
            float newCurtX = (minTileX + maxTileX) / 2;
            var newCurtPos = curtTrans.TransformPoint(new Vector3(newCurtX, minTileY, 0));
            buildCurt = Instantiate(Clear_Single_Curtain, newCurtPos, curtTrans.rotation);
            var clothCo = buildCurt.GetComponent<Cloth>().coefficients;
            var curtMesh = Instantiate(buildCurt.GetComponent<MeshFilter>().sharedMesh);
            var curtVerts = curtMesh.vertices;
            for (var j = 0; j < curtVerts.Length; j++)
            {
                curtVerts[j] = new Vector3(curtVerts[j].x * scaleX, curtVerts[j].y * scaleY, curtVerts[j].z);
            }
            curtMesh.vertices = curtVerts;
            buildCurt.GetComponent<MeshFilter>().mesh = curtMesh;
            buildCurt.GetComponent<SkinnedMeshRenderer>().sharedMesh = curtMesh;
            buildCurt.GetComponent<Cloth>().coefficients = clothCo;
        }
        else if (curtOpt == 2)
        {
            float origX = 6.095f;
            float origY = 2.134f;
            float scaleX = (maxTileX - minTileX) / origX;
            float scaleY = (maxTileY - minTileY) / origY;
            float newCurtX = (minTileX + maxTileX) / 2;
            var newCurtPos = curtTrans.TransformPoint(new Vector3(newCurtX, minTileY, 0));
            buildCurt = Instantiate(Yellow_Single_Curtain, newCurtPos, curtTrans.rotation);
            var clothCo = buildCurt.GetComponent<Cloth>().coefficients;
            var curtMesh = Instantiate(buildCurt.GetComponent<MeshFilter>().sharedMesh);
            var curtVerts = curtMesh.vertices;
            for (var j = 0; j < curtVerts.Length; j++)
            {
                curtVerts[j] = new Vector3(curtVerts[j].x * scaleX, curtVerts[j].y * scaleY, curtVerts[j].z);
            }
            curtMesh.vertices = curtVerts;
            buildCurt.transform.GetComponent<MeshFilter>().mesh = curtMesh;
            buildCurt.GetComponent<SkinnedMeshRenderer>().sharedMesh = curtMesh;
            buildCurt.GetComponent<Cloth>().coefficients = clothCo;
        }
    }

    public SortedVerts FindTileVerts(RaycastHit curtHit, Transform curtTrans)
    {
        var tileObject = curtHit.collider.gameObject;
        var tileMesh = CopyTileMesh(tileObject);

        return FindTileVerts(tileMesh, curtHit.collider.gameObject.transform, curtTrans);
    }

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

    public SortedVerts FindTileVerts(Mesh tileMesh, Transform tileTransform, Transform curtTransform)
    {

        var outVerts = new SortedVerts();
        //var tileMesh = Instantiate(curtHit.collider.gameObject.GetComponent<MeshFilter>().sharedMesh);
        var tileVerts = tileMesh.vertices;
        //var sortedTileVerts = new SortedVerts();
        var worldTileVerts = new List<Vector3>();
        var curtTileVerts = new List<Vector3>();
        var matchedTileVerts = new List<Vector3>();
        var tarVerts = 64;
        for (var i = 0; i < tileVerts.Length; i++)
        {
            worldTileVerts.Add(tileTransform.TransformPoint(tileVerts[i]));
        }
        for (var i = 0; i < tileVerts.Length; i++)
        {
            curtTileVerts.Add(curtTransform.InverseTransformPoint(worldTileVerts[i]));
        }
        for (var i = 0; i < curtTileVerts.Count; i++)
        {
            if (curtTileVerts[i].z > -1f && curtTileVerts[i].z < 1f)
            {
                matchedTileVerts.Add(curtTileVerts[i]);
            }
        }
        //using distinct() cuts verts to unique values (removes verts added for normals).
        IEnumerable<Vector3> distinctVerts = matchedTileVerts.Distinct();
        List<Vector3> distinctMatchedVerts = new List<Vector3>();
        List<Vector3> frontMatchedVerts = new List<Vector3>();
        List<Vector3> backMatchedVerts = new List<Vector3>();
        foreach (var item in distinctVerts)
        {
            distinctMatchedVerts.Add(item);
        }
        for (var i = 0; i < distinctMatchedVerts.Count; i++)
        {
            if (distinctMatchedVerts[i].z < 0)
            {
                frontMatchedVerts.Add(distinctMatchedVerts[i]);
            }
            else if (distinctMatchedVerts[i].z > 0)
            {
                backMatchedVerts.Add(distinctMatchedVerts[i]);
            }
        }
        var trimBackTileVerts = (backMatchedVerts.OrderBy(Vector3 => Vector3.z).ToList()).GetRange(0, tarVerts);
        var trimFrontTileVerts = (frontMatchedVerts.OrderByDescending(Vector3 => Vector3.z).ToList()).GetRange(0, tarVerts);
        var frontTileVerts = new SortedVerts();
        var backTileVerts = new SortedVerts();
        frontTileVerts.ri = ((trimFrontTileVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(tarVerts - 9, 9)).OrderBy(Vector3 => Vector3.y).ToList();
        frontTileVerts.le = ((trimFrontTileVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(0, 9)).OrderBy(Vector3 => Vector3.y).ToList();
        frontTileVerts.to = ((trimFrontTileVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(tarVerts - 25, 25)).OrderBy(Vector3 => Vector3.x).ToList();
        frontTileVerts.bo = ((trimFrontTileVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(0, 25)).OrderBy(Vector3 => Vector3.x).ToList();
        backTileVerts.ri = ((trimBackTileVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(tarVerts - 9, 9)).OrderBy(Vector3 => Vector3.y).ToList();
        backTileVerts.le = ((trimBackTileVerts.OrderBy(Vector3 => Vector3.x).ToList()).GetRange(0, 9)).OrderBy(Vector3 => Vector3.y).ToList();
        backTileVerts.to = ((trimBackTileVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(tarVerts - 25, 25)).OrderBy(Vector3 => Vector3.x).ToList();
        backTileVerts.bo = ((trimBackTileVerts.OrderBy(Vector3 => Vector3.y).ToList()).GetRange(0, 25)).OrderBy(Vector3 => Vector3.x).ToList();
        for (var i = 0; i < frontTileVerts.le.Count; i++)
        {
            outVerts.le.Add(new Vector3(frontTileVerts.le[i].x - (0 - frontTileVerts.le[i].z) * (backTileVerts.le[i].x - frontTileVerts.le[i].x) / (backTileVerts.le[i].z - frontTileVerts.le[i].z), frontTileVerts.le[i].y, 0));
        }
        for (var i = 0; i < frontTileVerts.ri.Count; i++)
        {
            outVerts.ri.Add(new Vector3(frontTileVerts.ri[i].x - (0 - frontTileVerts.ri[i].z) * (backTileVerts.ri[i].x - frontTileVerts.ri[i].x) / (backTileVerts.ri[i].z - frontTileVerts.ri[i].z), frontTileVerts.ri[i].y, 0));
        }
        for (var i = 0; i < frontTileVerts.to.Count; i++)
        {
            outVerts.to.Add(new Vector3(frontTileVerts.to[i].x, frontTileVerts.to[i].y + (0 - frontTileVerts.to[i].z) * (backTileVerts.to[i].y - frontTileVerts.to[i].y) / (backTileVerts.to[i].z - frontTileVerts.to[i].z), 0));
        }
        for (var i = 0; i < frontTileVerts.bo.Count; i++)
        {
            outVerts.bo.Add(new Vector3(frontTileVerts.bo[i].x, frontTileVerts.bo[i].y + (0 - frontTileVerts.bo[i].z) * (backTileVerts.bo[i].y - frontTileVerts.bo[i].y) / (backTileVerts.bo[i].z - frontTileVerts.bo[i].z), 0));
        }
        return outVerts;
    }
}
