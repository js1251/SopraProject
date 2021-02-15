using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects;
using SpaceTrouble.GameObjects.Projectiles;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.Statistics;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;

namespace SpaceTrouble.util.DataStructures.GameObjectStructure {
    internal sealed class ObjectDataStructure {
        private GameDataStructure ParentStructure { get; } // the overarching data-structure for this game
        private List<GameObject> AllNotTiles { get; } // a list of ALL objects that aren't tiles
        public List<GameObject>[,] ObjectsOnTiles { get; private set; } // a array of lists containing all objects currently on-top of tiles
        private Tile[,] Tiles { get; } // a 2D array for the tile-world
        private Dictionary<ObjectProperty, Dictionary<GameObjectEnum, List<GameObject>>> InterFacePropertyDictionary { get; }
        private Dictionary<ObjectProperty, Dictionary<GameObjectEnum, List<GameObject>>> AdditionalPropertyDictionary { get; }

        public ObjectDataStructure(GameDataStructure parentStructure) {
            ParentStructure = parentStructure;
            AllNotTiles = new List<GameObject>();
            Tiles = new Tile[Global.WorldWidth, Global.WorldHeight];
            ObjectsOnTiles = new List<GameObject>[Global.WorldWidth, Global.WorldHeight];

            // Any Interface will receive a Property entry. New Interfaces will have to be added to this list!
            InterFacePropertyDictionary = new Dictionary<ObjectProperty, Dictionary<GameObjectEnum, List<GameObject>>> {
                {ObjectProperty.Buildable, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.Moving, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.Collidable, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.BoundingBox, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.ResourceContainer, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.ResourceGenerator, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.Enemy, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.Friendly, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.Animating, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.CanHavePriority, new Dictionary<GameObjectEnum, List<GameObject>>()}
            };

            // Anything beyond interfaces that you wish to be a property need to be inserted here
            AdditionalPropertyDictionary = new Dictionary<ObjectProperty, Dictionary<GameObjectEnum, List<GameObject>>> {
                {ObjectProperty.UnderConstruction, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.RequiresSpawnResources, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.RequiresAmmunition, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.IsRealProjectile, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.Walkable, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.Enterable, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.Other, new Dictionary<GameObjectEnum, List<GameObject>>()},
                {ObjectProperty.Flying, new Dictionary<GameObjectEnum, List<GameObject>>()}
            };
        }

        public void Update(GameTime gameTime) {
            UpdateBuildingStatus();

            ObjectsOnTiles = new List<GameObject>[Global.WorldWidth, Global.WorldHeight];
            UpdateObjectTileRelation(ObjectProperty.Moving);
            UpdateObjectTileRelation(ObjectProperty.IsRealProjectile);
            UpdateObjectTileRelation(GameObjectEnum.FakeProjectile);
            UpdateObjectTileRelation(GameObjectEnum.ProjectileExplosion);

            UpdateTiles(gameTime); // always update Tiles first!
            UpdateRest(gameTime);
        }

        private void UpdateObjectTileRelation(ObjectProperty property) {
            UpdateObjectTileRelationHelper(WorldGameState.ObjectManager.GetAllObjects(property));
        }

        private void UpdateObjectTileRelation(GameObjectEnum type) {
            UpdateObjectTileRelationHelper(WorldGameState.ObjectManager.GetAllObjects(type));
        }

