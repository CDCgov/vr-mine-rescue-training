using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeakyFeederLineRen : MonoBehaviour {

    
    public GameObject Cylinder;
    public bool IsVisible = true;
    
    // Use this for initialization
    void Start () {


        if (IsVisible)
        {
            LeakyFeederNode[] nodes = transform.GetComponentsInChildren<LeakyFeederNode>();

            for (int i = 1; i < nodes.Length; i++)
            {
                CreateCylinderBetweenTwoPoints(nodes[i - 1].transform.position, nodes[i].transform.position, 0.01f);
            }
        }
    }

    public void CreateCylinderBetweenTwoPoints(Vector3 start, Vector3 end, float width)
    {
        Vector3 offset = end - start;
        Vector3 scale = new Vector3(width, offset.magnitude / 2, width);
        Vector3 pos = start + (offset / 2);

        GameObject cyl = Instantiate(Cylinder, pos, Quaternion.identity);
        cyl.name = "LeakyFeederObj";
        cyl.transform.up = offset;
        cyl.transform.localScale = scale;
        cyl.GetComponent<Renderer>().material.color = Color.yellow;
    }
}
