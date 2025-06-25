using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMapManReticleHoverBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Graphic Reticle;
    //public TextMeshProUGUI Cross;

    public Color StartColor;
    public Color HoverColor;

    private void Start()
    {
        if(Reticle == null)
        {
            if(!TryGetComponent<Graphic>(out Reticle))
            {
                Debug.LogError("No reticle assigned or found on Mapman Reticle hover");
                return;
            }
        }
        //if (Cross == null)
        //{
        //    Cross = GetComponentInChildren<TextMeshProUGUI>();
        //    if (Cross == null)
        //    {
        //        Debug.LogError("No cross assigned or found on Mapman Reticle hover");
        //        return;
        //    }
        //}

        SetColor(StartColor);
    }

    private void OnEnable()
    {
        SetColor(StartColor);
    }

    //public void OnHover()
    //{
    //    if (Reticle == null)// || Cross == null)
    //        return;

    //    Reticle.color = HoverColor;
    //    //Cross.color = Color.cyan;
    //}

    //public void OffHover()
    //{
    //    if (Reticle == null)// || Cross == null)
    //        return;

    //    Reticle.color = StartColor;
    //    //Cross.color = Color.green;

    //}

    public void OnPointerExit(PointerEventData eventData)
    {
        SetColor(StartColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetColor(HoverColor);
    }

    private void SetColor(Color color)
    {
        if (Reticle != null)
            Reticle.color = color;
    }
}
