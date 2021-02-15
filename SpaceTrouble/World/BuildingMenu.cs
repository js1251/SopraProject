using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.Tools;

namespace SpaceTrouble.World {
    internal sealed class BuildingMenu {
        public GameObjectEnum? mLockedBuilding;
        private Dictionary<Vector2, GameObjectEnum> TilesToPlace { get; }
        private Vector2 DragStartedWorldPos { get; set; }

        public BuildingMenu() {
            TilesToPlace = new Dictionary<Vector2, GameObjectEnum>();
        }

        public void Update(Dictionary<ActionType, InputAction> inputs) {
            TilesToPlace.Clear();

            if (mLockedBuilding == null) {
                return;
            }

            if (mLockedBuilding == GameObjectEnum.EmptyTile) {
                TryDelete(inputs);
                return;
            }

            TryPlace(inputs);
        }

        private static List<Tile> DeleteConstruction(Vector2 startingTilePos) {
            var leftPos = new Vector2(startingTilePos.X - 1, startingTilePos.Y);
            var rightPos = new Vector2(startingTilePos.X + 1, startingTilePos.Y);
            var topPos = new Vector2(startingTilePos.X, startingTilePos.Y - 1);
            var bottomPos = new Vector2(startingTilePos.X, startingTilePos.Y + 1);

            var tilesToCheck = new Dictionary<Vector2, Tile> {
                {leftPos, WorldGameState.ObjectManager.GetTile(leftPos)},
                {rightPos, WorldGameState.ObjectManager.GetTile(rightPos)},
                {topPos, WorldGameState.ObjectManager.GetTile(topPos)},
                {bottomPos, WorldGameState.ObjectManager.GetTile(bottomPos)},
            };

            var tilesToRemove = new List<Tile>();

            foreach (var (tilePos, tile) in tilesToCheck) {
                if (!(tile is IBuildable buildable1) || buildable1.BuildingFinished) {
                    continue;
                }

                var hash = new HashSet<Tile>();
                if (DeleteConstructionHelper(tilePos, hash)) {
                    tilesToRemove.AddRange(hash);
                }
            }

            return tilesToRemove;
        }

        private static bool DeleteConstructionHelper(Vector2 tilePos, HashSet<Tile> tilesToRemove) {
            var currentTileAtPosition = WorldGameState.ObjectManager.GetTile(tilePos);

            // if the current tile has already been checked return true
            if (tilesToRemove.Contains(currentTileAtPosition)) {
                return true;
            }

            // not buildable tiles are always fine (assumes walkable tiles are always buildable too)
            if (!(currentTileAtPosition is IBuildable buildable)) {
                return true;
            }

            // if a neighbor is a built platform the construction are connected and cannot be removed!
            if (buildable.BuildingFinished && buildable is PlatformTile) {
                return false;
            }

            // neighboring tiles that aren't platforms are always fine if they aren't also connected to a built platform
            if (!(currentTileAtPosition is PlatformTile) && !NeighborIsPlatform(tilePos)) {
                tilesToRemove.Add(currentTileAtPosition);
                return true;
            }

            tilesToRemove.Add(currentTileAtPosition);

            var leftPos = new Vector2(tilePos.X - 1, tilePos.Y);
            var rightPos = new Vector2(tilePos.X + 1, tilePos.Y);
            var topPos = new Vector2(tilePos.X, tilePos.Y - 1);
            var bottomPos = new Vector2(tilePos.X, tilePos.Y + 1);

            return DeleteConstructionHelper(leftPos, tilesToRemove) &&
                   DeleteConstructionHelper(rightPos, tilesToRemove) &&
                   DeleteConstructionHelper(topPos, tilesToRemove) &&
                   DeleteConstructionHelper(bottomPos, tilesToRemove);
        }

