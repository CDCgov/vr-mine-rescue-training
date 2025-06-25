using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactProxEffect : MonoBehaviour
{
    public GameObject MinerPrefab;
    public Transform MinerSpawnPoint;
    public GameObject MinerObject;

    private Renderer[] _renderers;

    private Rigidbody _minerRB;
    private BoxCollider _minerCol;
    private Light _sceneLight;

    private Color _sceneLightColor;

    // Use this for initialization
    void Start()
    {
        var sceneLightObj = GameObject.Find("Directional Light");
        if (sceneLightObj != null)
        {
            _sceneLight = sceneLightObj.GetComponent<Light>();
            _sceneLightColor = _sceneLight.color;
        }

        Reset();
    }

    public void Reset()
    {
        if (_sceneLight != null)
            _sceneLight.color = _sceneLightColor;


        Util.DestoryAllChildren(MinerSpawnPoint);
        if (MinerPrefab != null)
        {
            MinerObject = Instantiate<GameObject>(MinerPrefab);
            MinerObject.transform.SetParent(MinerSpawnPoint, false);
            MinerObject.transform.localPosition = Vector3.zero;
            MinerObject.transform.localRotation = Quaternion.identity;
        }

        _renderers = GetComponentsInChildren<Renderer>();

        _minerRB = MinerObject.GetComponent<Rigidbody>();
        _minerCol = MinerObject.GetComponent<BoxCollider>();
    }

    private void SpawnTestMiner(Vector3 pos)
    {
        if (MinerPrefab == null)
            return;

        RaycastHit hit;
        if (Physics.Raycast(pos, Vector3.down, out hit))
        {
            var obj = Instantiate<GameObject>(MinerPrefab);
            var xform = obj.transform;
            xform.SetParent(MinerSpawnPoint, false);
            xform.position = hit.point;
            xform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            obj.GetComponent<Rigidbody>().isKinematic = false;
            obj.GetComponent<BoxCollider>().enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled)
            return;

        if (other.gameObject.layer != 0)
            return;

        Debug.LogFormat("Impact Prox Effect trigged by {0}", other.gameObject.name);
        /*
        foreach (Renderer r in _renderers)
        {
            r.material.color = Color.red;
        } */

        //_minerCol.enabled = true;
        //_minerRB.isKinematic = false;

        if (_sceneLight != null)
        {
            _sceneLight.color = Color.red;
        }
    }
}
