using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[JsonObject(MemberSerialization.OptOut)]
public class SavedScenarioHeader
{
    public string ScenarioName;
    public DateTime CreationDateTime;
    public DateTime ModifiedDateTime;
    public string VRMineVersion;
}
