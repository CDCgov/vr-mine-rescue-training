using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapmanGraphicsRaycastManager : MonoBehaviour
{
    public GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    // Start is called before the first frame update
    void Start()
    {
        m_PointerEventData = new PointerEventData(EventSystem.current);
    }
       

    //public void PerformRaycast(Vector3 point)
    //{
    //    Vector3 v3ScreenPoint = eventCamera.WorldToScreenPoint(point);
    //    Vector2 v2Screen = new Vector2(v3ScreenPoint.x, v3ScreenPoint.y);
    //    List<RaycastResult> results = new List<RaycastResult>();
    //    m_PointerEventData.position = v2Screen;
    //    m_Raycaster.Raycast(m_PointerEventData, results);
        
    //}

    public void PerformRaycast(Vector2 point)
    {        
        List<RaycastResult> results = new List<RaycastResult>();
        m_PointerEventData.position = point;
        m_PointerEventData.button = PointerEventData.InputButton.Left;
        //m_PointerEventData.
        m_Raycaster.Raycast(m_PointerEventData, results);
        List<Button> buttons = new List<Button>();
        foreach(RaycastResult res in results)
        {
            Debug.Log(res.gameObject.name);
            Button but = res.gameObject.GetComponent<Button>();
            if(but != null)
            {
                buttons.Add(but);
            }            
        }
        if(buttons.Count > 0)
        {
            //Trigger the first button encountered by the raycast
            buttons[0].onClick.Invoke();
            for (int i=0; i<buttons.Count; i++)
            {
                Debug.Log("Time: " + Time.time + ", " + buttons[i].name + ", " + buttons[i].transform.GetSiblingIndex());
                //buttons[i].onClick.Invoke();
            }
            
        }
    }
}
