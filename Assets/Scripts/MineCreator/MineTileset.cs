/*
 * Hold the mine prefabs that make up the tileset of a defined type.
 */
using System.Collections.Generic;
using UnityEngine;

namespace NIOSH_MineCreation
{
    [CreateAssetMenu(menuName = "ScriptableObject/MineTileset")]
    public class MineTileset : ScriptableObject
    {
        [SerializeField]
        private MineSettings.TileSet type;

        
        public UnitsEditor UnitScale;
        
        [SerializeField]
        private List<MineTile> striaghtNSSections;
        [SerializeField]
        private List<MineTile> striaghtEWSections;

        [SerializeField]
        private List<MineTile> westTSections;
        [SerializeField]
        private List<MineTile> eastTSections;
        [SerializeField]
        private List<MineTile> northTSections;
        [SerializeField]
        private List<MineTile> southTSections;

        [SerializeField]
        private List<MineTile> fourwaySections;

        [SerializeField]
        private List<MineTile> northEndcaps;
        
        [SerializeField]
        private List<MineTile> southEndcaps;

        public float RockDustStartValue;
        public float RockDustMinValue;
        public float RockDustMaxValue;
        public int EntriesStartValue;
        public int EntriesMinValue;
        public int EntriesMaxValue;
        public int CrosscutsStartValue;
        public int CrosscutsMinValue;
        public int CrosscutsMaxValue;
        public float PillarWidthStartValue;
        public float PillarWidthMinValue;
        public float PillarWidthMaxValue;
        public float PillarLengthStartValue;
        public float PillarLengthMinValue;
        public float PillarLengthMaxValue;
        public float SeamHeightStartValue;
        public float SeamHeightMinValue;
        public float SeamHeightMaxValue;
        public float EntryWidthStartValue;
        public float EntryWidthMinValue;
        public float EntryWidthMaxValue;
        public float BoltSpacingStartValue;
        public float BoltSpacingMinValue;
        public float BoltSpacingMaxValue;
        public bool UseBolts;

        public MineSettings.TileSet GetTilesetType()
        {
            return type;
        }

        /// <summary>
        /// Returns a random NS tile from the list.
        /// </summary>
        public MineTile GetRandomStraightNSTile()
        {
            return striaghtNSSections[Random.Range(0, striaghtNSSections.Count)];
        }

        /// <summary>
        /// Returns a random EW tile from the list.
        /// </summary>
        public MineTile GetRandomStraightEWTile()
        {
            return striaghtEWSections[Random.Range(0, striaghtEWSections.Count)];
        }

        /// <summary>
        /// Returns a random East T tile from the list.
        /// </summary>
        public MineTile GetRandomEastTTile()
        {
            return eastTSections[Random.Range(0, eastTSections.Count)];
        }

        /// <summary>
        /// Returns a random West T tile from the list.
        /// </summary>
        public MineTile GetRandomWestTTile()
        {
            return westTSections[Random.Range(0, westTSections.Count)];
        }

        /// <summary>
        /// Returns a random North T tile from the list.
        /// </summary>
        public MineTile GetRandomNorthTTile()
        {
            return northTSections[Random.Range(0, northTSections.Count)];
        }

        /// <summary>
        /// Returns a random South T tile from the list.
        /// </summary>
        public MineTile GetRandomSouthTTile()
        {
            return southTSections[Random.Range(0, southTSections.Count)];
        }

        /// <summary>
        /// Returns a random North end cap tile from the list.
        /// </summary>
        public MineTile GetRandomNorthEndTile()
        {
            return northEndcaps[Random.Range(0, northEndcaps.Count)];
        }

        /// <summary>
        /// Returns a random South end cap tile from the list.
        /// </summary>
        public MineTile GetRandomSouthEndTile()
        {
            return southEndcaps[Random.Range(0, southEndcaps.Count)];
        }

        /// <summary>
        /// Get the fourway tile.
        /// </summary>
        public MineTile GetRandomFourwayTile()
        {
            return fourwaySections[Random.Range(0, fourwaySections.Count)];
        }
    }
}
