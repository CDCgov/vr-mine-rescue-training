using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleConveyorBelt : MonoBehaviour {

    public float ScrollX = 0;
    public float ScrollY = 0;
    public int MaterialID = 5;	
    // Update is called once per frame
    void Update () {
        float offsetX = Time.time * ScrollX;
        float offsetY = Time.time * ScrollY;
        GetComponent<Renderer>().materials[MaterialID].mainTextureOffset = new Vector2(offsetX,offsetY);
    }
}
