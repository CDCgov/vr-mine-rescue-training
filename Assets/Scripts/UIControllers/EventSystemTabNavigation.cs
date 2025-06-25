using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventSystem))]
public class EventSystemTabNavigation : MonoBehaviour
{
    public GameObject InitialKeyboardNavigationObject;

    private EventSystem _eventSystem;

    void Start()
    {
        TryGetComponent<EventSystem>(out _eventSystem);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_eventSystem.currentSelectedGameObject == null)
            {
                if (InitialKeyboardNavigationObject != null)
                    _eventSystem.SetSelectedGameObject(InitialKeyboardNavigationObject);

                return;
            }

            Selectable selectable = null;
            Selectable next = null;
            var shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            _eventSystem.currentSelectedGameObject.TryGetComponent<Selectable>(out selectable);

            if (selectable != null)
            {
                if (shiftPressed)
                {
                    next = selectable.FindSelectableOnLeft();
                    if (next == null)
                        next = selectable.FindSelectableOnUp();
                }
                else
                {
                    next = selectable.FindSelectableOnRight();
                    if (next == null)
                        next = selectable.FindSelectableOnDown();

                }

            }

            if (next != null)
                _eventSystem.SetSelectedGameObject(next.gameObject);

        }
    }
}
