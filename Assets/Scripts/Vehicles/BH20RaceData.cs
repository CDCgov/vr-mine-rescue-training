using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BH20RaceData : MonoBehaviour
{
    public float MachineCapacity = 40000;

    [FormerlySerializedAs("FollowPrefab")]
    public GameObject GuideProjectilePrefab;
    public GameObject LaunchPrefab;

    [System.NonSerialized]
    public float CoalLoad;
    [System.NonSerialized]
    public float CoalMinedLb;

    public Transform LoadZone;
    public Transform UnloadZone;

    private BH20Controller _controller;

    private Vector3 _startPos;
    private Quaternion _startRot;

    //Get load carred in lb
    public float GetCarriedLoad()
    {
        return CoalLoad * MachineCapacity;
    }

    public void LaunchGuideProjectile()
    {
        if (GuideProjectilePrefab == null)
            return;

        var obj = GameObject.Instantiate<GameObject>(GuideProjectilePrefab);
        obj.transform.position = transform.TransformPoint(new Vector3(0, 1, 2));
        obj.transform.rotation = transform.rotation;

        var fly = obj.AddComponent<FlyToTarget>();
        fly.Lifetime = 6.0f;
        if (CoalLoad > 0.8f)
        {
            fly.Target = UnloadZone.position;
        }
        else
        {
            fly.Target = LoadZone.position;
        }
    }

    public void LaunchProjectile()
    {
        if (LaunchPrefab == null)
            return;


        Transform t = transform;
        if (_controller != null)
        {
            t = _controller.WheelModels[0].transform.parent;
        }

        var obj = GameObject.Instantiate<GameObject>(LaunchPrefab);
        obj.transform.position = transform.TransformPoint(new Vector3(0, 1, 2));
        obj.transform.rotation = t.rotation;

        Destroy(obj, 5.0f);
    }

    public void ResetPosition()
    {
        transform.position = _startPos;
        transform.rotation = _startRot;

        var rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<BH20Controller>();

        _startPos = transform.position;
        _startRot = transform.rotation;
    }	

    // Update is called once per frame
    void Update()
    {

    }
}
