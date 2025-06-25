/*
 * Hold a collection of tilesets to be able to select from with the tileset type enum
 */
using System.Collections.Generic;
using UnityEngine;

namespace NIOSH_MineCreation
{
    [CreateAssetMenu(menuName = "ScriptableObject/MineTilesetCollection")]
    public class MineTilesetCollection : ScriptableObject
    {
        [SerializeField]
        private List<MineTileset> collection = new List<MineTileset>();

        /// <summary>
        /// Returns the first tileset in the collection with a type matching the given type.
        /// Returns null if not found in the collection.
        /// </summary>
        /// <param name="type">The type of collection to get</param>
        public MineTileset GetTileset(MineSettings.TileSet type)
        {
            for(int i = 0; i < collection.Count; i++)
            {
                if (collection[i].GetTilesetType() == type)
                    return collection[i];
            }

            // Tileset not found in collection
            Debug.LogWarning("Tileset " + type.ToString() + " not found in the collection, returning null!");
            return null;
        }
    }
}