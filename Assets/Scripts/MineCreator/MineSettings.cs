/*
 * Stores the user's chosen mine properties to build a mine map from.
 */

namespace NIOSH_MineCreation
{
    [System.Serializable]
    public class MineSettings
    {
        // enum to define the different tileset types that can be chosen
        public enum TileSet {Coal, Stone};

        /// <summary>
        /// The tileset that should be used to grab prefabs from whe instantiating mine tiles
        /// </summary>
        public TileSet tileSet = TileSet.Coal;

        public int rockDustLvl;

        /// <summary>
        /// The number of tunnels that lead into the mine
        /// </summary>
        public int numEntries;

        /// <summary>
        /// The number of crosscut tunnel sections deep the mine has
        /// </summary>
        public int numCrosscuts;

        /// <summary>
        /// The height from the floor to the ceiling of the tunnel in meters
        /// </summary>
        public float seamHeight;

        /// <summary>
        /// The length of the cross tunnels between intersections in meters
        /// </summary>
        public float pillarWidth;

        /// <summary>
        /// The length of the tunnels between cross sections in meters
        /// </summary>
        public float pillarLength;

        /// <summary>
        /// The width of all tunnels in the mine in meters
        /// </summary>
        public float entryWidth;

        /// <summary>
        /// The distance between roof bolt plates
        /// </summary>
        public float BoltSpacing;

        public float BoltRibOffset = 1.067f;

        public bool EnableCornerCurtains = false;

        // Basic constructor to fill the properties later
        public MineSettings() { }

        // Constructor to fill properties all at once
        public MineSettings(TileSet newTileSet, 
                            int newRockDustLvl, 
                            int newNumEntries, 
                            int newNumCrosscuts, 
                            float newSeamHeight,
                            float newPillarW,
                            float newPillarL,
                            float newEntryW,
                            float newBoltSpacing,
                            float newRibOffset,
                            bool enableCornerCurtains)
        {
            tileSet = newTileSet;
            rockDustLvl = newRockDustLvl;
            numEntries = newNumEntries;
            numCrosscuts = newNumCrosscuts;
            seamHeight = newSeamHeight;
            pillarWidth = newPillarW;
            pillarLength = newPillarL;
            entryWidth = newEntryW;
            BoltSpacing = newBoltSpacing;
            BoltRibOffset = newRibOffset;
            EnableCornerCurtains = enableCornerCurtains;
        }
    }
}
