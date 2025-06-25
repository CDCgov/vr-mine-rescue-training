using System;
using UnityEngine;
using System.Collections;

public class CommandConsoleCommandAttribute : Attribute
{
    public string CommandName;
    public string Description;

    public CommandConsoleCommandAttribute(string commandName, string commandDesc)
    {
        CommandName = commandName;
        Description = commandDesc;
    }
}
