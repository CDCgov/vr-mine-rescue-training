using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurtainReceiver : MonoBehaviour
{
    public bool CanAttach = true;
    public CurtainReceiver PairedReceiver;
    public GameObject IndicatorSphere;
    
    public Renderer SphereRenderer;
    public Material ValidColor;
    public GameObject ArrowIndictatorPrefab;
    public bool IsCornerPiece;

    private Material _startingMaterial;
    private GameObject _arrow;
    private GameObject _spawnedSphere;
    private Renderer _spawnedSphereRen;
    private Material _spawnedSphereStartingMaterial;

    private void Start()
    {
        //if(SphereRenderer == null)
        //{
        //    SphereRenderer = GetComponent<Renderer>();
        //}
        //_startingMaterial = SphereRenderer.material;
        //_arrow = Instantiate(ArrowIndictatorPrefab, transform);
        Vector3 dir = PairedReceiver.transform.position - transform.position;
        dir = Vector3.ProjectOnPlane(dir, Vector3.up);
        //Quaternion qr = Quaternion.FromToRotation(_arrow.transform.up, dir);
        //_arrow.transform.rotation = qr;
        //Vector3 move = transform.position + dir.normalized * 0.165f;
        //_arrow.transform.position = move;
        //_arrow.transform.parent = IndicatorSphere.transform;
    }
    public void ActivateIndicator()
    {
        //if(IndicatorSphere != null)
        //    IndicatorSphere.SetActive(true);
        if(PairedReceiver == null)
        {
            return;
        }
        if (_spawnedSphere == null)
        {
            _spawnedSphere = Instantiate(Resources.Load("IndicatorSphere", typeof(GameObject))) as GameObject;
            _spawnedSphere.transform.parent = transform;
            _spawnedSphere.transform.localPosition = Vector3.zero;
            _spawnedSphereRen = _spawnedSphere.GetComponent<Renderer>();
            if (IsCornerPiece)
            {
                _spawnedSphereStartingMaterial = Resources.Load("OrangeEmissiveTransparent", typeof(Material)) as Material;
                _spawnedSphereRen.material = _spawnedSphereStartingMaterial;
            }
            else
            {
                _spawnedSphereStartingMaterial = _spawnedSphereRen.material;
            }
        }
    }

    public void DeactivateIndicator()
    {
        if(PairedReceiver == null)
        {
            return;
        }
        if (_spawnedSphere == null)
            return;
        //if(IndicatorSphere != null)
        //    IndicatorSphere.SetActive(false);
        
        Destroy(_spawnedSphere);
        _spawnedSphere = null;
    }

    public void ChangeIndicatorColor(Color col)
    {
        if (PairedReceiver == null)
        {
            return;
        }
        if (_spawnedSphere == null)
            return;

        //SphereRenderer.material.color = col;
        _spawnedSphereRen.material.color = col;
    }

    public void SetValidColor()
    {
        if (PairedReceiver == null)
        {
            return;
        }
        if (_spawnedSphere == null)
            return;

        //SphereRenderer.material = ValidColor;
        _spawnedSphereRen.material = ValidColor;
    }

    public void RestoreStartingColor()
    {
        if (PairedReceiver == null)
        {
            return;
        }
        if (_spawnedSphere == null)
            return;

        //SphereRenderer.material = _startingMaterial;
        _spawnedSphereRen.material = _spawnedSphereStartingMaterial;
    }

    private void OnDrawGizmosSelected()
    {
        if(PairedReceiver != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, PairedReceiver.transform.position);
        }
    }
}
