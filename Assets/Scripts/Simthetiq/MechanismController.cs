using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mogoson.Machinery
{
    /// <summary>
    /// Script made to control the Mechanism "animation"
    /// </summary>
    
    public class MechanismController : MonoBehaviour
    {
        [Header("Handling MechAnimSetting")]
        public Mechanism handlingMech;
        public float HandlingVelocity = 1f;
        [Header("TrapDoor MechAnimSetting")]
        public Mechanism trapDoorMech;
        public float trapDoorVelocity = 10f;
        [Header("BucketHeightAnim MechAnimSetting")]
        public Mechanism bucketHeightMech;
        public float bucketHeightVelocity = 5f;
        [Header("DumpingAnim MechAnimSetting")]
        public Mechanism dumpingMech;
        public float dumpingVelocity = 10f;
        [Header("BatteryAnim MechAnimSetting")]
        public Mechanism batteryMech;
        public float batteryVelocity = 5f;
        
        // Use this for initialization
        void Start()
        {
            if (GameManager.inputManager != null)
            {
                GameManager.inputManager.LeftJoystictEvent.AddListener(HandlingMech);
                GameManager.inputManager.ButtonBumperRightEvent.AddListener(TrapDoorMech);
                GameManager.inputManager.ButtonYEvent.AddListener(BucketHeightMech);
                GameManager.inputManager.ButtonBEvent.AddListener(DumpingMech);
                GameManager.inputManager.ButtonXEvent.AddListener(BatteryMech);
            }

        }
        // all Method ending with Mech are method triggered by the input manager, each method send the value to expend or retract a specific moving part.
        public void HandlingMech(float value, InputControllerState cs)
        {
            if (cs == InputControllerState.Active)
            {
                handlingMech.Drive(HandlingVelocity * GameManager.masterSetting.directionCurve.Evaluate(Mathf.Abs(value))* -Mathf.Sign(value),
                    DriveType.Ignore);
            }
        }
        
        public void TrapDoorMech(float value, InputControllerState cs)
        {
            if (GameManager.inputManager.ButtonBumperLeftEvent.state == InputControllerState.Hold &&
                cs == InputControllerState.Hold)
            {
                trapDoorMech.Drive(-trapDoorVelocity,DriveType.Ignore);
                return;
            }
            if (cs == InputControllerState.Hold)
            {
                trapDoorMech.Drive(trapDoorVelocity, DriveType.Ignore);
            }

        }

       
        private void BucketHeightMech(float x, InputControllerState cs)
        {
            if (GameManager.inputManager.ButtonBumperLeftEvent.state == InputControllerState.Hold && cs == InputControllerState.Hold)
            {
                bucketHeightMech.Drive(-bucketHeightVelocity,DriveType.Ignore);
                return;
            }
            else if (cs == InputControllerState.Hold)
            {
                bucketHeightMech.Drive(bucketHeightVelocity, DriveType.Ignore);
            }

        }

        public void DriveDumping(bool extend)
        {
            if (extend)
            {
                dumpingMech.Drive(dumpingVelocity, DriveType.Ignore);
            }
            else
            {
                dumpingMech.Drive(-dumpingVelocity, DriveType.Ignore);
            }
        }
        
       public void DumpingMech(float value, InputControllerState cs)
       {
           if (GameManager.inputManager.ButtonBumperLeftEvent.state == InputControllerState.Hold &&
               cs == InputControllerState.Hold)
           {
               dumpingMech.Drive(-dumpingVelocity, DriveType.Ignore);
                return;
           }
           if (cs == InputControllerState.Hold)
           {
               dumpingMech.Drive(dumpingVelocity, DriveType.Ignore);
            }

       }

       public void DriveBattery(bool extend)
        {
            if (extend)
            {
                batteryMech.Drive(batteryVelocity, DriveType.Ignore);
            }
            else
            {
                batteryMech.Drive(-batteryVelocity, DriveType.Ignore);
            }
        }

        public void BatteryMech(float value, InputControllerState cs)
        {
            if (GameManager.inputManager.ButtonBumperLeftEvent.state == InputControllerState.Hold &&
                cs == InputControllerState.Hold)
            {
                batteryMech.Drive(batteryVelocity, DriveType.Ignore);
                return;
            }
            if (cs == InputControllerState.Hold)
            {
                batteryMech.Drive(-batteryVelocity, DriveType.Ignore);
            }
        }
        
    }
}