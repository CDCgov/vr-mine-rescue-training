using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Camera just to get things rolling can replace with something fancier later.
/// </summary>
public class SimpleFlyCamera : CameraLogic
{
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private float focusDistance = 1;
    private bool isActive = true;
    private float movementModifier = 1f;
    [SerializeField] Transform pivotObject;

    private void Start()
    {
        isActive = false;
    }
    void LateUpdate()
    {
        if (!isActive) { return; }
        if (Input.GetMouseButton(1))
        {
            transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0) * rotationSpeed * Time.deltaTime, Space.World);
            transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0) * rotationSpeed * Time.deltaTime, Space.Self);
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            movementModifier = 2f;
        }
        else
        {
            movementModifier = 1f;
        }
        transform.Translate(GetBaseInput() * moveSpeed * movementModifier * Time.deltaTime, Space.Self);
        pivotObject.position = new Vector3(transform.position.x,0,transform.position.z);
    }

    private Vector3 GetBaseInput()
    {
        if(EventSystem.current.IsPointerOverGameObject())
        {
            
            return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        }
        else
        {
            return new Vector3(Input.GetAxis("Horizontal"), Input.mouseScrollDelta.y, Input.GetAxis("Vertical"));
        }
        
    }

    public override void Activate()
    {
        isActive = true;
    }

    public override void Deactivate()
    {
        isActive = false;
    }

    // TODO consider using raycast and or bounds of object collider for focus instead of object position
    public override void FocusObject(GameObject go)
    {
        transform.LookAt(go.transform);
        Vector3 orbitPosition = (transform.position - go.transform.position).normalized * focusDistance;
        transform.position = go.transform.position + orbitPosition;
        pivotObject.position = new Vector3(go.transform.position.x,0, go.transform.position.z);
    }
}