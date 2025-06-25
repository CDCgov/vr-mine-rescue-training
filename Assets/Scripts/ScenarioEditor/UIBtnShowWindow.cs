using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBtnShowWindow : UIButtonBase
{
    public GameObject WindowInstance;
    public GameObject WindowPrefab;
    public Transform ParentTransform; 


    protected override void OnButtonClicked()
    {
        if (WindowPrefab == null && WindowInstance == null)
            return;

        if (WindowInstance == null)
        {
            if (WindowPrefab == null)
                return;

            WindowInstance = GameObject.Instantiate(WindowPrefab);

            if (ParentTransform != null)
                WindowInstance.transform.SetParent(ParentTransform, false);
        }

        WindowInstance.SetActive(true);
    }
}
