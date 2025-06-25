using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


[RequireComponent(typeof(Camera))]
public class CameraPan2D : MonoBehaviour, IPointerClickHandler
{
    private Camera _camera;

    private Vector3 _mouseStartPos;
    private Vector3 _camStartPos;
    private bool _mouseCaptured = false;

    private bool saveFirstPosition = true;
    private Vector3 _cachedPosition;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        //    return;

        //if (Input.GetMouseButtonDown(0))
        //{
        //    _mouseStartPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        //    _camStartPos = _camera.transform.position;
        //}

        bool overUIObject = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool buttonHeld = Input.GetMouseButton(0) || Input.GetMouseButton(2);

        //if (EventSystem.current.currentSelectedGameObject != null)
        //    Debug.Log($"CurrentSelectedGameObject: {EventSystem.current.currentSelectedGameObject.name}");

        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2)) && !overUIObject)
            _mouseCaptured = true;

        var pos = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (buttonHeld && _mouseCaptured)
        {
            var delta = pos - _mouseStartPos;
            delta.y = 0;

            if (saveFirstPosition)
            {
                _cachedPosition = _camera.transform.position;
                saveFirstPosition = false;
            }


            _mouseStartPos = pos;

            _camera.transform.position = _camera.transform.position - delta;
            _mouseStartPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        }
        else
        {
            _mouseCaptured = false;
            _mouseStartPos = pos;
        }
    }

    public void ResetCamera()
    {
        if (saveFirstPosition)
            return; // haven't cached a position yet

        _camera.transform.position = _cachedPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"OnPointerClicked {eventData}");
    }
}
