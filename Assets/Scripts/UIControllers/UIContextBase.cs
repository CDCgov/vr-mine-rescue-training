using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIContextBase : MonoBehaviour
{
    protected UIContextData _context;

    protected virtual void Start()
    {
        _context = transform.GetComponentInParent<UIContextData>();
        if (_context == null)
        {
            //Debug.LogError($"{GetType().Name}: Couldn't find UI Context Data for {gameObject.name}");
            this.enabled = false;
            throw new System.Exception($"{GetType().Name}: Couldn't find UI Context Data for {gameObject.name}");
        }
    }
}
