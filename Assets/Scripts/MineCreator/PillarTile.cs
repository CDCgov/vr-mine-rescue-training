using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NIOSH_MineCreation
{
    public class PillarTile : MonoBehaviour
    {
        [SerializeField]
        private bool isNSTile = false;
        [SerializeField]
        private MineTile tileDetails;

        private ComponentInfo_MineSegment _mineSegmentComponent;

        private void Awake()
        {
            _mineSegmentComponent = GetComponent<ComponentInfo_MineSegment>();
        }
        
        public void ScalePillarTile(MineSettings settings)
        {            
            Vector3 pillarScaler;
            if (isNSTile)
            {
                pillarScaler = new Vector3(settings.entryWidth / tileDetails.tileWidth,
                                               settings.seamHeight / tileDetails.tileSeamHeight,
                                               (settings.pillarLength - settings.entryWidth) / tileDetails.tileLength);
                if(pillarScaler.z < 1 && _mineSegmentComponent != null)
                {                    
                    _mineSegmentComponent.IsTeamstop = false;
                }
            }
            else
            {
                pillarScaler = new Vector3((settings.pillarWidth - settings.entryWidth) / tileDetails.tileWidth,
                                               settings.seamHeight / tileDetails.tileSeamHeight,
                                               settings.entryWidth / tileDetails.tileLength);
                if (pillarScaler.x < 1 && _mineSegmentComponent != null)
                {                    
                    _mineSegmentComponent.IsTeamstop = false;
                }
            }
            Debug.Log($"Pillar Scale: {pillarScaler}");
            transform.localScale = Vector3.Scale(transform.localScale, pillarScaler);
        }

        public void AllowAutoTeamstop()
        {
            Debug.Log("In AllowAutoTeamstop");
            if (_mineSegmentComponent == null)
            {
                return;
            }
            if (isNSTile)
            {
                Debug.Log($"Scale value: {transform.localScale.z}");
                if (transform.localScale.z < 1)
                {
                    _mineSegmentComponent.IsTeamstop = false;
                }
            }
            else
            {
                Debug.Log($"Scale value: {transform.localScale.x}");
                if (transform.localScale.x < 1)
                {
                    _mineSegmentComponent.IsTeamstop = false;
                }
            }
        }
    }
}