        private void UpdateObjectTileRelationHelper(List<GameObject> gameObjectsToCheck) {
            foreach (var gameObject in gameObjectsToCheck) {
                if (!gameObject.IsVisible) {
                    continue;
                }

                var tilePos = CoordinateManager.WorldToTile(gameObject.WorldPosition);

                if (!TileOutOfBounds(tilePos)) {
                    if (gameObject is IBoundingBox boundingBoxObj) {
                        var boundingBox = boundingBoxObj.GetBoundingBox();
                        var bottomLeftBound = new Vector2(boundingBox.Left, boundingBox.Bottom);
                        var bottomRightBound = new Vector2(boundingBox.Right, boundingBox.Bottom);

                        var bottomLeftBoundsTilePos = CoordinateManager.WorldToTile(bottomLeftBound);
                        var bottomRightBoundsTilePos = CoordinateManager.WorldToTile(bottomRightBound);

                        if (!TileOutOfBounds(tilePos + Vector2.UnitX) && (bottomLeftBoundsTilePos.X > tilePos.X || bottomRightBoundsTilePos.X > tilePos.X)) {
                            tilePos.X++;
                        }

                        if (!TileOutOfBounds(tilePos + Vector2.UnitY) && (bottomLeftBoundsTilePos.Y > tilePos.Y || bottomRightBoundsTilePos.Y > tilePos.Y)) {
                            tilePos.Y++;
                        }
                    }

                    ObjectsOnTiles[(int) tilePos.X, (int) tilePos.Y] ??= new List<GameObject>();
                    ObjectsOnTiles[(int) tilePos.X, (int) tilePos.Y].Add(gameObject);
                }
            }
        }

        private void UpdateTiles(GameTime gameTime) {
            foreach (var tile in Tiles) {
                tile.Update(gameTime);
            }
        }

