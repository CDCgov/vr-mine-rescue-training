using UnityEngine;

public class CameraPivotMovement : MonoBehaviour
{
    private bool isPanning;

    private Vector3 diff;
    private Vector3 start;
    LayerMask mask;
    [SerializeField]float pivotHeight;

    private Ray ray;
    private RaycastHit hit;
    private void Start()
    {
        mask = LayerMask.GetMask("PanPlane");
        mask += LayerMask.GetMask("SelectedObject");
    }
    public void MovePivot(float moveSpeed, Transform cameraT)
    {
        Vector3 motionVec = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            motionVec += cameraT.forward;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            motionVec += -cameraT.forward;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            motionVec += -cameraT.right;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            motionVec += cameraT.right;
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
        if (Input.GetMouseButton(2))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
            Vector3 direction = (transform.position - Camera.main.transform.position).normalized * Input.mouseScrollDelta.y;
            transform.position = new Vector3(transform.position.x + direction.x, pivotHeight, transform.position.z + direction.z);
        }
    }
}
