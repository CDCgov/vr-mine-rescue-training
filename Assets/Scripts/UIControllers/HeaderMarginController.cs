using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Changes the size of the window header when its obscured by boundary UI elements
/// </summary>
public class HeaderMarginController : MonoBehaviour
{
    [Tooltip("References to an external transform. Parent it to other UI elements that are competing for UI space with the header. If either bound is empty, the script instance destroys itself.")]
    public RectTransform LeftBound_rtfm;
    [Tooltip("References to an external transform. Parent it to other UI elements that are competing for UI space with the header. If either bound is empty, the script instance destroys itself.")]
    public RectTransform RightBound_rtfm;
   
    
    public TextMeshProUGUI Header_tmp;
    public RectTransform Header_rtfm;

    void Start()
    {
        //destroy if no boundaries present
        if (VarsValid() == false) 
        {
            Destroy(this);
            return;
        }
    }


    private void LateUpdate()
    {
        if(VarsValid() == true)
        {
            //calculate distance between each boundary and the center of the header rtfm
            float rightDistance = Vector3.Distance(Header_rtfm.position + new Vector3(Header_rtfm.rect.width/2, 0, 0), RightBound_rtfm.position);
            float leftDistance = Vector3.Distance(Header_rtfm.position - new Vector3(Header_rtfm.rect.width/2,0,0), LeftBound_rtfm.position);

            //set the right and left margins to the distance calculated
            Header_tmp.margin = new Vector4(leftDistance, 0, rightDistance, 0);
        }
    }

    bool VarsValid()
    {
        return LeftBound_rtfm != null && RightBound_rtfm != null && Header_tmp != null && Header_rtfm != null;
    }

}
