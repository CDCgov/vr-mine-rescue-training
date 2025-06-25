using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour 
{

    private IInputTarget _kmTarget; //target controlled by keyboard+mouse input

    public void BindKeyboardAndMouse(IInputTarget target)
    {
        _kmTarget = target;
    }

    void Start ()  
    {
    
    }
    
    void Update () 
    {
        if (_kmTarget != null)
        {
            InputTargetOptions opt = _kmTarget.GetInputTargetOptions();
            
            Vector3 motionVec = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                motionVec += Vector3.forward;

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                motionVec += Vector3.back;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                motionVec += Vector3.left;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                motionVec += Vector3.right;

            if (Input.GetKey(KeyCode.Space))
                motionVec += Vector3.up;

            if (Input.GetKey(KeyCode.C))
                motionVec += Vector3.down;

            if (motionVec.sqrMagnitude > 0.5)
                motionVec.Normalize();

            _kmTarget.SetMovementVector(motionVec);

            //determine if we should respond to mouse movement
            bool mouseCaptured = !opt.LookRequiresMousePress || Input.GetMouseButton(0);
            if (opt.ToggleMouseCapture)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    //check if we are over a GUI element
                    if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                    {

                        opt.MouseIsCaptured = !opt.MouseIsCaptured;

                        Debug.Log("Mouse Captured " + opt.MouseIsCaptured.ToString());

                        if (opt.MouseIsCaptured)
                        {
                            Cursor.lockState = CursorLockMode.Locked;
                            Cursor.visible = false;
                        }
                        else
                        {
                            Cursor.lockState = CursorLockMode.None;
                            Cursor.visible = true;
                        }
                    }

                    //Cursor.visible = opt.MouseIsCaptured;
                }

                mouseCaptured = opt.MouseIsCaptured;

            }

            if (mouseCaptured)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                Vector3 lookEulers = _kmTarget.GetLookEuler();

                lookEulers.y += mouseX;
                lookEulers.x -= mouseY;

                if (lookEulers.y > 360)
                    lookEulers.y -= 360;

                if (lookEulers.y < -360)
                    lookEulers.y += 360;

                //lookEulers.x = Mathf.Clamp(lookEulers.x, -80, 80);

                _kmTarget.SetLookEuler(lookEulers);
            }

            _kmTarget.ProcessCustomInput();
        }
    }
}