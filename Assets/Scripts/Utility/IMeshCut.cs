using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMeshCut 
{
    bool GetMeshCutInfo(out MeshCut.MeshCutInfo cutInfo);
}
