/*
 * NAME:LightController.cs
 * DESC:Handles the lights, enables switching the front and back light from white to red
 * Default KEYBIND:L
*/
using System;
using UnityEngine;

public class LightController : MonoBehaviour {
    public LightSO whiteLight;
    public LightSO redLight;
    public Light[] frontLights;
    public Light[] backLights;
    public LensFlare[] FrontLightFlare;
    public LensFlare[] BackLightFlair;
    public KeyCode KeyBind = KeyCode.L;
    public bool IsUsingInputManager; // if checked script will use the input manager instead of the keybind.
    public int flareFrameUpdate = 5;
    public float multiplier = 1.5f;

 
    private bool LightState = true; // true = white  - - false = red;
    private LensFlare[] flare = new LensFlare[4];
    // Use this for initialization
    void Start () {
        if(IsUsingInputManager && GameManager.inputManager != null) 
        {
            GameManager.inputManager.ButtonLeftJoystickEvent.AddListener(EventLightState);
        }

        UpdateLight(true);
        flare = GetComponentsInChildren<LensFlare>(); // added reference to new lensflare component
    }

    // Update is called once per frame
    
    void Update () {
        if (!IsUsingInputManager)
        {
            if (Input.GetKeyDown(KeyBind)) UpdateLight(LightState);
        }

        if(Time.frameCount % flareFrameUpdate == 0 ) UpdateFlareBrightness(); //flare brightness updates "each" frame. currently set to update every 5 frames. update rate can be changed using the Inspector variable flareFrameUpdate.
    }
    /*###############################################################################################################################
     *UpdateFlareBrightness() has been writen to override Default Unity Flare System.
     * we first do a reverse linear interpolation to have a smooth transition between each update.
     * we then apply a multiplier of X that can be modified to fit the desired look in the editor.
     *      - the square root formula was selected as it gave constant results. Other formulas could be used to achieve similar results but were not tested.
     * finally we apply the result to the flare brightness.
     *###############################################################################################################################*/
    private void UpdateFlareBrightness()
    {
        //float distance = Vector3.Distance(Camera.main.transform.position, transform.position) + 0.0000001f; // find the distance between the light system and the camera (light system distance is taken from the origin point aka the gizmo.
        //Debug.LogFormat("Distance: {0:F2}", distance);

        // float testBrightness = (500.0f / Mathf.Pow(distance, 2.0f));
        // testBrightness = Mathf.Clamp(testBrightness, 0, 10);

        if (Camera.main == null)
            return;

        for (int i = 0; i < flare.Length; i++)
        {
            if (flare[i] == null)
                continue;

            // float x = Mathf.InverseLerp(flare[i].brightness, distance, 10f); // inverse lerp to "override" default behavior of the unity flare system.
            // float y = (multiplier / Mathf.Sqrt(distance)) + x; //apply the multiplier to increase or decrease the effect.
            // flare[i].brightness = y; // apply brightness to the flare

            float distance = Vector3.Distance(Camera.main.transform.position, flare[i].transform.position) + 0.0000001f;
            float testBrightness = (500.0f / Mathf.Pow(distance, 2.0f));
            testBrightness = Mathf.Clamp(testBrightness, 0, 10);

            flare[i].brightness = testBrightness;
        }
    }

    private void EventLightState(float value, InputControllerState iCS)
    {
        if (iCS ==InputControllerState.Pressed)
        {
            UpdateLight(LightState);
        }
        
    }
    private void UpdateLight(bool state) // apply scriptable object parameters to the light when changing lights color
    {
        LightSO frontLightTemp = (state) ? redLight : whiteLight;
        LightSO backLightTemp = (state) ? whiteLight : redLight;
        for (int i = 0; i < FrontLightFlare.Length; i++)
        {
            frontLights[i].range = frontLightTemp.range;
            frontLights[i].intensity = frontLightTemp.intensity;
            frontLights[i].color = frontLightTemp.color;
            frontLights[i].spotAngle = frontLightTemp.spotAngle;
            FrontLightFlare[i].flare = frontLightTemp.flare;//change the flare scriptable object to mach the light color;
        }
        for (int i = 0; i < backLights.Length; i++)
        {
            backLights[i].range = backLightTemp.range;
            backLights[i].intensity = backLightTemp.intensity;
            backLights[i].color = backLightTemp.color;
            backLights[i].spotAngle = backLightTemp.spotAngle;
            BackLightFlair[i].flare = backLightTemp.flare; //change the flare scriptable object to mach the light color;
        }
        LightState = !state;
    }
}


