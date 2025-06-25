using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LinkLineBehavior : MonoBehaviour
{
    public GameObject TextDisplayObject;
    public TextMeshPro DisplayText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnActivateLinkLine()
    {
        DisplayText.text = "Linked\nStay in Place";
        TextDisplayObject.SetActive(true);
    }

    public void HideDisplay()
    {
        TextDisplayObject.SetActive(false);
    }

    public void OnLevelLoading()
    {
        DisplayText.text = "Loading. . .";
        TextDisplayObject.SetActive(true);
    }
}
