using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISessionRecorder
{
    void CreateLog(string filename, VRNLogHeader logHeader);
}
