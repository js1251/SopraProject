using Microsoft.Xna.Framework;

// Created by Jakob Sailer

namespace SpaceTrouble.util.Tools {
    internal static class CoordinateManager {
        /// <summary>
        /// Calculates a length along the isometric coordinate-system from a given length in the cartesian coordinate-system.
        /// </summary>
        /// <param name="input">A length in the cartesian coordinate system</param>
        /// <returns>A length in the isometric coordinate system</returns>
        public static float CartesianToIsometricLength(float input) {
            return input * 0.5f;
        }

        /// <summary>
        /// Calculates a length in number of tiles along the isometric coordinate-system from a given length in the cartesian coordinate-system.
        /// </summary>
        /// <param name="input">A length in the cartesian coordinate system</param>
        /// <returns>A length int number of tiles in the isometric coordinate system</returns>
        public static float CartesianToIsometricTileLength(float input) {
            return CartesianToIsometricLength(input) / (Global.TileWidth * 0.5f);
        }

        /// <summary>
        /// Calculates world-coordinates of given screen-coordinates.
        /// </summary>
        /// <param name="screenCords">Screen-coordinates.</param>
        /// <returns>World-coordinates of given screen-coordinates.</returns>
        public static Vector2 ScreenToWorld(Vector2 screenCords) {
            var cameraPosition = SpaceTrouble.Camera.CameraPos;
            var cameraOffset = SpaceTrouble.Camera.CameraOffset;
            var cameraZoom = SpaceTrouble.Camera.CameraZoom;

            return (screenCords - cameraOffset) / cameraZoom + cameraPosition;
        }

        /// <summary>
        /// Calculates world-cell-coordinates of given screen-coordinates.
        /// <param name="screenCords">Screen-coordinates.</param>
        /// </summary>
        /// <returns>Cell-coordinates underneath the cursor.</returns>
        public static Vector2 ScreenToCell(Vector2 screenCords) {
            return WorldToCell(ScreenToWorld(screenCords));
        }

        /// <summary>
        /// Calculates Tile-coordinates of given screen-coordinates.
        /// </summary>
        /// /// <param name="screenCords">Screen-coordinates.</param>
        /// <returns>Tile-coordinates of given screen-coordinates</returns>
        public static Vector2 ScreenToTile(Vector2 screenCords) {
            return WorldToTile(ScreenToWorld(screenCords));
        }

        /*
        /// <summary>
        /// Calculates screen-coordinates of given world-coordinates.
        /// </summary>
        /// <param name="worldCords">World-coordinates.</param>
        /// <returns>Screen-coordinates of given world-coordinates.</returns>
        public static Vector2 WorldToScreen(Vector2 worldCords) {
            var cameraPosition = SpaceTrouble.Camera.CameraPos;
            var cameraOffset = SpaceTrouble.Camera.CameraOffset;
            var cameraZoom = SpaceTrouble.Camera.CameraZoom;

            return (worldCords - cameraPosition) * cameraZoom + cameraOffset;
        }
        */

        /// <summary>
        /// Calculates the world-cell-coordinates of given screen-coordinates.
        /// </summary>
        /// <param name="worldCords">Screen-coordinates.</param>
        /// <returns>World-cell-coordinates of the given screen-coordinates.</returns>

        private static Vector2 WorldToCell(Vector2 worldCords) {
            var cellCords = WorldToCellFraction(worldCords);
            cellCords.X = (int) cellCords.X;
            cellCords.Y = (int) cellCords.Y;
            return cellCords;
        }

        /// <summary>
        /// Calculates the cell-coordinates of given world-coordinates.
        /// </summary>
        /// <returns>Cell-coordinates cast to an Integer.</returns>
        private static Vector2 WorldToCellFraction(Vector2 worldCords, float multiplier = 1) {
            // since tiles are offset by their origin
            worldCords.X += Global.TileWidth / 2f * multiplier;

            var tileCords = new Vector2 {
                X = worldCords.X / Global.TileWidth * multiplier,
                Y = worldCords.Y / Global.TileHeight * multiplier
            };
            return tileCords;
        }

        /// <summary>
        /// Calculates Tile-coordinates at given World-coordinates.
        /// </summary>
        /// <param name="worldCords">World-coordinates.</param>
        /// <returns>Tile-coordinates at given world-coordinates.</returns>
        public static Vector2 WorldToTile(Vector2 worldCords) {
            var cellCordsFraction = WorldToCellFraction(worldCords);
            var cellCords = cellCordsFraction.ToPoint().ToVector2();

            var tileCords = new Vector2 {
                X = (int) (cellCords.Y + (cellCords.X - Global.mWorldOrigin.X)),
                Y = (int) (cellCords.Y - (cellCords.X - Global.mWorldOrigin.X))
            };

            // get offset within tile and map it to -1 to 1
            var cellOffset = new Vector2(cellCordsFraction.X % 1, cellCordsFraction.Y % 1);
            cellOffset *= 2f;
            cellOffset -= Vector2.One;

            // checking for world-coordinates inside all four corners
            if (cellOffset.X + cellOffset.Y > 1) {
                tileCords.X += 1;
            } else if (cellOffset.X + cellOffset.Y < -1) {
                tileCords.X -= 1;
            } else if (cellOffset.X - cellOffset.Y > 1) {
                tileCords.Y -= 1;
            } else if (cellOffset.X - cellOffset.Y < -1) {
                tileCords.Y += 1;
            }

            return tileCords;
        }

        /// <summary>
        /// Calculates the world-coordinates at given tile-coordinates.
        /// </summary>
        /// <param name="tileCords">Tile-coordinates.</param>
        /// <param name="precise">Set to true if you want sub-tile outputs.</param>
        /// <returns>World-coordinates of given Tile-coordinates.</returns>
        public static Vector2 TileToWorld(Vector2 tileCords, bool precise = false) {
            // cast float Vector2 to "int" Vector2
            tileCords = precise ? tileCords : tileCords.ToPoint().ToVector2();

            var worldCords = new Vector2 {
                X = (tileCords.X - tileCords.Y) * (Global.TileWidth / 2f),
                Y = (tileCords.X + tileCords.Y) * (Global.TileHeight / 2f)
            };

            // the offset to move the whole grid into positive coordinates
            var originOffset = new Vector2 {
                X = Global.mWorldOrigin.X * Global.TileWidth,
                Y = Global.TileHeight / 2f
            };

            // add the offset to the world-coordinates
            worldCords += originOffset;

            return worldCords;
        }
    }
}