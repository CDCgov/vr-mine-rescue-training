using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPivotMovement : MonoBehaviour
{
    private bool isPanning;

    private Vector3 diff;
    private Vector3 start;
    LayerMask mask;
    [SerializeField]float pivotHeight;

    private Ray ray;
    private RaycastHit hit;

    private InputAction _actionMove;
    private InputAction _actionZoom;

    private void Start()
    {
        mask = LayerMask.GetMask("PanPlane");
        mask += LayerMask.GetMask("SelectedObject");

        _actionMove = InputSystem.actions.FindAction("Move");
        _actionZoom = InputSystem.actions.FindAction("Zoom");
    }
    public void MovePivot(float moveSpeed, Transform cameraT)
    {
        Vector3 motionVec = Vector3.zero;

        //if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        //    motionVec += cameraT.forward;

        //if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        //    motionVec += -cameraT.forward;

        //if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        //    motionVec += -cameraT.right;

        //if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        //    motionVec += cameraT.right;

        Vector2 moveVec = _actionMove.ReadValue<Vector2>();
        motionVec += moveVec.y * cameraT.forward;
        motionVec += moveVec.x * cameraT.right;
        
        /*
        motionVec.y = 0;

        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.RightArrow))
            motionVec += Vector3.down;

        if (Input.GetKey(KeyCode.Space))
            motionVec += Vector3.up;
        */
        Vector3 motion = motionVec * Time.unscaledDeltaTime * moveSpeed;
        motion.y = 0;
        transform.position += motion;

    }
    public void UpdatePivotDrag()
    {
        if (Mouse.current.rightButton.IsPressed())
        {
            ray = Camera.main.ScreenPointToRay(Mouse.current.GetPositionVec3());
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                if (hit.transform.gameObject.layer == 3)
                {
                    diff = hit.point - transform.position;
                }
            }

            if (isPanning == false)
            {
                isPanning = true;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity,mask))
                {
                    if (hit.transform.gameObject.layer == 3)
                    {
                        start = hit.point;
                    }
                }
            }
        }
        else
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 movement = start - diff;
            transform.position = new Vector3(movement.x, pivotHeight, movement.z);
        }
    }

    public void UpdateZoomPosition(bool isZooming)
    {
        if(isZooming)
        {
            float zoomValue = _actionZoom.ReadValue<float>();
            Debug.Log($"CameraPivotMovement: Zoom value read {zoomValue:F2}");
            Vector3 direction = (transform.position - Camera.main.transform.position).normalized * /*Input.mouseScrollDelta.y*/zoomValue;
            transform.position = new Vector3(transform.position.x + direction.x, pivotHeight, transform.position.z + direction.z);
        }
    }
}
