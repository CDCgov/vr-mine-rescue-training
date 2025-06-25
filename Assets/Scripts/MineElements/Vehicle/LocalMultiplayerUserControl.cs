using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityStandardAssets.CrossPlatformInput;
using XInputDotNetPure;


namespace UnityStandardAssets.Vehicles.Car
{
    //[RequireComponent(typeof(CarController))]
    public class LocalMultiplayerUserControl : MonoBehaviour
    {
        //private CarController m_Car; // the car controller we want to use
        public int PlayerNumber;

        private Rigidbody rb;
        public bool _ConfigMode = true;
        private int ControllersConnected = 0;

        public bool ShowDebugInfo = false;

        public int _PlayerOneValue = 0;
        public int _PlayerTwoValue = 0;
        public int _PlayerThreeValue = 0;
        private List<LocalMultiplayerUserControl> OtherPlayers;

        private WheelCollider[] _wheelColliders;
       

        private void Awake()
        {
            // get the car controller
            //m_Car = GetComponent<CarController>();
            rb = GetComponent<Rigidbody>();
            if (PlayerNumber == 1)
            {
                foreach (string id in Input.GetJoystickNames())
                {
                    Debug.Log(id);
                }
            }
            LocalMultiplayerUserControl[] controls = GameObject.FindObjectsOfType<LocalMultiplayerUserControl>();
            OtherPlayers = new List<LocalMultiplayerUserControl>();
            foreach (LocalMultiplayerUserControl control in controls)
            {
                if (control != this)
                {
                    OtherPlayers.Add(control);
                    Debug.Log(control.name);
                }
            }
        }

        private void Start()
        {
            //_wheelColliders = m_Car.GetWheelColliders();
        }


        private void FixedUpdate()
        {
            float h = 0;
            float v = 0;
            float handbrake = 0;
            bool reverse = false;
            GamePadState state = GamePad.GetState((PlayerIndex)(PlayerNumber - 1));
            //Debug.Log("Right Trigger P1: " + state.Triggers.Right);

            //if (_ConfigMode)
            //{                
            //    if (PlayerNumber == 1)
            //    {
            //        //User: Press Player one trigger! Repeat for Player 2 and 3
            //        switch (ControllersConnected)
            //        {
            //            case 0:
            //                for (int i = 1; i < 17; i++)
            //                {
            //                    if (Input.GetButtonDown("Jump" + i))
            //                    {
            //                        _PlayerOneValue = i;
            //                        ControllersConnected++;
            //                        //Debug.Log(Input.GetJoystickNames()[i] + " is player 1.");
            //                        Debug.Log("Player 1: " + i);
            //                    }
            //                }
            //                break;
            //            case 1:
            //                for (int i = 1; i < 17; i++)
            //                {
            //                    if (Input.GetButtonDown("Jump" + i) && i != _PlayerOneValue)
            //                    {
            //                        _PlayerTwoValue = i;
            //                        ControllersConnected++;
            //                        //Debug.Log(Input.GetJoystickNames()[i] + " is player 2.");
            //                        Debug.Log("Player 2: " + i);
            //                    }
            //                }
            //                break;
            //            case 2:
            //                for (int i = 1; i < 17; i++)
            //                {
            //                    if (Input.GetButtonDown("Jump" + i) && (i != _PlayerTwoValue))// && i != _PlayerOneValue)
            //                    {
            //                        _PlayerThreeValue = i;
            //                        ControllersConnected++;
            //                        //Debug.Log(Input.GetJoystickNames()[i-1] + " is player 3.");
            //                        Debug.Log("Player 3: " + i);
            //                    }
            //                }
            //                break;
            //            default:
            //                _ConfigMode = false;
            //                foreach(LocalMultiplayerUserControl lmuc in OtherPlayers)
            //                {
            //                    lmuc._PlayerOneValue = _PlayerOneValue;
            //                    lmuc._PlayerTwoValue = _PlayerTwoValue;
            //                    lmuc._PlayerThreeValue = _PlayerThreeValue;
            //                    lmuc._ConfigMode = false;

            //                }
            //                break;
            //        }
            //    }

            //}
            //else
            //{
            h = state.ThumbSticks.Left.X;
            v = state.Triggers.Left * -1 + state.Triggers.Right;
            reverse = state.Buttons.X == ButtonState.Pressed;
            //switch (PlayerNumber)
            //{
            //    case 1:
            //        h = CrossPlatformInputManager.GetAxis("Horizontal" + _PlayerOneValue);
            //        v = CrossPlatformInputManager.GetAxis("Triggers" + _PlayerOneValue); //CHANGE BACK TO Triggers
            //                                                                             //handbrake = CrossPlatformInputManager.GetButton("Jump" + _PlayerOneValue);
            //        reverse = CrossPlatformInputManager.GetButton("Jump" + _PlayerOneValue);
            //        //Debug.Log(h);
            //        //if (CrossPlatformInputManager.GetButton("Fire1"))
            //        //{
            //        //    transform.rotation = Quaternion.identity;
            //        //    transform.Translate(new Vector3(0, 1, 0));
            //        //}
            //        break;
            //    case 2:
            //        h = CrossPlatformInputManager.GetAxis("Horizontal" + _PlayerTwoValue);
            //        v = CrossPlatformInputManager.GetAxis("Triggers" + _PlayerTwoValue);//CHANGE BACK TO Triggers2
            //        reverse = CrossPlatformInputManager.GetButton("Jump" + _PlayerTwoValue);
            //        break;
            //    case 3:
            //        h = CrossPlatformInputManager.GetAxis("Horizontal" + _PlayerThreeValue);
            //        v = CrossPlatformInputManager.GetAxis("Triggers" + _PlayerThreeValue);//CHANGE BACK TO Triggers3
            //        reverse = CrossPlatformInputManager.GetButton("Jump" + _PlayerThreeValue);
            //        break;
            //    case 4:
            //        h = CrossPlatformInputManager.GetAxis("Horizontal4");
            //        v = CrossPlatformInputManager.GetAxis("Vertical4");//CHANGE BACK TO Triggers4
            //        handbrake = CrossPlatformInputManager.GetAxis("Jump4");
            //        break;
            //    default:
            //        h = 0;
            //        v = 0;
            //        handbrake = 0;
            //        break;
            //}
            // pass the input to the car!

            //original controls
            //if (reverse)
            //{
            //    v = -v;
            //    m_Car.Move(h, v, 0, handbrake);
            //}
            //else
            //{
            //    if (v > 0)
            //    {
            //        m_Car.Move(h, v, 0, handbrake);
            //    }
            //    else if (Mathf.Abs(v) < 0.1f)
            //    {
            //        m_Car.Move(h, 0, -0.2f, handbrake);
            //    }
            //    else
            //    {
            //        m_Car.Move(h, 0, v, handbrake);
            //    }
            //}

            if (ShowDebugInfo)
            {
                var wheel = _wheelColliders[0];
                Debug.Log($"Controls v:{v:F2} h:{h:F2} rpm:{wheel.rpm:F0} trq:{wheel.motorTorque:F0} brk:{wheel.brakeTorque:F0}");
            }
            
            //simple controls
            if (Mathf.Abs(v) < 0.1f)
            {
                //m_Car.Move(h, 0, -1f, handbrake);
            }
            else
            {
                var rpm = _wheelColliders[0].rpm;
                var reversing = rpm * v;
                if (Mathf.Abs(rpm) > 5 && reversing < 0)
                {
                    //m_Car.Move(h, 0, -1f, handbrake);
                }
                else
                {
                    //m_Car.Move(h, v, 0, handbrake);
                }
            }
            //}
        }
    }
}
