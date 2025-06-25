using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public Button ColorPickButton;
    // Start is called before the first frame update
    void Start()
    {
        ColorPickButton.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        GraphicRaycaster graphicRaycaster;
        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData pointerEventData;
        EventSystem eventSystem;
        eventSystem = GetComponent<EventSystem>();
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);
        foreach (RaycastResult result in results)
        {
            
            Debug.Log($"Hit: {result.gameObject.name}");
        }
    }
}
