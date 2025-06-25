using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapBoxTest : MonoBehaviour
{
    public Bounds Bounds;

    private Collider[] _hits;
    private int _numHits = 0;

    // Start is called before the first frame update
    void Start()
    {
        _hits = new Collider[25];
    }

    // Update is called once per frame
    void Update()
    {
        _numHits = Physics.OverlapBoxNonAlloc(transform.TransformPoint(Bounds.center), Bounds.extents, _hits);

        for (int i = 0; i < _numHits; i++)
        {
            Debug.Log($"Hit: {_hits[i].name}");
        }
    }

    private void OnDrawGizmos()
    {
        if (_numHits <= 0)
            Gizmos.color = Color.cyan;
        else
            Gizmos.color = Color.red;

        Gizmos.DrawCube(transform.TransformPoint(Bounds.center), Bounds.size);
    }
}
