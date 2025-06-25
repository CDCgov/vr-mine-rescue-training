using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public interface ISelectableObject
{
    string GetObjectDisplayName();
    void GetObjectInfo(StringBuilder sb);
}