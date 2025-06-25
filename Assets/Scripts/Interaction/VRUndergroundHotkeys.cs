using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRUndergroundHotkeys : MonoBehaviour
{
    public InputBindingManager InputBindingManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        if (InputBindingManager == null)
            InputBindingManager = InputBindingManager.GetDefault();

    
    }

    private void OnDisable() 
    {
    
    }
}
