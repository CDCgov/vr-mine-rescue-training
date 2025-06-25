using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class VentUIObj : MonoBehaviour, ISelectableObjectInfo
{
    public VentObj VentObj;
    //public VentAirway Airway;

    public GameObject MethaneVisual;

    private MeshRenderer _meshRenderer;

    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();
        SetColor(Color.blue);
    }

    private void Start()
    {
        
        

        
    }

    public void UpdatePosition(float y_height)
    {
        if (VentObj == null)
            return;

        if (VentObj is VentJunction)
        {
            var junc = (VentJunction)VentObj;
            Vector3 pos = junc.WorldPosition;
            //pos.y = y_height;

            transform.position = pos;

            if (junc.IsStartJunction)
                SetColor(Color.green);
            else
                SetColor(Color.blue);
        }
        else if (VentObj is VentAirway)
        {
            var airway = (VentAirway)VentObj;
            var pos = (airway.Start.WorldPosition + airway.End.WorldPosition) * 0.5f;
            var dir = airway.End.WorldPosition - airway.Start.WorldPosition;

            //pos.y = y_height;

            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            if (MethaneVisual != null)
            {
                if (airway.CH4EmissionRate > 0)
                    MethaneVisual.SetActive(true);
                else
                    MethaneVisual.SetActive(false);
            }


            if (airway.Resistance > 500)
            {
                SetColor(Color.red);
            }
            else if (airway.Resistance > 8)
            {
                SetColor(new Color(1.0f, 0.5f, 0.0f));
            }
            else
            {
                SetColor(Color.yellow);
            }
        }
        else if (VentObj is VentFire)
        {
            Vector3 pos = ((VentFire)VentObj).WorldPosition;
            pos.y = y_height;

            transform.position = pos;
        }
    }

    private void SetColor(Color color)
    {
        _mpb.SetColor("_BaseColor", color);
        _mpb.SetColor("_EmissionColor", color);

        if (_meshRenderer != null)
        {            
            _meshRenderer.SetPropertyBlock(_mpb);
        }
    }

    public void SetResistance(double resistance)
    {
        var airway = VentObj as VentAirway;
        if (airway == null)
            return;

        airway.OverrideResistance = resistance;
        airway.CalculateResistance();
        airway.UpdateAirway();
    }

    public void AppendText(StringBuilder sb)
    {
        if (VentObj == null)
        {
            sb.AppendLine("Invalid VentUIObj");
        }
        else 
        {
            VentObj.AppendText(sb);
            //Junction.AppendText(sb);
        }
        //else if (Airway != null)
        //{
        //    Airway.AppendText(sb);
        //}
        //else
        //    sb.AppendLine("VentUIObj: No Data");
    }

    public override string ToString()
    {
        if (VentObj == null)
            return "VentUIObj: No Data";
        else
            return VentObj.ToString();
        //if (Junction != null && Airway != null)
        //{
        //    return "Invalid VentUIObj";
        //}
        //else if (Junction != null)
        //{
        //    return Junction.ToString();
        //}
        //else if (Airway != null)
        //{
        //    return Airway.ToString();
        //}
        //else
        //    return "VentUIObj: No Data";
    }

    public void GetObjectInfo(StringBuilder sb)
    {
        sb.AppendLine(ToString());
    }
}
