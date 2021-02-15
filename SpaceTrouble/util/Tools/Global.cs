using Microsoft.Xna.Framework;

namespace SpaceTrouble.util.Tools {
    internal static class Global {
        public const string Version = "1.1";
        public static int WindowWidth { get; set; }
        public static int WindowHeight { get; set; }

        public const int WorldWidth = 70; // This NEEDS to be an even numbers!
        public const int WorldHeight = 70; // This NEEDS to be an even numbers!
        public static Vector2 mWorldOrigin = new Vector2(WorldWidth / 2f, WorldHeight / 2f);

        public const int TileWidth = 64; // width of a tile in world-units, NEEDS to be even
        public const int TileHeight = 32; // height of a tile in world-units, NEEDS to be even
    }
}