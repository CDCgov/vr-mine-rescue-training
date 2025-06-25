using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeakyFeederCable : MonoBehaviour {

    [HideInInspector]
    public bool bNodesSpawned = false;
    
    public LeakyFeederNode FirstNode;
    public Transform LeakyFeederNodesParent;

    public bool UseCommZones = false;

    bool _VizSpawned = false;
    Mesh _mesh;
    MeshRenderer _VisualizationRenderer;
    Vector3[] _verticies;
    int[] _tris;
    List<Vector3> _vertList;
    List<int> _triList;
    List<int> _centerIndexes;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F1))
        {
            VisualizeField();            
        }
    }


    private void VisualizeField()
    {
        if (!_VizSpawned)
        {
            LeakyFeederNode source = FirstNode;
            LeakyFeederNode dest = FirstNode.NextNodes[0];
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            cube.transform.position = new Vector3(5000, 5000, 5000);
            cube.GetComponent<Renderer>().material.color = Color.red;
            //Destroy(cube.GetComponent<BoxCollider>());
            cube.GetComponent<BoxCollider>().enabled = false;
            //LeakyFeederLineRen line = GetComponent<LeakyFeederLineRen>();

            _mesh = LeakyFeederNodesParent.GetComponent<MeshFilter>().mesh;
            _mesh.Clear();
            _vertList = new List<Vector3>();
            _triList = new List<int>();
            _centerIndexes = new List<int>();

            Vector3 negVec = -1 * Vector3.Normalize(dest.transform.position - source.transform.position);
            Vector3 sourcePosFix = source.transform.position;
            sourcePosFix.y = sourcePosFix.y - 0.4f;

            _vertList.Add(sourcePosFix);

            RaycastHit hit;
            //int layerMask = 1 << 12;
            //layerMask = ~layerMask;
            //line.CreateCylinderBetweenTwoPoints(sourcePosFix, sourcePosFix + source.Range * negVec, 0.01f);
            Vector3 center;
            if (Physics.Linecast(sourcePosFix, sourcePosFix + source.Range * negVec, out hit))
            {
                //GameObject vis = Instantiate(cube);
                //vis.transform.position = hit.point;
                //line.CreateCylinderBetweenTwoPoints(sourcePosFix, hit.point, 0.01f);
                center = hit.point;
            }
            else
            {
                //GameObject vis = Instantiate(cube);
                //vis.transform.position = sourcePosFix + source.Range * negVec;
                //line.CreateCylinderBetweenTwoPoints(sourcePosFix, vis.transform.position, 0.01f);
                center = sourcePosFix + source.Range * negVec;
            }

            List<Vector3> positives = new List<Vector3>();
            List<Vector3> negatives = new List<Vector3>();

            for (int i = 1; i < 90; i = i + 1)
            {
                Vector3 yPlusRot = new Vector3(0, i, 0);
                Vector3 yNegRot = new Vector3(0, -i, 0);
                Vector3 yPlusPnt = Vector3.Normalize(Quaternion.Euler(yPlusRot) * negVec);
                Vector3 yNegPnt = Vector3.Normalize(Quaternion.Euler(yNegRot) * negVec);


                //RaycastHit hit;
                //int layerMask = 1 << 12;
                //layerMask = ~layerMask;

                if (Physics.Linecast(sourcePosFix, sourcePosFix + source.Range * yPlusPnt, out hit))
                {
                    //GameObject vis = Instantiate(cube);
                    //vis.transform.position = hit.point;
                    //line.CreateCylinderBetweenTwoPoints(sourcePosFix, hit.point, 0.01f);
                    //Vector3 fix = vis.transform.position;
                    //fix.y = source.transform.position.y;
                    positives.Add(hit.point);
                }
                else
                {
                    //GameObject vis = Instantiate(cube);
                    //vis.transform.position = sourcePosFix + source.Range * yPlusPnt;
                    //line.CreateCylinderBetweenTwoPoints(sourcePosFix, sourcePosFix + source.Range * yPlusPnt, 0.01f);
                    //line.CreateCylinderBetweenTwoPoints(sourcePosFix, vis.transform.position, 0.01f);
                    positives.Add(sourcePosFix + source.Range * yPlusPnt);
                }


                //RaycastHit hit;
                //int layerMask = 1 << 12;
                //layerMask = ~layerMask;

                if (Physics.Linecast(sourcePosFix, sourcePosFix + source.Range * yNegPnt, out hit))
                {
                    //GameObject vis = Instantiate(cube);
                    //vis.transform.position = hit.point;
                    //line.CreateCylinderBetweenTwoPoints(sourcePosFix, hit.point, 0.01f);
                    negatives.Insert(0, hit.point);
                }
                else
                {
                    //GameObject vis = Instantiate(cube);
                    //vis.transform.position = sourcePosFix + source.Range * yNegPnt;
                    //line.CreateCylinderBetweenTwoPoints(sourcePosFix, vis.transform.position, 0.01f);
                    negatives.Insert(0, sourcePosFix + source.Range * yNegPnt);
                }

            }

            _centerIndexes.Add(0);
            _vertList.AddRange(negatives);
            _vertList.Add(center);
            _vertList.AddRange(positives);

            for (int i = 1; i < (_vertList.Count - 1); i++)
            {
                _triList.Add(0);
                _triList.Add(i);
                _triList.Add(i + 1);
            }

            //int centerInd = 0;
            bool firstPass = true;
            int positiveIndex = 1;
            int priorPosIndex = 0;
            int negativeIndex = _vertList.Count - 1;
            int priorNegativeIndex = 0;
            int centerIndex = 0;
            int priorCenterIndex = 0;

            while (dest != null)
            {
                Vector3 dir = Vector3.Normalize(dest.transform.position - source.transform.position);
                sourcePosFix = source.transform.position;
                sourcePosFix.y = sourcePosFix.y - 0.4f;
                negVec = dir;
                //_vertList.Add(sourcePosFix);

                for (int i = 0; i < 100; i++)
                {
                    Vector3 pos = source.transform.position + ((i * 0.01f) * Vector3.Magnitude(dest.transform.position - source.transform.position) * dir);
                    pos.y = pos.y - 0.4f;
                    Vector3 posPnt = Vector3.Normalize(Vector3.Cross(Vector3.up, dir));
                    Vector3 negPnt = -posPnt;

                    if (!firstPass)
                    {
                        _vertList.Add(pos);
                        priorCenterIndex = centerIndex;
                        centerIndex = _vertList.Count - 1;
                    }

                    //RaycastHit hit;
                    //int layerMask = 1 << 12;
                    //layerMask = ~layerMask;
                    if (Physics.Linecast(pos, pos + source.Range * posPnt, out hit))
                    {
                        //GameObject vis = Instantiate(cube);
                        //vis.transform.position = hit.point;
                        //vis.GetComponent<Renderer>().material.color = Color.cyan;
                        //line.CreateCylinderBetweenTwoPoints(pos, hit.point, 0.01f);

                        //Vector3 fix = vis.transform.position;
                        //fix.y = source.transform.position.y;
                        _vertList.Add(hit.point);
                    }
                    else
                    {
                        //GameObject vis = Instantiate(cube);
                        //vis.transform.position = pos + source.Range * posPnt;
                        //line.CreateCylinderBetweenTwoPoints(pos, vis.transform.position, 0.01f);
                        _vertList.Add(pos + source.Range * posPnt);

                    }
                    priorPosIndex = positiveIndex;
                    positiveIndex = _vertList.Count - 1;


                    //RaycastHit hit;
                    //int layerMask = 1 << 12;
                    //layerMask = ~layerMask;
                    if (Physics.Linecast(pos, pos + source.Range * negPnt, out hit))
                    {
                        //GameObject vis = Instantiate(cube);
                        //vis.transform.position = hit.point;

                        //line.CreateCylinderBetweenTwoPoints(pos, hit.point, 0.01f);
                        //Vector3 fix = vis.transform.position;
                        //fix.y = source.transform.position.y;
                        _vertList.Add(hit.point);
                    }
                    else
                    {
                        //GameObject vis = Instantiate(cube);
                        //vis.transform.position = pos + source.Range * negPnt;
                        //line.CreateCylinderBetweenTwoPoints(pos, vis.transform.position, 0.01f);
                        _vertList.Add(pos + source.Range * negPnt);

                    }
                    priorNegativeIndex = negativeIndex;
                    negativeIndex = _vertList.Count - 1;

                    if (firstPass)
                    {
                        _triList.Add(0);
                        _triList.Add(positiveIndex);
                        _triList.Add(1);

                        _triList.Add(0);
                        _triList.Add(_vertList.Count - 3);
                        _triList.Add(_vertList.Count - 1);
                        firstPass = false;
                    }
                    else
                    {
                        _triList.Add(centerIndex);
                        _triList.Add(positiveIndex);
                        _triList.Add(priorPosIndex);

                        _triList.Add(priorPosIndex);
                        _triList.Add(priorCenterIndex);
                        _triList.Add(centerIndex);

                        _triList.Add(centerIndex);
                        _triList.Add(priorCenterIndex);
                        _triList.Add(priorNegativeIndex);

                        _triList.Add(priorNegativeIndex);
                        _triList.Add(negativeIndex);
                        _triList.Add(centerIndex);
                    }
                }
                //Vector3 savedDir = dir;
                source = dest;
                if (source.NextNodes.Count != 0)
                {

                    if (source.NextNodes[0] != null)
                    {
                        dest = source.NextNodes[0];
                        float angle = Vector3.Angle(dir, Vector3.Normalize(dest.transform.position - source.transform.position));

                        if (Mathf.Abs(angle) > 45)
                        {

                            Vector3 negDir = Vector3.Normalize(dest.transform.position - source.transform.position);
                            negDir = -negDir;
                            //int turnPriorPosIndex = 0;
                            //int turnPriorNegIndex = 0;
                            float signedAngle = AngleSigned(dir, Vector3.Normalize(dest.transform.position - source.transform.position), Vector3.up);
                            sourcePosFix = source.transform.position;
                            sourcePosFix.y = sourcePosFix.y - 0.4f;
                            Debug.Log("angle change threshold exceeded: " + source.name + ", " + signedAngle);
                            if (Physics.Linecast(sourcePosFix, sourcePosFix + source.Range * negDir, out hit))
                            {
                                //GameObject vis = Instantiate(cube);
                                //vis.transform.position = hit.point;
                                //line.CreateCylinderBetweenTwoPoints(sourcePosFix, hit.point, 0.01f);
                            }
                            else
                            {
                                //GameObject vis = Instantiate(cube);
                                //vis.transform.position = sourcePosFix + source.Range * negDir;
                                //line.CreateCylinderBetweenTwoPoints(sourcePosFix, vis.transform.position, 0.01f);
                            }


                            for (int i = 1; i <= 90; i = i + 1)
                            {
                                Vector3 yPlusRot = new Vector3(0, i, 0);
                                Vector3 yNegRot = new Vector3(0, -i, 0);
                                Vector3 yPlusPnt = Vector3.Normalize(Quaternion.Euler(yPlusRot) * negDir);
                                Vector3 yNegPnt = Vector3.Normalize(Quaternion.Euler(yNegRot) * negDir);


                                //RaycastHit hit;
                                //int layerMask = 1 << 12;
                                //layerMask = ~layerMask;
                                if (signedAngle > 0)
                                {
                                    if (Physics.Linecast(sourcePosFix, sourcePosFix + source.Range * yPlusPnt, out hit))
                                    {
                                        //GameObject vis = Instantiate(cube);
                                        //vis.transform.position = hit.point;
                                        //line.CreateCylinderBetweenTwoPoints(sourcePosFix, hit.point, 0.01f);
                                        //Vector3 fix = vis.transform.position;
                                        //fix.y = source.transform.position.y;
                                        _vertList.Add(hit.point);
                                    }
                                    else
                                    {
                                        //GameObject vis = Instantiate(cube);
                                        //vis.transform.position = sourcePosFix + source.Range * yPlusPnt;
                                        //line.CreateCylinderBetweenTwoPoints(sourcePosFix, sourcePosFix + source.Range * yPlusPnt, 0.01f);
                                        //line.CreateCylinderBetweenTwoPoints(sourcePosFix, vis.transform.position, 0.01f);
                                        _vertList.Add(sourcePosFix + source.Range * yPlusPnt);
                                    }
                                    priorNegativeIndex = negativeIndex;
                                    negativeIndex = _vertList.Count - 1;

                                    //_triList.Add(centerIndex);
                                    //_triList.Add(priorCenterIndex);
                                    //_triList.Add(priorNegativeIndex);

                                    //_triList.Add(priorNegativeIndex);
                                    //_triList.Add(negativeIndex);
                                    //_triList.Add(centerIndex);

                                    _triList.Add(centerIndex);
                                    _triList.Add(priorNegativeIndex);
                                    _triList.Add(negativeIndex);

                                }
                                else
                                {
                                    if (Physics.Linecast(sourcePosFix, sourcePosFix + source.Range * yNegPnt, out hit))
                                    {
                                        //GameObject vis = Instantiate(cube);
                                        //vis.transform.position = hit.point;
                                        //line.CreateCylinderBetweenTwoPoints(sourcePosFix, hit.point, 0.01f);
                                        _vertList.Add(hit.point);
                                    }
                                    else
                                    {
                                        //GameObject vis = Instantiate(cube);
                                        //vis.transform.position = sourcePosFix + source.Range * yNegPnt;
                                        //line.CreateCylinderBetweenTwoPoints(sourcePosFix, sourcePosFix + source.Range * yNegPnt, 0.01f);
                                        _vertList.Add(sourcePosFix + source.Range * yNegPnt);
                                    }
                                    priorPosIndex = positiveIndex;
                                    positiveIndex = _vertList.Count - 1;

                                    //_triList.Add(centerIndex);
                                    //_triList.Add(positiveIndex);
                                    //_triList.Add(priorPosIndex);

                                    //_triList.Add(priorPosIndex);
                                    //_triList.Add(priorCenterIndex);
                                    //_triList.Add(centerIndex);
                                    _triList.Add(centerIndex);
                                    _triList.Add(positiveIndex);
                                    _triList.Add(priorPosIndex);
                                }



                            }
                        }
                    }
                    else
                    {
                        dest = null;
                    }
                }
                else
                {
                    dest = null;

                    positives.Clear();
                    negatives.Clear();

                    //negVec = -negVec;
                    sourcePosFix = source.transform.position;
                    sourcePosFix.y = sourcePosFix.y - 0.4f;
                    //Center line!
                    _vertList.Add(sourcePosFix);
                    priorCenterIndex = centerIndex;
                    centerIndex = _vertList.Count - 1;

                    if (Physics.Linecast(sourcePosFix, sourcePosFix + source.Range * negVec, out hit))
                    {
                        //GameObject vis = Instantiate(cube);
                        //vis.transform.position = hit.point;                    
                        //line.CreateCylinderBetweenTwoPoints(sourcePosFix, hit.point, 0.01f);
                        center = hit.point;
                    }
                    else
                    {
                        //GameObject vis = Instantiate(cube);
                        //vis.transform.position = sourcePosFix + source.Range * negVec;
                        //vis.GetComponent<Renderer>().material.color = Color.green;
                        //line.CreateCylinderBetweenTwoPoints(sourcePosFix, vis.transform.position, 0.01f);
                        center = sourcePosFix + source.Range * negVec;
                    }


                    for (int i = 0; i <= 90; i = i + 1)
                    {
                        Vector3 yPlusRot = new Vector3(0, i, 0);
                        Vector3 yNegRot = new Vector3(0, -i, 0);
                        Vector3 yPlusPnt = Vector3.Normalize(Quaternion.Euler(yPlusRot) * negVec);
                        Vector3 yNegPnt = Vector3.Normalize(Quaternion.Euler(yNegRot) * negVec);


                        //RaycastHit hit;
                        //int layerMask = 1 << 12;
                        //layerMask = ~layerMask;

                        if (Physics.Linecast(sourcePosFix, sourcePosFix + source.Range * yPlusPnt, out hit))
                        {
                            //GameObject vis = Instantiate(cube);
                            //vis.transform.position = hit.point;
                            //line.CreateCylinderBetweenTwoPoints(sourcePosFix, hit.point, 0.01f);
                            //Vector3 fix = vis.transform.position;
                            //fix.y = source.transform.position.y;
                            positives.Add(hit.point);
                        }
                        else
                        {
                            //GameObject vis = Instantiate(cube);
                            //vis.transform.position = sourcePosFix + source.Range * yPlusPnt;
                            //line.CreateCylinderBetweenTwoPoints(sourcePosFix, sourcePosFix + source.Range * yPlusPnt, 0.01f);
                            //line.CreateCylinderBetweenTwoPoints(sourcePosFix, vis.transform.position, 0.01f);
                            positives.Add(sourcePosFix + source.Range * yPlusPnt);
                        }


                        //RaycastHit hit;
                        //int layerMask = 1 << 12;
                        //layerMask = ~layerMask;

                        if (Physics.Linecast(sourcePosFix, sourcePosFix + source.Range * yNegPnt, out hit))
                        {
                            //GameObject vis = Instantiate(cube);
                            //vis.transform.position = hit.point;
                            //line.CreateCylinderBetweenTwoPoints(sourcePosFix, hit.point, 0.01f);
                            negatives.Insert(0, hit.point);
                        }
                        else
                        {
                            //GameObject vis = Instantiate(cube);
                            //vis.transform.position = sourcePosFix + source.Range * yNegPnt;
                            //line.CreateCylinderBetweenTwoPoints(sourcePosFix, vis.transform.position, 0.01f);
                            negatives.Insert(0, sourcePosFix + source.Range * yNegPnt);
                        }
                    }

                    priorNegativeIndex = negativeIndex;
                    negativeIndex = _vertList.Count;

                    _vertList.AddRange(negatives);
                    _vertList.Add(center);
                    _vertList.AddRange(positives);

                    priorPosIndex = positiveIndex;
                    positiveIndex = _vertList.Count - 1;

                    //Add quads
                    _triList.Add(centerIndex);
                    _triList.Add(positiveIndex);
                    _triList.Add(priorPosIndex);
                    _triList.Add(priorPosIndex);
                    _triList.Add(priorCenterIndex);
                    _triList.Add(centerIndex);

                    _triList.Add(priorCenterIndex);
                    _triList.Add(priorNegativeIndex);
                    _triList.Add(negativeIndex);
                    _triList.Add(negativeIndex);
                    _triList.Add(centerIndex);
                    _triList.Add(priorCenterIndex);

                    for (int i = negativeIndex; i < positiveIndex; i++)
                    {
                        _triList.Add(centerIndex);
                        _triList.Add(i);
                        _triList.Add(i + 1);
                    }


                }
            }
            //int count = 0;
            //Debug.Log(_vertList.Count + ", vertlist count");
            //foreach(Vector3 point in _vertList)
            //{
            //    //Debug.Log(count + ": " + point.ToString());
            //   // _vertList[]
            //    count++;
            //}

            for (int i = 0; i < _vertList.Count; i++)
            {
                _vertList[i] = _vertList[i] - transform.position;
            }

            _mesh.vertices = _vertList.ToArray();
            _mesh.triangles = _triList.ToArray();
            _mesh.RecalculateNormals();
            _VisualizationRenderer = LeakyFeederNodesParent.GetComponent<MeshRenderer>();
            _VizSpawned = true;
        }
        else
        {
            _VisualizationRenderer.enabled = !_VisualizationRenderer.enabled;
        }
    }

    public float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }
}