        private void UpdateRest(GameTime gameTime) {
            // .ToList() since some objects might get removed on update
            foreach (var gameObject in AllNotTiles.ToList()) {
                gameObject.Update(gameTime);
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch, DebugMode mode) {
            foreach (var gameObject in AllNotTiles) {
                gameObject.DrawDebug(spriteBatch, mode);
            }
        }

        public void Insert(GameObject gameObject) {
            if (gameObject is Tile) {
                InsertOrRemoveTile(gameObject, false);
            } else {
                InsertOrRemoveAll(gameObject, false);
            }

            InsertOrRemoveInterfaceDictionary(gameObject, false);
            InsertOrRemoveAdditionalDictionary(gameObject, false);
            InsertOrRemoveOther(gameObject, false); // TODO: only do if nothing else matches
        }

        public void Remove(GameObject gameObject) {
            InsertOrRemoveAll(gameObject, true);
            InsertOrRemoveTile(gameObject, true);
            InsertOrRemoveInterfaceDictionary(gameObject, true);
            InsertOrRemoveAdditionalDictionary(gameObject, true);

            InsertOrRemoveOther(gameObject, true); // TODO: only do if nothing else matches
        }

        public Tile GetTile(Vector2 tilePos) {
            return !TileOutOfBounds(tilePos) ? Tiles[(int) tilePos.X, (int) tilePos.Y] : null;
        }

        public Tile[,] GetAllTiles() {
            return Tiles;
        }

        private static bool TileOutOfBounds(Vector2 tilePos) {
            return (int) tilePos.X < 0 || (int) tilePos.X >= Global.WorldWidth || (int) tilePos.Y < 0 || (int) tilePos.Y >= Global.WorldHeight;
        }

        // TODO: clean up
        public List<GameObject> GetAllObjects(GameObjectEnum type) {
            foreach (var dictionary in InterFacePropertyDictionary.Values) {
                if (dictionary.ContainsKey(type)) {
                    return dictionary[type];
                }
            }

            foreach (var dictionary in AdditionalPropertyDictionary.Values) {
                if (dictionary.ContainsKey(type)) {
                    return dictionary[type];
                }
            }

            return new List<GameObject>();
        }

        public List<GameObject> GetAllObjects(ObjectProperty? property = null) {
            if (property == null) {
                var allObjects = new List<GameObject>(AllNotTiles);
                allObjects.AddRange(Tiles.Cast<Tile>());
                return allObjects;
            }

            var objectList = new List<GameObject>();
            if (InterFacePropertyDictionary.ContainsKey((ObjectProperty) property)) {
                foreach (var typeList in InterFacePropertyDictionary[(ObjectProperty) property].Values) {
                    objectList.AddRange(typeList);
                }
            } else if (AdditionalPropertyDictionary.ContainsKey((ObjectProperty) property)) {
                foreach (var typeList in AdditionalPropertyDictionary[(ObjectProperty) property].Values) {
                    objectList.AddRange(typeList);
                }
            }

            return objectList;
        }

        private void InsertOrRemoveAll(GameObject gameObject, bool remove) {
            if (remove) {
                AllNotTiles.Remove(gameObject);
            } else {
                AllNotTiles.Add(gameObject);
            }
        }

        private void InsertOrRemoveTile(GameObject gameObject, bool remove) {
            if (gameObject is Tile tile) {
                var tilePos = CoordinateManager.WorldToTile(tile.WorldPosition);
                if (!TileOutOfBounds(tilePos)) {
                    if (remove) {
                        // TODO: do we want to remove tiles all-together or just override them as we do atm?
                    } else {
                        var oldTile = Tiles[(int) tilePos.X, (int) tilePos.Y];
                        if (oldTile != null) {
                            ParentStructure.Remove(oldTile);
                        }

                        Tiles[(int) tilePos.X, (int) tilePos.Y] = tile;
                    }

                    return;
                }

                System.Diagnostics.Debug.WriteLine("Cannot remove/ create " + gameObject + " at: " + tilePos + " ! Out of Bounds for: " + new Vector2(Global.WorldWidth - 1, Global.WorldHeight - 1));
            }
        }

        private void InsertOrRemoveInterfaceDictionary(GameObject gameObject, bool remove) {
            foreach (var implementingInterface in gameObject.GetType().GetInterfaces()) {
                var interfaceName = implementingInterface.Name;
                interfaceName = interfaceName.Substring(interfaceName.LastIndexOf(".", StringComparison.Ordinal) + 2);
                var property = (ObjectProperty) Enum.Parse(typeof(ObjectProperty), interfaceName);
                ChangeDictionary(InterFacePropertyDictionary, gameObject, property, remove);
            }
        }

        private void InsertOrRemoveAdditionalDictionary(GameObject gameObject, bool remove) {
            if (gameObject is RealProjectile) {
                ChangeDictionary(AdditionalPropertyDictionary, gameObject, ObjectProperty.IsRealProjectile, remove);
                return;
            }

            if (gameObject.DrawFlying) {
                ChangeDictionary(AdditionalPropertyDictionary, gameObject, ObjectProperty.Flying, remove);
            }

            var shouldUpdateNavMesh = false;
            if (gameObject is Tile tile) {
                if (tile.IsWalkable) {
                    ChangeDictionary(AdditionalPropertyDictionary, gameObject, ObjectProperty.Walkable, remove);
                    shouldUpdateNavMesh = true;
                }

                if (tile.IsEnterable) {
                    ChangeDictionary(AdditionalPropertyDictionary, gameObject, ObjectProperty.Enterable, remove);
                    shouldUpdateNavMesh = true;
                }

                if (tile is TowerTile) {
                    WorldGameState.DifficultyManager.TowerCount++;
                }

                if (tile is IBuildable buildable) {
                    if (!buildable.BuildingFinished) {
                        ChangeDictionary(AdditionalPropertyDictionary, gameObject, ObjectProperty.UnderConstruction, remove);
                    } else if (buildable is BarrackTile) {
                        ChangeDictionary(AdditionalPropertyDictionary, gameObject, ObjectProperty.RequiresSpawnResources, remove);
                    } else if (buildable is TowerTile) {
                        ChangeDictionary(AdditionalPropertyDictionary, gameObject, ObjectProperty.RequiresAmmunition, remove);
                    }
                }
            }

            if (shouldUpdateNavMesh) {
                WorldGameState.NavigationManager.UpdateNavMesh();
            }
        }

        private void InsertOrRemoveOther(GameObject gameObject, bool remove) {
            ChangeDictionary(AdditionalPropertyDictionary, gameObject, ObjectProperty.Other, remove);
        }

        private static void ChangeDictionary(Dictionary<ObjectProperty, Dictionary<GameObjectEnum, List<GameObject>>> dictionary, GameObject gameObject, ObjectProperty property, bool remove) {
            var type = gameObject.Type;

            // HACK! TODO: Do we ever want to specifically get LaboratoryTiles only?
            if (type.Equals(GameObjectEnum.LaboratoryTile)) {
                type = GameObjectEnum.PortalTile;
            }

            if (dictionary.ContainsKey(property)) {
                if (dictionary[property].ContainsKey(type)) {
                    if (remove) {
                        dictionary[property][type].Remove(gameObject);
                        return;
                    }

                    dictionary[property][type].Add(gameObject);
                    return;
                }

                if (!remove) {
                    dictionary[property].Add(type, new List<GameObject> {gameObject});
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Trying to remove \"" + type + "\": Object-type has no entry in this Dictionary under \"" + property + "\"!");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Property \"" + property + "\" is not part of this Dictionary! Create a new Enum and Dictionary entry in \"ObjectDataStructure.cs\"");
        }

        private void UpdateBuildingStatus() {
            // check to see if any buildings have updated their status
            var shouldUpdateNavMesh = false;
            foreach (var type in InterFacePropertyDictionary[ObjectProperty.Buildable].Keys) {
                foreach (var gameObject in InterFacePropertyDictionary[ObjectProperty.Buildable][type]) {
                    if (gameObject is Tile tile && tile is IBuildable buildable) {
                        if (buildable.BuildingFinished && buildable.HasChanged) {
                            SpaceTrouble.StatsManager.AddValue(Statistic.BuildingsConstructed, 1);

                            if (!(buildable is PlatformTile)) {
                                SpaceTrouble.SoundManager.PlaySound(Sound.BuildFinished);
                            }

                            if (buildable is LaboratoryTile) {
                                SpaceTrouble.StatsManager.AddValue(Statistic.LaboratoriesBuilt, 1);
                            }

                            ChangeDictionary(AdditionalPropertyDictionary, tile, ObjectProperty.UnderConstruction, true);
                            if (tile.IsWalkable) {
                                ChangeDictionary(AdditionalPropertyDictionary, tile, ObjectProperty.Walkable, false);
                                shouldUpdateNavMesh = true;
                            }

                            if (tile.IsEnterable) {
                                ChangeDictionary(AdditionalPropertyDictionary, tile, ObjectProperty.Enterable, false);
                                shouldUpdateNavMesh = true;
                            } else {
                                ChangeDictionary(AdditionalPropertyDictionary, tile, ObjectProperty.Enterable, true);
                            }


                            if (!buildable.RequiredResources.IsEmpty()) {
                                ChangeDictionary(AdditionalPropertyDictionary, tile, ObjectProperty.RequiresSpawnResources, false);
                            }

                            // if now build is a tower create a vector field around it for enemies to avoid
                            if (tile is TowerTile tower) {
                                ParentStructure.CollisionData.UpdateTowerReach(tower);
                                WorldGameState.NavigationManager.UpdateAirBiasFieldFromTile(tower, tower.AttackRadius, 0.3f);
                                ChangeDictionary(AdditionalPropertyDictionary, tile, ObjectProperty.RequiresAmmunition, false);
                            } else if (tile is ExtractorTile) {
                                WorldGameState.NavigationManager.UpdateAirBiasFieldFromTile(tile, 3, 0.5f);
                            }
                        }

                        buildable.HasChanged = false;
                    }
                }
            }

            if (shouldUpdateNavMesh) {
                WorldGameState.NavigationManager.UpdateNavMesh();
            }
        }
    }
}