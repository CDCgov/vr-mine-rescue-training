using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class DebriefEventItem : MonoBehaviour
{
    public SVGImage Icon;
    public Image PlayerColorImage;
    public SVGImage HighlightImage;
    public SoundEffectCollection SoundEffect;
    public Vector3 WorldSpacePosition;

    public int PlayerID;
    public int ActionID;
    public bool IsHiddenPlayer = false;
    public bool IsHiddenAction = false;
    public SessionEventData EventData;
    public bool EventActive = false;
    

    public void EnableHighlight()
    {
        HighlightImage.enabled = true;
        transform.SetAsLastSibling();
    }

    public void DisableHighlight()
    {
        HighlightImage.enabled = false;
    }

    public void EventActivate(bool isActive)
    {
        if(gameObject == null)
        {
            return;
        }

        if (isActive && !EventActive && SoundEffect != null)
        {
            //event turned on
            SoundEffect.PlaybackRandomNonSpatial();
        }

        EventActive = isActive;
        if (isActive)
        {
            if(!IsHiddenAction && !IsHiddenPlayer)
            {
                gameObject.SetActive(true);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
