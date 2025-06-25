/*
 * Holds the details and settings for a mine tile prefab.
 */
using UnityEngine;

namespace NIOSH_MineCreation
{
    [CreateAssetMenu(menuName = "ScriptableObject/MineTile")]
    public class MineTile : ScriptableObject
    {
        public LoadableAsset TileAsset;
        public GameObject tileObject;

        public float tileLength;
        public float tileWidth;
        public float tileSeamHeight;
        public float entryWidth;
    }
}