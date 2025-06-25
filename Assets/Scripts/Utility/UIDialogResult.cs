using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum DialogResult
{
    None,
    Cancel,
    Yes,
    No,
}

public delegate void DialogResultCallback(DialogResult result);

public class UIDialogResult : MonoBehaviour
{
    public DialogResult DefaultResult;

    protected DialogResult _result;
    protected DialogResultCallback _callback;

    //public async Task<DialogResult> WaitForResult()
    //{
    //    if (!this.isActiveAndEnabled)
    //        return DefaultResult;

    //    while (this.isActiveAndEnabled)
    //        await Task.Yield();

    //    return _result;
    //}

    public bool IsInUse
    {
        get
        {
            if (_callback != null)
                return true;
            else
                return false;
        }
    }

    public void ResetDialog()
    {
        _callback = null;
        //gameObject.SetActive(false);
        _result = DefaultResult;
    }

    public void SetDialogResult(DialogResult result)
    {
        _result = result;
        gameObject.SetActive(false);
    }

    public void ShowDialog(DialogResultCallback callback)
    {
        if (_callback != null)
        {
            Debug.LogWarning($"Warning: overriding callback for dialog {gameObject.name}");
        }

        _callback = callback;
        gameObject.SetActive(true);

    }

    protected virtual void OnEnable()
    {
        _result = DefaultResult;
    }

    protected virtual void OnDisable()
    {
        if (_callback != null)
        {
            try
            {
                _callback(_result);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in dialog callback {gameObject.name} {ex.Message} :: {ex.StackTrace}");
            }

            _callback = null;
        }
    }
}
