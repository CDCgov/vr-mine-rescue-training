using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NIOSH_MineCreation
{
    [CreateAssetMenu(menuName = "ScriptableObject/TileRotateSet")]
    public class TileRotateSet : ScriptableObject
    {
        [SerializeField]
        private List<GameObject> rotateSet;

        public int DetermineIndex(GameObject query)
        {
            if (!query.TryGetComponent<ObjectInfo>(out var queryObjInfo))
                return -1;

            for(int i = 0; i < rotateSet.Count; i++)
            {
                if (!rotateSet[i].TryGetComponent<ObjectInfo>(out var objInfo))
                    continue;

                if (objInfo.DisplayName == queryObjInfo.DisplayName)
                    return i;

                //if(rotateSet[i].GetComponent<MineSegment>().SegmentGeometry == query.GetComponent<MineSegment>().SegmentGeometry)
                //{
                //    return i;
                //}
            }

            return -1;
        }

        public GameObject GetNextInSet(int index, bool clockwise)
        {
            if (clockwise)
            {
                if (index + 1 >= rotateSet.Count)
                    index = 0;
                else
                    index++;
            }
            else
            {
                if (index - 1 < 0)
                    index = rotateSet.Count - 1;
                else
                    index--;
            }

            return rotateSet[index];
        }
    }
}
