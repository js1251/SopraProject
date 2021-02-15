using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using SpaceTrouble.GameObjects;
using SpaceTrouble.GameObjects.Creatures;
using SpaceTrouble.GameObjects.Projectiles;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;
using Point = Microsoft.Xna.Framework.Point;

// Created by Jakob Sailer

namespace SpaceTrouble.util.DataStructures.GameObjectStructure {
    internal sealed class CollisionDataStructure {
        private GameDataStructure ParentStructure { get; } // the overarching data-structure for this game
        private List<TowerTile>[,] TowerReach { get; }
        private int MaxTowersShootingAtSameTarget { get; }
        private float TowerReachResolution { get; }

        public CollisionDataStructure(GameDataStructure parentStructure) {
            ParentStructure = parentStructure;

            TowerReachResolution = 2; // number of sections to cut tiles into
            var towerReachSizeX = Global.WorldWidth * TowerReachResolution;
            var towerReachSizeY = Global.WorldHeight * TowerReachResolution;
            TowerReach = new List<TowerTile>[(int) towerReachSizeX, (int) towerReachSizeY];
            MaxTowersShootingAtSameTarget = 5;

            InitializeTowerReach();
        }

        private void InitializeTowerReach() {
            for (var x = 0; x < TowerReach.GetLength(0); x++) {
                for (var y = 0; y < TowerReach.GetLength(1); y++) {
                    TowerReach[x, y] = new List<TowerTile>();
                }
            }
        }

        public void Update() {
            CheckAllCollisions(ObjectProperty.Collidable);
            CheckAllCollisions(ObjectProperty.IsRealProjectile);

            // check TowerReach
            CheckTowerReach();
        }

        public void UpdateTowerReach(Tile tile) {
            if (tile is TowerTile tower) {
                foreach (var cell in GetTowerCells(tower).Item2) {
                    cell.Add(tower);
                }
            }
        }

        private (List<Point>, List<List<TowerTile>>) GetTowerCells(TowerTile tower) {
            var cellsTowerIsIn = new List<Point>();
            var listsTowerIsIn = new List<List<TowerTile>>();

            var towerRadius = (int) (CoordinateManager.CartesianToIsometricTileLength(tower.AttackRadius) * TowerReachResolution); // radius in tiles * resolution
            var towerPosition = CoordinateManager.WorldToTile(tower.WorldPosition) * TowerReachResolution; // position in the TowerReachArray
            for (var x = -towerRadius; x <= towerRadius; x++) {
                for (var y = -towerRadius; y <= towerRadius; y++) {
                    var cell = towerPosition + new Vector2(x, y);
                    if (IsOutOfTowerReachArray((int) cell.X, (int) cell.Y)) {
                        continue;
                    }

                    if (Vector2.Distance(cell, towerPosition) <= towerRadius) {
                        cellsTowerIsIn.Add(CoordinateManager.TileToWorld((towerPosition + new Vector2(x, y)) / TowerReachResolution, true).ToPoint());
                        listsTowerIsIn.Add(TowerReach[(int) cell.X, (int) cell.Y]);
                    }
                }
            }

            return (cellsTowerIsIn, listsTowerIsIn);
        }

        private bool IsOutOfTowerReachArray(int x, int y) {
            return x < 0 || x >= TowerReach.GetLength(0) || y < 0 || y >= TowerReach.GetLength(1);
        }

        private bool IsOutOfTileArray(int x, int y) {
            var tileArray = ParentStructure.ObjectData.GetAllTiles();
            return x < 0 || x >= tileArray.GetLength(0) || y < 0 || y >= tileArray.GetLength(1);
        }

        private void CheckTowerReach() {
            foreach (var gameObject in WorldGameState.ObjectManager.GetAllObjects(ObjectProperty.Enemy)) {
                var enemyTowerReachPosition = (CoordinateManager.WorldToTile(gameObject.WorldPosition) * TowerReachResolution).ToPoint();
                if (IsOutOfTowerReachArray(enemyTowerReachPosition.X, enemyTowerReachPosition.Y)) {
                    continue;
                }

                var towerReachEntry = TowerReach[enemyTowerReachPosition.X, enemyTowerReachPosition.Y];

                if (!(gameObject is Creature creature)) {
                    continue;
                }

                // tower that are out of ammo should not be considered
                foreach (var tower in towerReachEntry.ToList()) {
                    if (!tower.TowerHasAmmo) {
                        towerReachEntry.Remove(tower);
                    }
                }

                var towerCount = Math.Clamp(towerReachEntry.Count, 0, MaxTowersShootingAtSameTarget);
                for (var i = 0; i < towerCount; i++) {
                    towerReachEntry[i].SetTarget(creature);
                }
            }
        }

        public List<GameObject> GetNearObjects(Vector2 worldPos, float radius) {
            var tilePos = CoordinateManager.WorldToTile(worldPos).ToPoint();
            var nearObjects = new List<GameObject>();

            var tileRadius = (int) Math.Round(radius / Global.TileWidth);

            for (var x = tilePos.X - tileRadius; x <= tilePos.X + tileRadius; x++) {
                for (var y = tilePos.Y - tileRadius; y <= tilePos.Y + tileRadius; y++) {
                    if (IsOutOfTileArray(x, y)) {
                        continue;
                    }

                    var objectsOnTile = ParentStructure.ObjectData.ObjectsOnTiles[x, y];
                    if (objectsOnTile != null) {
                        nearObjects.AddRange(objectsOnTile);
                    }
                }
            }

            return nearObjects;
        }

