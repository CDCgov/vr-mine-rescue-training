using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleSwitch : MonoBehaviour
{
    public TMP_Text LabelText;

    [SerializeField] RectTransform switchHandle;
    [SerializeField] RectTransform switchPoint_Left;
    [SerializeField] RectTransform switchPoint_Right;
    [SerializeField] float duration;

    public UnityEvent<bool> onToggleComplete;
    public UnityEngine.EventSystems.EventSystem UIEventSystem;    
    public Toggle ToggleControl;
    public TextMeshProUGUI StateLabel;
    public string OnStateText = "On";
    public string OffStateText = "Off";

    float timeElapsed;

    private void Awake()
    {
        ToggleControl = GetComponent<Toggle>();
        ToggleControl.onValueChanged.AddListener(delegate { Toggle(ToggleControl.isOn); });
        if(UIEventSystem == null)
        {
            UIEventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        }
    }

    private void OnDestroy()
    {
        if(ToggleControl != null)
        {
            ToggleControl.onValueChanged.RemoveListener(delegate { Toggle(ToggleControl.isOn); });
        }
    }

    public void SetLabelText(string label)
    {
        if (LabelText == null)
            return;

        LabelText.text = label;
    }

    public void Toggle(bool state)
    {
        Vector3 targetSwitchPoint = Vector3.zero;
        if (!state) 
        { 
            targetSwitchPoint = switchPoint_Left.localPosition; 
        }
        else 
        { 
            targetSwitchPoint = switchPoint_Right.localPosition; 
        }
        StopAllCoroutines();
        IEnumerator coroutine = MoveSwitchHandle(targetSwitchPoint, state);
        StartCoroutine(coroutine);
        //Debug.Log("toggle switch");
    }

    public void ToggleInstantly(bool state)
    {
        Vector3 targetSwitchPoint = Vector3.zero;
        if (!state)
        {
            targetSwitchPoint = switchPoint_Left.localPosition;
        }
        else
        {
            targetSwitchPoint = switchPoint_Right.localPosition;
        }
        switchHandle.localPosition = targetSwitchPoint;
        Debug.Log($"Switch handle moved to: {switchHandle.localPosition}");
        ToggleControl.SetIsOnWithoutNotify(state);

        if (StateLabel != null)
        {
            StateLabel.text = state ? OnStateText : OffStateText;
        }
    }

    public void ToggleWithoutNotify(bool state)
    {
        Vector3 targetSwitchPoint = Vector3.zero;
        if (!state)
        {
            targetSwitchPoint = switchPoint_Left.localPosition;
        }
        else
        {
            targetSwitchPoint = switchPoint_Right.localPosition;
        }

        switchHandle.localPosition = targetSwitchPoint;
        if (ToggleControl != null)
        {
            ToggleControl.SetIsOnWithoutNotify(state);
        }
        if (StateLabel != null)
        {
            StateLabel.text = state ? OnStateText : OffStateText;
        }
    }

    public Toggle GetToggleControl()
    {
        return ToggleControl;
    }

    //Cannot interact with the positioning code when the tab/game object is not active without it bugging out. This allows for the visuals to update to match the Toggle's data state upon enabling
    private void OnEnable()
    {
        if (LabelText != null)
        {
            Debug.Log($"Setting toggle switch postiion of {LabelText.text} to state of {ToggleControl.isOn}?");
        }
        Vector3 targetSwitchPoint = Vector3.zero;
        if (!ToggleControl.isOn)
        {
            targetSwitchPoint = switchPoint_Left.localPosition;
        }
        else
        {
            targetSwitchPoint = switchPoint_Right.localPosition;
        }

        switchHandle.localPosition = targetSwitchPoint;
        
        if (StateLabel != null)
        {
            StateLabel.text = ToggleControl.isOn ? OnStateText : OffStateText;
        }
    }

    public bool GetToggleButtonState()
    {
        return ToggleControl.isOn;
    }

    IEnumerator MoveSwitchHandle(Vector3 targetSwitchPoint, bool state)
    {
        Debug.Log($"Move switch handle called! {gameObject.name}");
        // disable to prevent race condition
        ToggleControl.interactable = false;
        
        var startPosition = switchHandle.localPosition;
        //move the switch
        while (timeElapsed < duration)
        {
            switchHandle.position = Vector3.Lerp(startPosition, targetSwitchPoint, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //halt the switch and reset
        switchHandle.localPosition = targetSwitchPoint;
        timeElapsed = 0;

        // wait for the handle to stop moving before invoking event
        onToggleComplete.Invoke(state);
        if (StateLabel != null)
        {
            StateLabel.text = state ? OnStateText : OffStateText;
        }
        // re-enable to prevent race condition
        ToggleControl.interactable = true;
        //Selects the toggle switch upon successful toggle, in line with other handles in the UI
        //if (UIEventSystem.currentSelectedGameObject == null)
        //{
        //    UIEventSystem.SetSelectedGameObject(gameObject);
        //}
        yield break;        
    }
}
