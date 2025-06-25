using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Cloth))]
public class ClothObserveLocalColliders : MonoBehaviour
{
    public float Radius = 100;
    private Cloth _cloth;

    public List<CapsuleCollider> _capsuleColliders = new List<CapsuleCollider>();
    private List<SphereCollider> _sphereColliders = new List<SphereCollider>();

    // Use this for initialization
    void Start()
    {
        _cloth = GetComponent<Cloth>();

        //InvokeRepeating("UpdateColliders", 0.1f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateColliders();
    }

    void UpdateColliders()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, Radius);

        _capsuleColliders.Clear();
        _sphereColliders.Clear();

        if (_cloth == null)
            _cloth = GetComponent<Cloth>();

        for (int i = 0; i < colliders.Length; i++)
        {
            CapsuleCollider capsuleCol = colliders[i] as CapsuleCollider;
            if (capsuleCol != null)
            {
                _capsuleColliders.Add(capsuleCol);
                continue;
            }


            /*SphereCollider sphereCol = colliders[i] as SphereCollider;
            if (sphereCol != null)
            {
                _sphereColliders.Add(sphereCol);
                continue;
            }*/
        }

        if (_cloth.capsuleColliders.Length != _capsuleColliders.Count)
            _cloth.capsuleColliders = _capsuleColliders.ToArray();
        else
        {
            for (int i = 0; i < _capsuleColliders.Count; i++)
            {
                if (_cloth.capsuleColliders[i] != _capsuleColliders[i])
                {
                    _cloth.capsuleColliders = _capsuleColliders.ToArray();
                }
            }
        }

        /*
        foreach (CapsuleCollider col in _cloth.capsuleColliders)
        {
            if (!_capsuleColliders.Contains(col))
            {
                _cloth.capsuleColliders = _capsuleColliders.ToArray();
                Debug.LogFormat("Updating capsule colliders on {0}", gameObject.name);
                break;
            }
        } */

        
        
    }
}
