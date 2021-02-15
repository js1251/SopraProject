using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.Tools;

// Created by Jakob Sailer

namespace SpaceTrouble.World {
    internal sealed class PriorityManager {
        [JsonIgnore] public Dictionary<ObjectProperty, HashSet<Tile>> PrioritizedTiles { get; }
        [JsonIgnore] private ObjectManager ObjectManager { get; }
        [JsonProperty] private Dictionary<ObjectProperty, HashSet<Vector2>> PositionToTileMapping { get; set; }

        public PriorityManager() {
            PrioritizedTiles = new Dictionary<ObjectProperty, HashSet<Tile>> {
                {ObjectProperty.UnderConstruction, new HashSet<Tile>()},
                {ObjectProperty.RequiresAmmunition, new HashSet<Tile>()},
                {ObjectProperty.RequiresSpawnResources, new HashSet<Tile>()}
            };

            PositionToTileMapping = new Dictionary<ObjectProperty, HashSet<Vector2>> {
                {ObjectProperty.UnderConstruction, new HashSet<Vector2>()},
                {ObjectProperty.RequiresAmmunition, new HashSet<Vector2>()},
                {ObjectProperty.RequiresSpawnResources, new HashSet<Vector2>()}
            };

            ObjectManager = WorldGameState.ObjectManager;
        }

        internal void CreateSaveLoadMapping() {
            foreach (var (type, hash) in PrioritizedTiles) {
                foreach (var tile in hash) {
                    PositionToTileMapping[type].Add(tile.WorldPosition);
                }
            }
        }

        internal void LoadSavedMapping() {
            foreach (var (type, hash) in PositionToTileMapping) {
                foreach (var pos in hash) {
                    PrioritizedTiles[type].Add(ObjectManager.GetTile(CoordinateManager.WorldToTile(pos)));
                }
            }
        }

        internal void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            UpdateAlpha(gameTime);
            UpdateSelection(inputs);
        }

        private void UpdateSelection(Dictionary<ActionType, InputAction> inputs) {
            if (WorldGameState.BuildingMenu.mLockedBuilding != null) {
                return;
            }

            if (inputs.TryGetValue(ActionType.MouseLeftClick, out var input)) {
                var cursorPos = input.Origin;
                var tileAtPos = WorldGameState.ObjectManager.GetTile(CoordinateManager.ScreenToTile(cursorPos));

                if (tileAtPos is IBuildable buildable) {
                    if (!buildable.BuildingFinished) {
                        TogglePriority(ObjectProperty.UnderConstruction, tileAtPos);
                    } else if (tileAtPos is TowerTile) {
                        TogglePriority(ObjectProperty.RequiresAmmunition, tileAtPos);
                    } else if (tileAtPos is BarrackTile) {
                        TogglePriority(ObjectProperty.RequiresSpawnResources, tileAtPos);
                    }
                }
            }
        }

        private void UpdateAlpha(GameTime gameTime) {
            var alphaChangeAmount = MathExtension.Oscillation((float) gameTime.TotalGameTime.TotalSeconds, 3f,0.5f,1f);
            foreach (var (property, hash) in PrioritizedTiles) {
                foreach (var tile in hash.ToList()) {
                    if (TileExists(tile)) {
                        tile.PriorityAlpha = alphaChangeAmount;
                    } else {
                        PrioritizedTiles[property].Remove(tile);
                    }
                }
            }
        }

        private bool TileExists(Tile tile) {
            return ObjectManager.GetTile(CoordinateManager.WorldToTile(tile.WorldPosition)).Equals(tile);
        }

        private void TogglePriority(ObjectProperty property, Tile tile) {
            // if a construction should be prioritized mark all platforms leading up to that tile as prioritized too
            if (property == ObjectProperty.UnderConstruction) {
                var connectedTiles = BfsSearch(tile);
                if (PrioritizedTiles[ObjectProperty.UnderConstruction].Contains(tile)) {
                    foreach (var neighbor in connectedTiles) {
                        neighbor.HasPriority = false;
                        PrioritizedTiles[ObjectProperty.UnderConstruction].Remove(neighbor);
                    }
                } else {
                    foreach (var neighbor in connectedTiles) {
                        neighbor.HasPriority = true;
                        PrioritizedTiles[ObjectProperty.UnderConstruction].Add(neighbor);
                    }
                }

                return;
            }

            // otherwise just toggle the priority of the selected tile
            if (PrioritizedTiles[property].Contains(tile)) {
                tile.HasPriority = false;
                PrioritizedTiles[property].Remove(tile);
            } else {
                tile.HasPriority = true;
                PrioritizedTiles[property].Add(tile);
            }
        }


        private List<Tile> BfsSearch(Tile start) {
            var visited = new Dictionary<Tile, Tile> {
                {start, null} // source, parent
            };

            var queue = new Queue<Tile>();
            queue.Enqueue(start);

            Tile nearestPlatform = null;

            while (queue.Count > 0) {
                var currentTile = queue.Dequeue();

                if (currentTile is IBuildable buildable && buildable.BuildingFinished) {
                    nearestPlatform = currentTile;
                    break;
                }

                foreach (var neighbor in GetNeighbors(currentTile, visited)) {
                    visited.Add(neighbor, currentTile);
                    queue.Enqueue(neighbor);
                }
            }

            var priorityTiles = new List<Tile>();
            if (nearestPlatform != null) {
                var parent = visited[nearestPlatform];
                while (parent != null) {
                    priorityTiles.Add(parent);
                    parent = visited[parent];
                }
            }

            return priorityTiles;
        }

        private List<Tile> GetNeighbors(Tile tile, Dictionary<Tile, Tile> visited) {
            var tilePos = CoordinateManager.WorldToTile(tile.WorldPosition);
            var potentialNeighbors = new List<Tile> {
                ObjectManager.GetTile(tilePos + Vector2.UnitX),
                ObjectManager.GetTile(tilePos - Vector2.UnitX),
                ObjectManager.GetTile(tilePos + Vector2.UnitY),
                ObjectManager.GetTile(tilePos - Vector2.UnitY)
            };

            var trueNeighbors = new List<Tile>();
            foreach (var neighbor in potentialNeighbors) {
                if (neighbor == null || visited.ContainsKey(neighbor)) {
                    continue;
                }

                if (!(neighbor is PlatformTile)) {
                    continue;
                }

                trueNeighbors.Add(neighbor);
            }

            return trueNeighbors;
        }
    }
}