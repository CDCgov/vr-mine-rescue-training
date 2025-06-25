using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_EditorLayers;
using System;

namespace NIOSH_MineCreation
{
    public class VentilationConnectionAirway : LayerControlledClass
    {
        private MeshRenderer _renderer;
        
        private VentAirway airway;
        
        private VentGraph _ventGraph;

        private bool endPointsSet = false;

        private Transform start;
        private Transform end;

        public static bool AirwayBeingConstructed = false;

        protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
        {
            if (newLayer == LayerManager.EditorLayer.Ventilation)
            {
                _renderer.enabled = true;
            }
            else
            {
                _renderer.enabled = false;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            base.Start();

            _renderer = GetComponent<MeshRenderer>();
            
            _ventGraph = GameObject.Find("VentilationControl").GetComponent<VentilationControl>().VentGraph;
            
            CheckInitalLayer();
        }

        private void Update()
        {
            if (endPointsSet)
            {
                UpdatePhysicalShape();
            }
        }

        private void UpdatePhysicalShape()
        {
            if (start == null || end == null)
            {
                Destroy(gameObject);
                return;
            }

            PositionConnectionVisual(transform, start.transform.position, end.position);
            
            ////move airway between two conneciton points
            //transform.position = end.position;
            //transform.LookAt(start.transform.position);
            //transform.Translate(Vector3.forward * (Vector3.Distance(end.position, start.position) / 2.0f));

            ////scale airway to connect the points
            //transform.localScale = new Vector3(0.2f, 
            //                                   0.2f, 
            //                                   Vector3.Distance(end.position, start.position));
        }

        public static void PositionConnectionVisual(Transform xform, Vector3 startPos, Vector3 endPos)
        {
            if (xform == null)
                return;

            xform.position = endPos;// + new Vector3(0, 0.1f, 0);//Vector3.up;
            xform.LookAt(startPos/* + new Vector3(0, 0.1f, 0)*/);
            xform.Translate(Vector3.forward * (Vector3.Distance(endPos, startPos) / 2.0f));

            //scale airway to connect the points
            xform.localScale = new Vector3(0.2f,
                                               0.2f,
                                               Vector3.Distance(endPos, startPos));
        }

        private void OnDisable()
        {
            if (airway != null && _ventGraph != null && _ventGraph.FindAirway(airway.AirwayID) != null)
                _ventGraph.RemoveAirway(airway.AirwayID);
        }

        public void DetachFromVentGraph()
        {
            _ventGraph = null;
        }

        new void OnDestroy()
        {
            base.OnDestroy();
                        
            AirwayBeingConstructed = false;
        }

        public void SetAirway(VentAirway newAirway)
        {
            airway = newAirway;
            AirwayBeingConstructed = false;
        }

        public VentAirway GetAirway() { return airway; }

        public void SetStartEndPoints(Transform newStart, Transform newEnd)
        {
            start = newStart;
            end = newEnd;

            endPointsSet = true;
        }
    }
}