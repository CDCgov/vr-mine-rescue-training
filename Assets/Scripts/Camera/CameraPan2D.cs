using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


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
        bool buttonHeld = Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed;

        //if (EventSystem.current.currentSelectedGameObject != null)
        //    Debug.Log($"CurrentSelectedGameObject: {EventSystem.current.currentSelectedGameObject.name}");

        if ((Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame) && !overUIObject)
            _mouseCaptured = true;

        var pos = _camera.ScreenToWorldPoint(Mouse.current.GetPositionVec3());

        if (buttonHeld && _mouseCaptured)
        {
            var delta = pos - _mouseStartPos;
            delta.y = 0;

            if (saveFirstPosition)
            {
                SaveStartPosition();
            }


            _mouseStartPos = pos;

            _camera.transform.position = _camera.transform.position - delta;
            _mouseStartPos = _camera.ScreenToWorldPoint(Mouse.current.GetPositionVec3());
        }
        else
        {
            _mouseCaptured = false;
            _mouseStartPos = pos;
        }
    }

    public void SaveStartPosition()
    {
        _cachedPosition = _camera.transform.position;
        saveFirstPosition = false;
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
