using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionEventManager : IDisposable
{

    public enum InputActionEvent
    {
        None = 0,
        Performed,
        Started,
        Cancelled
    }

    private struct InputActionCallbackData
    {
        public InputAction Action;
        public InputActionEvent EventType;
        public Action<InputAction.CallbackContext> Callback;
    }

    private List<InputActionCallbackData> _callbackData = new List<InputActionCallbackData>();

    public void RegisterPerformedHandler(string actionNameOrId, Action<InputAction.CallbackContext> callback,
        InputActionAsset actionAsset = null)
    {
        RegisterHandler(actionNameOrId, InputActionEvent.Performed, callback, actionAsset);
    }

    public void RegisterStartedHandler(string actionNameOrId, Action<InputAction.CallbackContext> callback,
        InputActionAsset actionAsset = null)
    {
        RegisterHandler(actionNameOrId, InputActionEvent.Started, callback, actionAsset);
    }

    public void RegisterCancelledHandler(string actionNameOrId, Action<InputAction.CallbackContext> callback,
        InputActionAsset actionAsset = null)
    {
        RegisterHandler(actionNameOrId, InputActionEvent.Cancelled, callback, actionAsset);
    }

    public void RegisterHandler(string actionNameOrId, InputActionEvent eventType, Action<InputAction.CallbackContext> callback, 
        InputActionAsset actionAsset = null)
    {
        if (actionAsset == null)
            actionAsset = InputSystem.actions;

        var action = actionAsset.FindAction(actionNameOrId);
        if (action == null)
            return;

        var data = new InputActionCallbackData
        {
            Action = action,
            EventType = eventType,
            Callback = callback,
        };

        Register(data);
        _callbackData.Add(data);
    }


    public void UnregisterAll()
    {
        for (int i = 0; i < _callbackData.Count; i++)
        {
            Unregister(_callbackData[i]);
        }

        _callbackData.Clear();
    }

    private void Register(InputActionCallbackData data)
    {
        if (data.Action == null || data.Callback == null)
            return;

        switch (data.EventType)
        {
            case InputActionEvent.Performed:
                data.Action.performed += data.Callback;
                break;

            case InputActionEvent.Started:
                data.Action.started += data.Callback;
                break;

            case InputActionEvent.Cancelled:
                data.Action.canceled += data.Callback;
                break;
        }
    }

    private void Unregister(InputActionCallbackData data)
    {
        if (data.Action == null || data.Callback == null)
            return;

        switch (data.EventType)
        {
            case InputActionEvent.Performed:
                data.Action.performed -= data.Callback;
                break;

            case InputActionEvent.Started:
                data.Action.started -= data.Callback;
                break;

            case InputActionEvent.Cancelled:
                data.Action.canceled -= data.Callback;
                break;
        }

    }
    
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                UnregisterAll();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
