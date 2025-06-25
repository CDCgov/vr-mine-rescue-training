using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The script assumes both wheels are aligned on a single axis
/// (right axis)
/// </summary>
public class Balance : MonoBehaviour
{
    const float epsilon = 0.001f; // custom epsilon at 8mm
    const float epsilonAngle = .05f;

    public float min = -5f;
    public float max = 5f;

    public LayerMask raycastMask;

    public Transform leftWheelJoint;
    public Transform rightWheelJoint;

    public WheelCollider leftWheelCollider;
    public WheelCollider rightWheelCollider;

    private Vector3 groundUp;
    private Vector3 wheelsForward;
    private Vector3 wheelsUp;
    private Quaternion deltaRotation;

    private Ray leftRay;
    private Ray rightRay;

    private RaycastHit hitLeft;
    private RaycastHit hitRight;

    private float leftDistance = 0;
    private float rightDistance = 0;

    private float lastVelocity = 0;
    private float distanceBetweenWheels;
    private float initialVelocity = 0;
    private float deltaAngle = 0;
    private float targetAngle = 0;

    private float currentQuatAngle = 0;

    private FallingObjectType fallingObject = FallingObjectType.NOTHING;


    /// <summary>
    /// Using a enum to store what kind of object is currently falling 
    /// (not grounded and has a velocity)
    /// </summary>
    private enum FallingObjectType
    {
        NOTHING,
        LEFT_WHEEL,
        RIGHT_WHEEL
    }

    /// <summary>
    /// Store precious data
    /// </summary>
    private void Start()
    {
        distanceBetweenWheels = Vector3.Distance(leftWheelJoint.position, rightWheelJoint.position);
    }

    /// <summary>
    /// Are distance pseudo-equals.
    /// </summary>
    /// <returns><c>true</c>, if equals, <c>false</c> otherwise.</returns>
    /// <param name="a">The first ditance.</param>
    /// <param name="b">The second distance.</param>
    bool ArePseudoEquals(float a, float b)
    {
        return (Mathf.Abs(a - b) < epsilon);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    public static float GetSignedAngle(Quaternion A, Quaternion B, Vector3 axis)
    {
        float angle = 0f;
        Vector3 angleAxis = Vector3.zero;
        Quaternion q = B * Quaternion.Inverse(A);
        q.ToAngleAxis(out angle, out angleAxis);
        if (Vector3.Angle(axis, angleAxis) > 90f)
        {
            angle = -angle;
        }
        return Mathf.DeltaAngle(0f, angle);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="A"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    public static float GetSignedAngle(Quaternion A, Vector3 axis)
    {
        float angle = 0f;
        Vector3 angleAxis = Vector3.zero;
        (Quaternion.Inverse(A)).ToAngleAxis(out angle, out angleAxis);
        if (Vector3.Angle(axis, angleAxis) > 90f)
        {
            angle = -angle;
        }
        return Mathf.DeltaAngle(0f, angle);
    }

    /// <summary>
    /// Physics stuff
    /// </summary>
    void FixedUpdate()
    {
        leftRay = new Ray(leftWheelJoint.position, Vector3.down);
        rightRay = new Ray(rightWheelJoint.position, Vector3.down);

        bool leftHasHit = Physics.Raycast(leftRay, out hitLeft, 10f, raycastMask);
        bool rightHasHit = Physics.Raycast(rightRay, out hitRight, 10f, raycastMask);

        /// Check if Raycast is successful
        if (leftHasHit && rightHasHit)
        {
            leftDistance = hitLeft.distance - leftWheelCollider.radius;
            rightDistance = hitRight.distance - rightWheelCollider.radius;

            /// Check if distances are differents
            if (!ArePseudoEquals(leftDistance, rightDistance))
            {
                /// Double-check : distances must be greater than the radius of the wheel
                /// To avoid damping issues
                if (leftDistance >= epsilon || rightDistance >= epsilon)
                {
                    /// Compute the wheels forward axis
                    wheelsForward = Vector3.Normalize(.5f * (leftWheelJoint.forward + rightWheelJoint.forward));

                    /// Compute the wheels up axis
                    wheelsUp = Vector3.Normalize(.5f * (leftWheelJoint.up + rightWheelJoint.up));

                    /// Compute the ground normal using the wheels forward
                    groundUp = Vector3.Cross((hitLeft.point - hitRight.point).normalized, wheelsForward);

                    if (groundUp.y < 0)
                        Debug.Log("ERROR: Line 152"); // the ground is upside-down...

                    /// find the difference between ground normal and wheels up axis
                    //deltaRotation = Quaternion.Inverse(Quaternion.FromToRotation(groundUp, wheelsUp));

                    /// Get the current angle using wheels forward axis
                    deltaAngle = Vector3.SignedAngle(wheelsUp, groundUp, -wheelsForward);
                    currentQuatAngle = Mathf.DeltaAngle(0f, transform.localEulerAngles.z/*currentQuatAngle*/);

                    /// Clamp the delta rotation using min and max
                    deltaAngle = Mathf.Clamp(currentQuatAngle + deltaAngle, min, max) - currentQuatAngle;

                    /// Only perform rotation if the deltaAngle is big enough
                    /// (ex : if the left wheel is not grounded, but the object is already rotated to the maximum, we shouldn't do anything)
                    if (Mathf.Abs(deltaAngle) > epsilonAngle)
                    {
                        /// Compute the velocity of the falling object
                        initialVelocity = Physics.gravity.magnitude * Time.fixedDeltaTime;
                        switch (fallingObject)
                        {
                            case FallingObjectType.NOTHING:
                                lastVelocity = initialVelocity;
                                break;
                            case FallingObjectType.LEFT_WHEEL:
                                if (leftDistance > rightDistance)
                                    lastVelocity += initialVelocity;
                                else
                                    lastVelocity = initialVelocity;
                                break;
                            case FallingObjectType.RIGHT_WHEEL:
                                if (leftDistance > rightDistance)
                                    lastVelocity = initialVelocity;
                                else
                                    lastVelocity += initialVelocity;
                                break;
                            default:
                                lastVelocity = initialVelocity;
                                break;
                        }

                        /// Store the currently falling wheel for next frame
                        if (leftDistance > rightDistance)
                            fallingObject = FallingObjectType.LEFT_WHEEL;
                        else
                            fallingObject = FallingObjectType.RIGHT_WHEEL;

                        /// Compute the angle to target for this frame
                        targetAngle = Mathf.Atan2(lastVelocity, distanceBetweenWheels) * Mathf.Rad2Deg;

                        /// If the angle targeted is bigger than the delta (aka maximum angle) one, clamp it.
                        /// This avoid bounces from a side to another.
                        if (deltaAngle < 0)
                        {
                            targetAngle *= -1f;

                            if (targetAngle < deltaAngle)
                                targetAngle = deltaAngle;
                        }
                        else
                        {
                            if (targetAngle > deltaAngle)
                                targetAngle = deltaAngle;
                        }

                        
                        if (targetAngle > 5f)
                            Debug.Log("ERROR: Condition line 223"); // The rotation angle is too big check Collider on the ground


                        /// Perform rotation
                        transform.localRotation *= Quaternion.Euler(0, 0, targetAngle);
                    }
                    else
                    {
                        fallingObject = FallingObjectType.NOTHING;
                        lastVelocity = 0;
                    }

                }
                else
                {
                    fallingObject = FallingObjectType.NOTHING;
                    lastVelocity = 0;
                }
            }
            else
            {
                fallingObject = FallingObjectType.NOTHING;
                lastVelocity = 0;
            }
        }

        

    }
}
