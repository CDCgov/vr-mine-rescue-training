using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CalibrationStatusText : SelectedPlayerControl, IStatusText
{
    public SystemManager SystemManager;
    public PlayerManager PlayerManager;


    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
    }

    private void ComputeErrorDistsMm(Vector3 pt, Vector3 refPt, out float xzDist, out float yDist)
    {
        //var testPt = SystemManager.SystemConfig.CalibrationTestPointVec3;
        var poiSpace = _player.GetRightControllerPOISpace();
        //compute Y axis error
        yDist = Mathf.Abs(refPt.y - pt.y) * 1000.0f;

        //compute XZ plane error
        refPt.y = 0;
        pt.y = 0;
        xzDist = Vector3.Distance(refPt, pt) * 1000.0f;
    }

    public void AppendStatusText(StringBuilder statusText)
    {
        if (_player == null)
            _player = PlayerManager.CurrentPlayer;

        if (_player == null)
            return;

        float leftAngle, rightAngle;
        _player.ComputeControllerAngles(out leftAngle, out rightAngle);

        //var testPt = SystemManager.SystemConfig.CalibrationTestPointVec3;
        //var poiSpace = _player.GetRightControllerPOISpace();
        ////compute Y axis error
        //var yDist = Mathf.Abs(testPt.y - poiSpace.y) * 1000.0f;

        ////compute XZ plane error
        //testPt.y = 0;
        //poiSpace.y = 0;
        //var xzDist = Vector3.Distance(testPt, poiSpace) * 1000.0f;

        //var refPt = SystemManager.SystemConfig.CalibrationTestPointVec3;
        var refPt = PlayerManager.ComputeTestPointCentroid();

        //compute live position error
        float xzDist, yDist;
        ComputeErrorDistsMm(_player.GetRightControllerPOISpace(), refPt, out xzDist, out yDist);

        //compute saved test point position error
        float xzTest, yTest;
        ComputeErrorDistsMm(_player.CalTestPoint, refPt, out xzTest, out yTest);

        float testPointCloudSize = PlayerManager.ComputeCalTestPointCloudSize();

        float stabilityDist = 0;
        _player.GetRightControllerStability(out stabilityDist);
        string stabilityColor;
        if (stabilityDist < 10.0f)
            stabilityColor = "green";
        else
            stabilityColor = "red";

        string testPointCloudSizeColor;
        if (testPointCloudSize < 120.0f)
            testPointCloudSizeColor = "green";
        else if (testPointCloudSize < 250.0f)
            testPointCloudSizeColor = "yellow";
        else
            testPointCloudSizeColor = "red";

        statusText.AppendLine($"<color=\"{stabilityColor}\">R  Stb: {stabilityDist:F1}</color>");
        statusText.AppendLine($"XZ Err: {xzDist,6:F0}mm ({xzTest,6:F0}mm)");
        statusText.AppendLine($"Y  Err: {yDist,6:F0}mm ({yTest,6:F0}mm)");
        statusText.AppendLine($"Offset: ({_player.CalibrationPos.x:F1}, {_player.CalibrationPos.y:F1}, {_player.CalibrationPos.z:F1})");
        statusText.AppendLine();
        statusText.AppendLine($"<color=\"{testPointCloudSizeColor}\">Overall range: {testPointCloudSize:F0} mm</color>");

        //statusText.AppendLine($"XZ Err: {xzDist:F0}mm Y Err: {yDist:F0}mm");
        //statusText.AppendLine($"Height: {_player.PlayerHeight:F1} Timestamp: {_player.CalReceiveTimestamp:F1}");
        //statusText.AppendLine($"CalPos: {_player.CalibrationPos} CalRot: {_player.CalibrationRot} LAngle: {leftAngle:F1} RAngle: {rightAngle:F1} Height: {_player.PlayerHeight:F1} Timestamp: {_player.CalReceiveTimestamp:F1}");
    }
}
