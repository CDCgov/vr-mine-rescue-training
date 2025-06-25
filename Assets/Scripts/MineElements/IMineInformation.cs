using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMineInformation
{    
    string GetMineInfo();
    string GetMineInfo(Vector3 location);

    int GetPriority();
}
