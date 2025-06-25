using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AutoMapRevealer : MonoBehaviour
{
    public GameObject[] HiddenObjects;
    public Sprite SpriteVar;
    public string NameForLabel;
    public SpriteRenderer SpriteRen;
    public TextMeshPro TmProRen;

    private bool _notTriggered = true;

    private void Start()
    {
        foreach(GameObject item in HiddenObjects)
        {
            item.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_notTriggered && other.tag == "MapPencil")
        {
            foreach(GameObject item in HiddenObjects)
            {
                item.SetActive(true);                
            }
            if (SpriteVar != null)
            {
                SpriteRen.sprite = SpriteVar;
            }
            if(TmProRen != null)
            {
                TmProRen.text = NameForLabel;
            }
            _notTriggered = false;
        }
    }

    public void Reveal()
    {
        
        foreach (GameObject item in HiddenObjects)
        {
            item.SetActive(true);
        }
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        if (SpriteVar != null)
        {
            SpriteRen.sprite = SpriteVar;
        }
        if (TmProRen != null)
        {
            TmProRen.text = NameForLabel;
        }
        _notTriggered = false;
        
    }
}