        private List<GameObject> GetNearCollidingObjects(RectangleF bBox) {
            var collidingObjects = new List<GameObject>();

            var vertices = new[] {
                CoordinateManager.WorldToTile(new Vector2(bBox.Right - bBox.Left / 2f, bBox.Bottom - bBox.Top / 2f)).ToPoint(),
                CoordinateManager.WorldToTile(new Vector2(bBox.Left, bBox.Top)).ToPoint(),
                CoordinateManager.WorldToTile(new Vector2(bBox.Right, bBox.Top)).ToPoint(),
                CoordinateManager.WorldToTile(new Vector2(bBox.Right, bBox.Bottom)).ToPoint(),
                CoordinateManager.WorldToTile(new Vector2(bBox.Left, bBox.Bottom)).ToPoint()
            };

            foreach (var vertex in vertices) {
                if (vertex.Equals(vertices[0])) {
                    continue;
                }

                if (IsOutOfTileArray(vertex.X, vertex.Y)) {
                    continue;
                }

                var objectsOnTile = ParentStructure.ObjectData.ObjectsOnTiles[vertex.X, vertex.Y];

                if (objectsOnTile == null) {
                    continue;
                }

                foreach (var gameObject in objectsOnTile) {
                    if (gameObject is IBoundingBox boundingBoxObject) {
                        if (bBox.IntersectsWith(boundingBoxObject.BoundingBox)) {
                            collidingObjects.Add(gameObject);
                        }
                    }
                }
            }

            return collidingObjects;
        }

        /// <summary>
        /// Checks for all objects of a given property if that object is colliding with something else
        /// </summary>
        private void CheckAllCollisions(ObjectProperty property) {
            /* Utilizes dynamic programming. This will cut complexity by a factor of 2
             *
             * Imagine the following situation:
             * A collides with B and B collides with A and C
             * 1. do calculations for A
             * 2. add B to intersecting of A and A to intersecting of B
             * 3. do calculations for all objects that have an empty list -> so skip B and do C
             * 4. add C to intersecting of B and B to intersecting of C
             *
             * --> no objects are missed
             */

            var memoryDictionary = new Dictionary<GameObject, Tuple<List<GameObject>, List<GameObject>>>();

            // for all objects that have a bounding box
            foreach (var currentBoundingBoxObject in ParentStructure.ObjectData.GetAllObjects(property).ToList()) {
                // Objects that are already contained in the dictionary don't have to be calculated. See 3. above
                if (memoryDictionary.ContainsKey(currentBoundingBoxObject)) {
                    continue;
                }

                if (currentBoundingBoxObject is IBoundingBox currentObject) {
                    // get all objects that intersect with the current object
                    var allIntersectingWithCurrent = GetNearCollidingObjects(currentObject.BoundingBox);


                    foreach (var intersectingWithCurrent in allIntersectingWithCurrent) {
                        // only do calculations if the object intersecting with the current object is a collidable
                        if (intersectingWithCurrent is ICollidable collidingWithCurrent) {
                            // the object that collides with the current object could be set to ignore collision with the current object
                            if (!collidingWithCurrent.IgnoreCollision.Contains(((GameObject) currentObject).Type)) {
                                // call OnCollide for the object that's colliding with the current object
                                collidingWithCurrent.OnCollide((GameObject) currentObject);

                                // add the current object to the colliding objects of the object that is colliding with the current object
                                if (!memoryDictionary.ContainsKey((GameObject) collidingWithCurrent)) {
                                    memoryDictionary.Add((GameObject) collidingWithCurrent, new Tuple<List<GameObject>, List<GameObject>>(new List<GameObject>(), new List<GameObject>()));
                                }

                                memoryDictionary[(GameObject) collidingWithCurrent].Item1.Add((GameObject) currentObject);
                            }

                            // if the current object is also a collidable
                            if (currentObject is ICollidable currentCollidable) {
                                // the current object could be set to ignore collision with the object that is collides with the current object
                                if (!currentCollidable.IgnoreCollision.Contains(((GameObject) collidingWithCurrent).Type)) {
                                    // call OnCollide for the current object that's colliding with the colliding object
                                    currentCollidable.OnCollide((GameObject) collidingWithCurrent);

                                    // add the object that is colliding with the current object to the list of colliding objects of the current object
                                    if (!memoryDictionary.ContainsKey((GameObject) currentCollidable)) {
                                        memoryDictionary.Add((GameObject) currentCollidable, new Tuple<List<GameObject>, List<GameObject>>(new List<GameObject>(), new List<GameObject>()));
                                    }

                                    memoryDictionary[(GameObject) currentCollidable].Item1.Add((GameObject) collidingWithCurrent);
                                }
                            }

                            // if the current object is a projectile call OnHit on it with the object that its colliding with
                            // note: you never have to check the other way around since nothing can collide with projectiles since they are not collidable
                            // only collidables can collide with projectiles. This is done to avoid bloating the QuadTree for collision-sorting
                            if (currentObject is RealProjectile currentProjectile) {
                                // the object that collides with the current object could be set to ignore collision with the current object
                                if (!collidingWithCurrent.IgnoreCollision.Contains(currentProjectile.Type)) {
                                    currentProjectile.OnHit(/*collidingWithCurrent*/);
                                    // alreadyChecked doesn't have to be updated with the projectile either for the same reason as explained above
                                    // note: it would only make sense if the projectile is checked twice for some reason; but if you do that you're doing something wrong.
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}