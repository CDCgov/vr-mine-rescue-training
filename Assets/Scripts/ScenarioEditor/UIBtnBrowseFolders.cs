using SFB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnBrowseFolders : UIButtonBase
{
    public string ContextVariable = "SCENARIO_FOLDER";

    private long _lastClick;

    protected override void OnButtonClicked()
    {
        Debug.Log("UIBtnBrowseFolders Clicked");
        //the button can receive the return keypress while the folder window is open resulting in back-to-back activations
        //add a time delay for now to prevent this

        var curTime = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        if (curTime - _lastClick < 300)
                return;

        string startingFolder = null;
        if (_context != null)
            startingFolder = _context.GetStringVariable(ContextVariable);

        if (startingFolder == null)
            startingFolder = "";

        var path = StandaloneFileBrowser.OpenFolderPanel("Select custom scenario folder", startingFolder, false);
        if (path.Length > 0 && path[0] != null && path[0].Length > 0 && _context != null)
        {
            Debug.Log($"Changing folder {ContextVariable} to {path[0]}");
            _context.SetVariable(ContextVariable, path[0]);
        }

        _lastClick = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond; 
    }
}
