using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitConfirmation : MonoBehaviour
{
    public void OnExit()
    {
        Application.Quit();
    }

    public void OnCancel()
    {
        gameObject.SetActive(false);
    }
}