        private static bool NeighborIsPlatform (Vector2 tilePos) {
            // naive approach.. 10 seconds thought was given
            var leftTile = WorldGameState.ObjectManager.GetTile(new Vector2(tilePos.X - 1, tilePos.Y));
            var rightPos = WorldGameState.ObjectManager.GetTile(new Vector2(tilePos.X + 1, tilePos.Y));
            var topPos = WorldGameState.ObjectManager.GetTile(new Vector2(tilePos.X, tilePos.Y - 1));
            var bottomPos = WorldGameState.ObjectManager.GetTile(new Vector2(tilePos.X, tilePos.Y + 1));

            return leftTile is PlatformTile lP && lP.BuildingFinished ||
                rightPos is PlatformTile rP && rP.BuildingFinished ||
                topPos is PlatformTile tP && tP.BuildingFinished ||
                bottomPos is PlatformTile bP && bP.BuildingFinished;
        }

        // tries to delete a building at the current cursor pos
        private static void TryDelete(Dictionary<ActionType, InputAction> inputs) {
            if (!inputs.ContainsKey(ActionType.MouseLeftClick)) {
                return;
            }

            var cursorPos = inputs[ActionType.MouseMoved].Origin;
            var tilePosition = CoordinateManager.ScreenToTile(cursorPos);
            var currentTileAtPosition = WorldGameState.ObjectManager.GetTile(tilePosition);

            // fully built constructions cannot be removed
            if (currentTileAtPosition is IBuildable buildable && buildable.BuildingFinished) {
                return;
            }

            // the selected construction should always be removed
            DeleteTile(currentTileAtPosition);

            // "floating" constructions can also be removed
            foreach (var tile in DeleteConstruction(tilePosition)) {
                DeleteTile(tile);
            }
        }

        private static void DeleteTile(Tile tile) {
            var tilePos = CoordinateManager.WorldToTile(tile.WorldPosition);
            if (tile is ExtractorTile) {
                WorldGameState.ObjectManager.CreateTile(tilePos, GameObjectEnum.MassTile);
            } else if (tile is PortalTile) {
                WorldGameState.ObjectManager.CreateTile(tilePos, GameObjectEnum.PortalTile);
            } else {
                if (tile is TowerTile) {
                    WorldGameState.DifficultyManager.TowerCount--;
                }
                WorldGameState.ObjectManager.CreateTile(tilePos, GameObjectEnum.EmptyTile);
            }
        }

        private void TryPlace(Dictionary<ActionType, InputAction> inputs) {
            if (!inputs.TryGetValue(ActionType.MouseMoved, out var input)) {
                return;
            }

            // get some basic inputs
            var cursorPos = input.Origin;
            var dragStopped = false;
            if (inputs.ContainsKey(ActionType.MouseDragStart)) {
                DragStartedWorldPos = CoordinateManager.ScreenToWorld(cursorPos);
            } else if (inputs.ContainsKey(ActionType.MouseDragStop)) {
                dragStopped = true;
            } else if (!inputs.ContainsKey(ActionType.MouseDrag)) {
                DragStartedWorldPos = CoordinateManager.ScreenToWorld(cursorPos);
            }

            CreateGhostLine(cursorPos);

            if (dragStopped) {
                PlaceTiles();
            }
        }

        // creates a preview of the building selected at the position of the cursor
        private bool CreateGhostPreview(Vector2 tilePos) {
            if (mLockedBuilding == null) {
                return false;
            }

            var valid = CheckBuildingCondition(mLockedBuilding, tilePos);
            var color = valid ? Color.Green : Color.Red;
            WorldGameState.ObjectManager.CreateGhostTile(tilePos, (GameObjectEnum) mLockedBuilding, color * 0.5f);
            return valid;
        }

