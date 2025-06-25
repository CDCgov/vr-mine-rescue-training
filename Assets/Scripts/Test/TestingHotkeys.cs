using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingHotkeys : MonoBehaviour
{
    public List<TensionedCable> Cables;

    private bool _cableHighlighted = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.C))
            {

                var cables = GameObject.FindObjectsOfType(typeof(TensionedCable));

                if (!_cableHighlighted)
                {
                    Debug.Log("Highlighting Cable");

                    foreach (TensionedCable cable in Cables)
                    {
                        MeshRenderer rend = cable.GetComponent<MeshRenderer>();
                        rend.material.SetColor("_EmissionColor", Color.yellow);
                    }

                    foreach (var obj in cables)
                    {
                        MeshRenderer rend = ((TensionedCable)obj).GetComponent<MeshRenderer>();
                        rend.material.SetColor("_EmissionColor", Color.yellow);
                    }

                    _cableHighlighted = true;
                }
                else
                {
                    foreach (TensionedCable cable in Cables)
                    {
                        MeshRenderer rend = cable.GetComponent<MeshRenderer>();
                        rend.material.SetColor("_EmissionColor", Color.black);
                    }

                    foreach (var obj in cables)
                    {
                        MeshRenderer rend = ((TensionedCable)obj).GetComponent<MeshRenderer>();
                        rend.material.SetColor("_EmissionColor", Color.black);
                    }

                    _cableHighlighted = false;
                }
            }
        }

    }
}
