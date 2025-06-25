using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonLaserHelper : MonoBehaviour
{
    public GameObject LeftHandLaser;
    public GameObject RightHandLaser;
    public LineRenderer LeftLineRenderer;
    public LineRenderer RightLineRenderer;

    
    public bool IsOldCharacterRig = false;

    private MinerFinalIK minerFinalIK;

    private void Start()
    {
        if(minerFinalIK == null)
        {
            minerFinalIK = GetComponent<MinerFinalIK>();
        }
    }
    // Start is called before the first frame update

    public void EnableRightLaser()
    {
        RightHandLaser.SetActive(true);
        LeftHandLaser.SetActive(false);
    }

    public void EnableLeftLaser()
    {
        RightHandLaser.SetActive(false);
        LeftHandLaser.SetActive(true);
    }

    public void DisableLasers()
    {
        RightHandLaser.SetActive(false);
        LeftHandLaser.SetActive(false);
    }

    private void LateUpdate()
    {
        if (RightHandLaser.activeSelf) 
        {
            if (minerFinalIK == null)
            {               
                RightLineRenderer.enabled = true;
                RightLineRenderer.useWorldSpace = false;
                //RightLineRenderer.SetPosition(0, Vector3.zero);
                //RaycastHit _hit;
                //if (Physics.Raycast(RightHandLaser.transform.position, -RightHandLaser.transform.right, out _hit, 0.5f, LayerMask.GetMask("UI")))
                //{
                //    RightLineRenderer.SetPosition(1, _hit.point);
                //}
                //else
                //{
                //    RightLineRenderer.SetPosition(1, RightHandLaser.transform.position + (-RightHandLaser.transform.right * 0.5f));
                //}
                //RightLineRenderer.SetPosition(1, new Vector3(0, 0.5f, 0));
            }
            else
            {
                if(minerFinalIK.RightHandTarget != null)
                {
                    RightLineRenderer.enabled = true;
                    
                    
                    //if (IsOldCharacterRig)
                    //{
                    //    RightLineRenderer.useWorldSpace = true;
                    //    RightLineRenderer.SetPosition(0, minerFinalIK.RightHandTarget.position);
                    //    RightLineRenderer.SetPosition(1, minerFinalIK.RightHandTarget.position + (-minerFinalIK.RightHandTarget.right) * 0.5f);
                    //}
                    //else
                    //{
                    //    //RightLineRenderer.SetPosition(1, transform.position + (-transform.up) * 0.5f);
                    //}
                }
                else
                {
                    RightLineRenderer.enabled = true;
                    RightLineRenderer.useWorldSpace = false;
                    //RightLineRenderer.SetPosition(0, Vector3.zero);
                    //RightLineRenderer.SetPosition(1, new Vector3(0, 0.5f, 0));
                    //RightLineRenderer.SetPosition(1, transform.position + (-transform.up) * 0.5f);
                }
            }
        }
        else
        {
            RightLineRenderer.enabled = false;
        }

        if (LeftHandLaser.activeSelf)
        {
            if (minerFinalIK == null)
            {
                LeftLineRenderer.enabled = true;
                //LeftLineRenderer.SetPosition(0, Vector3.zero);
                LeftLineRenderer.useWorldSpace = false;
                //RaycastHit _hit;
                //if (Physics.Raycast(LeftHandLaser.transform.position, -LeftHandLaser.transform.right, out _hit, 0.5f, LayerMask.GetMask("UI")))
                //{
                //    LeftLineRenderer.SetPosition(1, _hit.point);
                //}
                //else
                //{
                //    LeftLineRenderer.SetPosition(1, LeftHandLaser.transform.position + (-LeftHandLaser.transform.right * 0.5f));
                //}
                //LeftLineRenderer.SetPosition(1, new Vector3(-0.5f, 0, 0));
            }
            else
            {
                if (minerFinalIK.LeftHandTarget != null)
                {
                    LeftLineRenderer.enabled = true;
                    
                    
                    //if (IsOldCharacterRig)
                    //{
                    //    LeftLineRenderer.useWorldSpace = true;
                    //    LeftLineRenderer.SetPosition(0, minerFinalIK.LeftHandTarget.position);
                    //    LeftLineRenderer.SetPosition(1, transform.position + (-transform.right) * 0.5f);
                    //}
                    //else
                    //{
                    //    //LeftLineRenderer.SetPosition(1, transform.position + (-transform.up) * 0.5f);
                    //}
                }
                else
                {
                    LeftLineRenderer.enabled = true;
                    LeftLineRenderer.useWorldSpace = false;
                    //LeftLineRenderer.SetPosition(0, Vector3.zero);
                    ////RightLineRenderer.SetPosition(1, new Vector3(-0.5f, 0, 0));
                    //LeftLineRenderer.SetPosition(1, transform.position + (-transform.up) * 0.5f);
                }
            }
        }
        else
        {
            LeftLineRenderer.enabled = false;
        }
    }
}