        // creates a line of previews from the start to the end of a drag
        private void CreateGhostLine(Vector2 cursorPos) {
            const float factor = 8 / (float) Global.TileHeight; // can be used to tweak accuracy
            var cursorWorldPos = CoordinateManager.ScreenToWorld(cursorPos);
            var distanceFromCursor = (int) (Vector2.Distance(DragStartedWorldPos, cursorWorldPos) * factor);

            for (var i = 0; i <= distanceFromCursor; i++) {
                var worldPos = DragStartedWorldPos;

                // don't divide by 0
                worldPos = distanceFromCursor > 0 ? worldPos + (cursorWorldPos - DragStartedWorldPos) / distanceFromCursor * i : worldPos;
                var tilePos = CoordinateManager.WorldToTile(worldPos);

                var blocked = !CreateGhostPreview(tilePos);
                if (blocked) {
                    return;
                }

                if (mLockedBuilding != null) {
                    TilesToPlace.TryAdd(tilePos, (GameObjectEnum) mLockedBuilding);
                }
            }
        }

        // determines whether or not a building can be placed at a given location
        private static bool CheckBuildingCondition(GameObjectEnum? type, Vector2 tilePos) {
            // checks if it is beside a platform
            if (tilePos.X < 0 || tilePos.X >= Global.WorldWidth || tilePos.Y < 0 || tilePos.Y >= Global.WorldWidth) {
                return false;
            }

            var leftTile = tilePos.X > 0 ? WorldGameState.ObjectManager.GetTile(new Vector2(tilePos.X - 1, tilePos.Y), true) : null;
            var rightTile = tilePos.X <= Global.WorldWidth ? WorldGameState.ObjectManager.GetTile(new Vector2(tilePos.X + 1, tilePos.Y), true) : null;
            var upperTile = tilePos.Y > 0 ? WorldGameState.ObjectManager.GetTile(new Vector2(tilePos.X, tilePos.Y - 1), true) : null;
            var lowerTile = tilePos.Y <= Global.WorldHeight ? WorldGameState.ObjectManager.GetTile(new Vector2(tilePos.X, tilePos.Y + 1), true) : null;

            // note: a StockTile is also a PlatformTile
            if (!(rightTile is PlatformTile) && !(leftTile is PlatformTile) && !(lowerTile is PlatformTile) && !(upperTile is PlatformTile)) {
                return false;
            }

            // the tile it the checked location
            var tileAtPos = WorldGameState.ObjectManager.GetTile(tilePos);

            // extractor tiles can be placed on MassTiles only
            if (type == GameObjectEnum.ExtractorTile) {
                return tileAtPos is MassTile;
            }

            // laboratory tiles can be placed on PortalTiles only
            if (type == GameObjectEnum.LaboratoryTile) {
                return tileAtPos is PortalTile;
            }

            // any other tile can only be placed on EmptyTiles
            return tileAtPos is EmptyTile;
        }

        private void PlaceTiles() {
            if (TilesToPlace.Count > 0) {
                SpaceTrouble.SoundManager.PlaySound(Sound.Tile);
            }

            foreach (var (tilePos, type) in TilesToPlace) {
                // if a player manages to place a laboratory on a active portal the spawn-information needs to be transferred
                if (WorldGameState.ObjectManager.GetTile(tilePos) is PortalTile portal) {
                    HandlePortalEdgeCase(portal, tilePos, type);
                } else {
                    WorldGameState.ObjectManager.CreateTile(tilePos, type);
                }
            }

            TilesToPlace.Clear();
        }

        private static void HandlePortalEdgeCase(PortalTile portal, Vector2 tilePos, GameObjectEnum type) {
            var spawnCount = portal.MaxSpawnNumber;
            var spawnList = portal.CreaturesSpawned;
            var newTile = WorldGameState.ObjectManager.CreateTile(tilePos, type);

            if (newTile is LaboratoryTile laboratory) {
                laboratory.MaxSpawnNumber = spawnCount;
                laboratory.CreaturesSpawned = spawnList;
                foreach (var creature in spawnList) {
                    creature.SpawnOrigin = laboratory;
                }
            } else {
                System.Diagnostics.Debug.WriteLine("A portal was replaced from the building menu by a tile other than a laboratory");
            }
        }
    }
}