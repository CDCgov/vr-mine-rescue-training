using UnityEngine;
using System.Collections;
using System;

public class ResearcherActionAttribute : Attribute
{
    public string CommandName;

    public ResearcherActionAttribute(string name)
    {
        CommandName = name;
    }
}
