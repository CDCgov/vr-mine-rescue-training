using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NIOSH_MineCreation
{
    public class RotatorSetAccess : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("rotateSet")]
        public TileRotateSet RotateSet;

        private int _index = -1;

        // Start is called before the first frame update
        void Start()
        {
            if(RotateSet != null)
                _index = RotateSet.DetermineIndex(gameObject);
        }

        public GameObject GetRotateToObject(bool clockwise, Transform assetContainer)
        {
            if (_index == -1)
                return gameObject;

            //Destroy(gameObject);
            MineLayerTile thisTile = GetComponent<MineLayerTile>();

            var prefab = RotateSet.GetNextInSet(_index, clockwise);
            if (prefab == null)
                return null;

            GameObject newObj = Instantiate(prefab, transform.position, transform.rotation, assetContainer);

            newObj.GetComponent<MineLayerTile>().ChangeModeToEdit(false);
            newObj.GetComponent<MineLayerTile>().ScaleToSettings();

            //if (thisTile.ReturnTile != null)
            //{
            //    newObj.GetComponent<MineLayerTile>().ReturnTile = thisTile.ReturnTile;
            //    thisTile.ReturnTile = null;
            //    Destroy(gameObject);
            //}
            //else
            //{
            //    thisTile.ReturnToLastValid();
            //    gameObject.SetActive(false);
            //    newObj.GetComponent<MineLayerTile>().ReturnTile = gameObject;
            //}
            return newObj;
        }
    }
}