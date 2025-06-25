using UnityEngine;
using System.Collections;

public class PathAnimate : MonoBehaviour
{
    public float Speed = 10.0f;
    public float SmoothTime = 2.0f;
    public Vector3[] PathPositions;

    private int _targetIndex = 0;
    private Vector3 _startPos;

    public AnimationCurve SpeedCurve;

    private Vector3 _currentVel = Vector3.zero;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        foreach (Vector3 pos in PathPositions)
        {
            Gizmos.DrawSphere(transform.position + pos, 0.3f);
        }
    }

    // Use this for initialization
    void Start()
    {
        _startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (PathPositions == null || PathPositions.Length < 1)
            return;

        Vector3 target = PathPositions[_targetIndex] + _startPos;

        transform.position = Vector3.SmoothDamp(transform.position, target, ref _currentVel, SmoothTime, Speed);

        //transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * Speed);

        Vector3 delta = target - transform.position;		
        if (delta.magnitude < 0.1f)
            NextTarget();
        
    }

    void NextTarget()
    {
        _targetIndex++;
        if (_targetIndex >= PathPositions.Length)
            _targetIndex = 0;
    }
}
