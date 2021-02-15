using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects;
using SpaceTrouble.GameObjects.Creatures;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.World;

// Created by Jakob Sailer

namespace SpaceTrouble.util.Tools {
    internal sealed class LocalSteering {
        private readonly ObjectManager mObjectManager;
        private Creature Creature { get; }
        private float CollisionRadius { get; }
        private float AwarenessRadius { get; }
        public List<GameObject> NearObjects { get; private set; }
        private int UpdateCount { get; set; }
        private int UpdateInterval { get; }

        public LocalSteering(Creature creature, float awarenessFactor) {
            mObjectManager = WorldGameState.ObjectManager;
            Creature = creature;
            CollisionRadius = creature.Dimensions.X / 2f;
            AwarenessRadius = CollisionRadius * awarenessFactor;
            NearObjects = new List<GameObject>();

            // optimization
            UpdateInterval = 5;
            UpdateCount = UpdateInterval;
        }

        public void Update() {
            UpdateCount++;
            if (UpdateCount >= UpdateInterval) {
                // new improved near object detection
                NearObjects = mObjectManager.GetNearObjects(Creature.WorldPosition, AwarenessRadius);
                UpdateCount = 0;
            }
        }

        public void DrawDebug(SpriteBatch spriteBatch, DebugMode mode, Vector2 steerVector) {
            if (mode is DebugMode.ObstacleAvoidance) {
                // a vector showing direction and force of the current steer vector
                spriteBatch.DrawLine(Creature.WorldPosition, Creature.WorldPosition + AwarenessRadius * steerVector, Color.Red, 0.5f);

                // a collision circle that cannot be penetrated by other object
                spriteBatch.DrawCircle(Creature.WorldPosition, CollisionRadius, Color.Red, 0.25f);

                // a awareness circle. Other objects in this radius will steer this object away from them
                spriteBatch.DrawCircle(Creature.WorldPosition, AwarenessRadius, Color.Orange, 0.25f, 32);
            }
        }

        /// <summary>
        /// Combines two direction Vectors
        /// </summary>
        /// <param name="currentHeading">The current Heading.</param>
        /// <param name="addHeading">An additional Heading to combine with.</param>
        /// <param name="stayOnWalkable">True if the object being steered should stay on walkable tiles</param>
        /// <returns></returns>
        public Vector2 CombineSteering(Vector2 currentHeading, Vector2 addHeading, bool stayOnWalkable = false) {
            // eliminate NaN operations
            if (currentHeading.Length() is float.NaN) {
                return Vector2.Zero;
            }

            // eliminate NaN operations
            if (addHeading.Length() is float.NaN || addHeading.Length() < 0.01f) {
                return currentHeading;
            }

            var newHeading = currentHeading + addHeading;

            if (newHeading.Length() < 0.01f) {
                return currentHeading;
            }


            if (stayOnWalkable) {
                // check if the resulting vector points outside the walkable tile-world
                var resultingTile = WorldGameState.ObjectManager.GetTile(CoordinateManager.WorldToTile(Creature.WorldPosition + Vector2.Normalize(newHeading)));
                if (resultingTile != null && !resultingTile.IsWalkable) {
                    return currentHeading;
                }
            }

            return Vector2.Normalize(newHeading);
        }

        public static Vector2 LerpHeading(Vector2 oldVector, Vector2 newVector, float amount) {
            return Vector2.Lerp(oldVector, newVector, amount);
        }

        /// <summary>
        /// Calculates a new heading that steers towards given object types.
        /// </summary>
        /// <param name="steerInfluences">A list of objects-types that influence steering.</param>
        /// <param name="inverse">If True use all objects that AREN'T in the list as influence.</param>
        /// <returns>A new heading vector.</returns>
        public Vector2 SteerTowards(List<GameObjectEnum> steerInfluences, bool inverse = false) {
            return Steer(steerInfluences, inverse);
        }

        /// <summary>
        /// Calculates a new heading that steers away from given object types.
        /// </summary>
        /// <param name="steerInfluences">A list of objects-types that influence steering.</param>
        /// <param name="inverse">If True use all objects that AREN'T in the list as influence.</param>
        /// <returns>A new heading vector.</returns>
        public Vector2 SteerAway(List<GameObjectEnum> steerInfluences, bool inverse = false) {
            return Steer(steerInfluences, true, inverse);
        }

        private Vector2 Steer(List<GameObjectEnum> steerInfluences, bool steerAway = false, bool inverse = false) {
            if (NearObjects.Count == 0) {
                return Vector2.Zero;
            }

            var newHeading = Vector2.Zero;

            foreach (var obstacle in NearObjects) {
                // objects should never collide with itself!
                if (Creature.Equals(obstacle)) {
                    continue;
                }

                // only avoid obstacles that should be avoided
                if (!steerInfluences.Contains(obstacle.Type) && !inverse || steerInfluences.Contains(obstacle.Type) && inverse) {
                    continue;
                }

                // that goes the other way too:
                // If a creature has found another creature to collide with BUT that other creature ignores
                // collision with this creature also skip it
                if (obstacle is ICollidable collidable && collidable.IgnoreCollision.Contains(Creature.Type)) {
                    continue;
                }

                var creaturePosition = Creature.WorldPosition;
                var obstaclePosition = obstacle.WorldPosition;

                var obstacleCollisionRadius = obstacle.Dimensions.X / 2f;
                var obstacleDistance = Vector2.Distance(creaturePosition, obstaclePosition);
                var obstacleDirection = steerAway ? Vector2.Normalize(creaturePosition - obstaclePosition) : Vector2.Normalize(obstaclePosition - creaturePosition);

                var summedCollisionRadii = CollisionRadius + obstacleCollisionRadius;
                var evadeForce = GetEvadeForce(obstacleDistance, summedCollisionRadii);
                
                // if creatures walk directly at each other
                if (Vector2.Distance(creaturePosition + Creature.Heading * obstacleDistance, obstaclePosition) < 5f) {
                    obstacleDirection += Vector2.Normalize(obstacleDirection + VectorMath.Rotate90ClockWise(obstacleDirection) * evadeForce);
                }

                newHeading += obstacleDirection * evadeForce;
            }

            return newHeading;
        }

        private static float GetEvadeForce(float obstacleDistance, float summedCollisionRadii) {
            return Math.Clamp(Math.Abs(1 / (obstacleDistance - summedCollisionRadii)), 0, 10f);
        }
    }
}