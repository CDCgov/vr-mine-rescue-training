using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineSegNavTest : MonoBehaviour 
{
    private MineSegment _curSegment;

    public MineSegment[] SegmentPathList;
    public float Speed = 15.0f;
    public float SlowSpeed = 3.0f;
    public float RotationSpeed = 220.0f;
    public float Acceleration = 25.0f;
    public bool Loop = true;
    public bool PingPong = false;
    public bool ReverseForwardDirectionOnPingPong = true;
    public bool RestartOnKeypress = false;
    public KeyCode RestartKey = KeyCode.R;

    private int _pathIndex = -1;
    private MineSegment _destSegment;

    private int _actorsInZone = 0;
    private float _currentSpeed = 0;
    private float _targetSpeed = 0;

    private Vector3 _startPos;
    private Quaternion _startRot;
    Rigidbody _rb;
    private int _pathInc = 1;
    private bool _flipForwardDir = false;

    void Start () 
    {
        _startPos = transform.position;
        _startRot = transform.rotation;
        //int count = MineNetwork.MineSegments.Length;
        //_curSegment = MineNetwork.MineSegments[Random.Range(0, count)];
        ChooseNextDestination();
        _curSegment = _destSegment;
        transform.position = _curSegment.transform.position;

        _targetSpeed = _currentSpeed = Speed;
        _rb = GetComponent<Rigidbody>();

        ChooseNextDestination();
    }

    void ChooseNextDestination()
    {
        if (SegmentPathList != null && SegmentPathList.Length >= 2)
        {
            _pathIndex += _pathInc;

            if (_pathIndex >= SegmentPathList.Length || _pathIndex < 0)
            {
                

                if (PingPong)
                {
                    _pathInc *= -1;
                    _pathIndex += (_pathInc * 2);

                    if (ReverseForwardDirectionOnPingPong)
                        _flipForwardDir = !_flipForwardDir;
                }
                else if (Loop)
                {
                    _pathIndex = 0;
                }
                else
                {
                    _pathIndex = 0;
                    _destSegment = null;
                    return;
                }
            }

            //Debug.Log(_pathIndex);

            _destSegment = SegmentPathList[_pathIndex];
        }
        else
        {
            int numLinks = _curSegment.MineSegmentLinks.Count;

            int randomLinkID = Random.Range(0, numLinks);

            //Debug.LogFormat("choosing next destination, {0} links, chose {1}", numLinks, randomLinkID);
            MineSegmentLink link = _curSegment.MineSegmentLinks[randomLinkID];

            if (link.Segment1 == _curSegment)
                _destSegment = link.Segment2;
            else
                _destSegment = link.Segment1;
        }
    }

    void Restart()
    {
        //gameObject.SetActive(false);



        //gameObject.SetActive(true);
        _destSegment = null;
        

        Invoke("Move1", 0.25f);
    }

    void Move1()
    {
        Vector3 pos = _startPos;
        pos.y = 100;
        transform.position = pos;
        transform.rotation = _startRot;
        _rb.position = pos;
        _rb.rotation = _startRot;
        _rb.velocity = Vector3.zero;

        Invoke("MoveFinal", 0.25f);
    }

    void MoveFinal()
    {
        transform.position = _startPos;
        transform.rotation = _startRot;
        _rb.position = _startPos;
        _rb.rotation = _startRot;
        _rb.velocity = Vector3.zero;

        _pathIndex = -1;
        ChooseNextDestination();

        //Invoke("EnableMe", 0.25f);
    }

    void EnableMe()
    {
        gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<ActorHost>() != null)	
            _actorsInZone++;

    }

    void OnTriggerExit(Collider col)
    {
        if (col.GetComponent<ActorHost>() != null)
            _actorsInZone--;
    }

    public void SetTargetSpeed(float targetSpeed)
    {
        _targetSpeed = targetSpeed;		
    }
    
    void Update () 
    {

        if (RestartOnKeypress)
        {
            if (Input.GetKeyDown(RestartKey))
            {
                Restart();
            }
        }

        if (_destSegment == null)
            return;

        float targetSpeed = _targetSpeed;
        float acceleration = Acceleration;

        if (_actorsInZone > 0)
        {
            targetSpeed = 0;
        }

        if (targetSpeed != _currentSpeed)
        {
            float speedChange = Time.deltaTime * acceleration;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, speedChange);
        }

        Vector3 curpos = transform.position;
        Vector3 destpos = _destSegment.transform.position;


        Vector3 dir = destpos - curpos;
        if (dir.magnitude < 0.2f)
        {
            MineSegment oldSeg = _curSegment;
            _curSegment = _destSegment;

            for (int i = 0; i < 10; i++)
            {
                ChooseNextDestination();

                if (_destSegment != oldSeg)
                    break;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(curpos, destpos, _currentSpeed * Time.deltaTime);

            if (_flipForwardDir)
                dir *= -1;

            Quaternion facingDir = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, facingDir, RotationSpeed * Time.deltaTime);
        }
        
    }
}