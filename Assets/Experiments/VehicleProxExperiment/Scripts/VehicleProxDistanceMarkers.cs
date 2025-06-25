using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VehicleProxDistanceMarkers : MonoBehaviour
{
    public GameObject MarkerPrefab;
    public int StartAt = 1;
    public int NumMarkers = 20;
    public int FeetSeparation = 1;
    public int LabelInterval = 2; 

    // Use this for initialization
    void Start()
    {

        for (int i = 0; i < NumMarkers; i++)
        {
            var obj = Instantiate<GameObject>(MarkerPrefab, Vector3.zero, Quaternion.identity, transform);
            var text = obj.GetComponentInChildren<TextMeshPro>();

            float dist = i * FeetSeparation + StartAt;

            obj.transform.localPosition = new Vector3(0, 0, (float)dist * 0.3048f * -1);
            if (text != null)
            {
                if ((i) % LabelInterval == 0)
                {
                    text.text = dist.ToString();
                    var rend = obj.GetComponent<MeshRenderer>();
                    var mat = new Material(rend.material);
                    mat.color = text.material.color;
                    rend.material = mat;
                }
                else
                {
                    text.gameObject.SetActive(false);
                }
            }
        }
    }

}
