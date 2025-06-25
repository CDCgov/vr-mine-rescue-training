using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

[RequireComponent(typeof(BH20Controller))]
public class BH20GamepadControl : MonoBehaviour
{

    public PlayerIndex GamepadIndex;

    private BH20Controller _controller;
    private BH20RaceData _raceData;

    private GamePadState _lastState;

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<BH20Controller>();
        _raceData = GetComponent<BH20RaceData>();
    }

    // Update is called once per frame
    void Update()
    {
        GamePadState state = GamePad.GetState(GamepadIndex);

        float steering = state.ThumbSticks.Left.X;
        float accel = state.Triggers.Left * -1 + state.Triggers.Right;

        _controller.DriveCenterPivot(steering);

        if (Mathf.Abs(accel) > 0.1f)
        {
            _controller.Accelerate(accel);
        }
        else
        {
            _controller.Brake(1.0f);
        }

        if (state.Buttons.LeftStick == ButtonState.Pressed && _lastState.Buttons.LeftStick == ButtonState.Released)
        {
            _controller.SpeedBoost(10.0f);
        }

        if (_raceData != null)
        {
            if (state.Buttons.Y == ButtonState.Pressed && _lastState.Buttons.Y == ButtonState.Released)
            {
                _raceData.LaunchGuideProjectile();
            }

            if (state.Buttons.A == ButtonState.Pressed && _lastState.Buttons.A == ButtonState.Released)
            {
                _raceData.LaunchProjectile();
            }
        }

        _lastState = state;
        
    }
}
