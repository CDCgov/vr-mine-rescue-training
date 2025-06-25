using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IMinimizableWindow
{
    
    event System.Action<string> TitleChanged;
    string GetTitle();
    void Minimize(bool minimize);
    void ToggleMinimize();

    void AssignTaskbarButton(Button button);





}
