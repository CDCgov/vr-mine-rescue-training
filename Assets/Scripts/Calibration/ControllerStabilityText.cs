using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class ControllerStabilityText : MonoBehaviour
{
    public SystemManager SystemManager;
    public PlayerManager PlayerManager;
    //public SceneLoadManager SceneLoadManager;

    public XRNode ControllerNode = XRNode.RightHand;


    private TMP_Text _text;
    private InputDevice _xrDevice;
    private StringBuilder _sb;

    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        //if (SceneLoadManager == null)
        //    SceneLoadManager = SceneLoadManager.GetDefault();

        _sb = new StringBuilder();
        _text = GetComponent<TMP_Text>();

        _xrDevice = InputDevices.GetDeviceAtXRNode(ControllerNode);

        UpdateText();
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(UpdateText), 1, 0.5f);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void UpdateText()
    {
        if (PlayerManager.CurrentPlayer == null || _xrDevice == null || _text == null)
            return;

        if (!_xrDevice.isValid)
            _xrDevice = InputDevices.GetDeviceAtXRNode(ControllerNode);

        _sb.Clear();       

        bool isTracked = false;
        if (!_xrDevice.TryGetFeatureValue(CommonUsages.isTracked, out isTracked) || !isTracked)
        {
            _sb.AppendLine("Controller\nNot\nTracked");
        }
        else
        {
            float stabilityDist;
            PlayerManager.CurrentPlayer.GetRightControllerStability(out stabilityDist);

            _sb.AppendFormat("S: {0:F0}\n", stabilityDist);
        }

        var pos = PlayerManager.CurrentPlayer.GetRightControllerPOISpace();
        var refPos = SystemManager.SystemConfig.CalibrationTestPoint.ToVector3();

        var dist = Vector3.Distance(pos, refPos);

        _sb.AppendFormat("D: {0:F0}", dist * 1000);

        _text.text = _sb.ToString();
    }
}
