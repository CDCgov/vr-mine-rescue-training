using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRenderBetweenTransforms : MonoBehaviour
{
    public Transform StartPoint;
    public Transform EndPoint;

    private LineRenderer _lineRenderer;
    private Vector3 _pos1;
    private Vector3 _pos2;

    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        if (StartPoint == null || EndPoint == null || _lineRenderer == null)
        {
            this.enabled = false;
            Debug.LogError($"LineRenderBetweenTransforms invalid configuration on {gameObject.name}");
        }

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, Vector3.zero);

        _pos1 = _pos2 = Vector3.zero;

    }

    // Update is called once per frame
    void Update()
    {
        var pos1 = StartPoint.position;
        var pos2 = EndPoint.position;

        UpdatePoint(0, pos1, ref _pos1);
        UpdatePoint(1, pos2, ref _pos2);
    }

    private void UpdatePoint(int index, Vector3 pos, ref Vector3 oldPos)
    {
        if (pos != oldPos)
        {
            oldPos = pos;
            _lineRenderer.SetPosition(index, pos);
        }
    }
}
