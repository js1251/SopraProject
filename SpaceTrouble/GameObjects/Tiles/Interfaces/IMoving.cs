using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects.Creatures;
using SpaceTrouble.GameObjects.Creatures.enemy;
using SpaceTrouble.GameObjects.Creatures.friendly;
using SpaceTrouble.Menu.Statistics;
using SpaceTrouble.util.Tools;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles.Interfaces {
    internal interface IMoving {
        public Vector2 WorldPosition { get; set; }
        public Vector2 Heading { get; }
        public float Speed { get; }
        public Stack<Vector2> TargetDestinations { get; }
        public Stack<Vector2> WayPoints { get; set; }
        public float ReachedTolerance { get; }

        /// <summary>
        /// Draws some debug information
        /// </summary>
        public void DrawDebug(SpriteBatch spriteBatch, DebugMode mode) {
            if (this is WalkingCreature && (mode is DebugMode.NavigationWalkingMesh || mode is DebugMode.NavigationWalkingNodes) || this is FlyingEnemy && mode is DebugMode.NavigationFlying) {
                // draw line to all next targetDestination
                if (TargetDestinations.Count > 0) {
                    var lineStart = WorldPosition;

                    // this is beyond stupid?!?! why is the stack reversed when giving it as an init parameter?!
                    var cSharpIsStupidList = TargetDestinations.ToArray().Reverse();

                    var tempStack = new Stack<Vector2>(cSharpIsStupidList);

                    while (tempStack.Count > 0) {
                        var lineEnd = tempStack.Pop();
                        var color = this is IEnemy ? Color.OrangeRed : Color.DarkSeaGreen;
                        spriteBatch.DrawLine(lineStart, lineEnd, color * 0.5f, 2f);
                        lineStart = lineEnd;
                    }
                }
            } else if (mode is DebugMode.ObstacleAvoidance) {
                // draw current heading
                if (WayPoints.Count > 0) {
                    spriteBatch.DrawLine(WorldPosition, 10f * Heading + WorldPosition, Color.Green);
                }
            }
        }

        /// <summary>
        /// This method is called whenever a moving has no next way-points to move to. Defaults to a straight line to the next TargetDestination.
        /// </summary>
        /// <returns>A stack of way-points.</returns>
        public Stack<Vector2> GetWayPoints() {
            var wayPoints = new Stack<Vector2>();
            if (TargetDestinations.Count > 0) {
                wayPoints.Push(TargetDestinations.Peek());
            }

            return wayPoints;
        }

        /// <summary>
        /// Called whenever a moving reaches a way-point
        /// </summary>
        public void OnReachedWayPoint() {
            // definitely pop the current way-point
            if (WayPoints.Count > 0) {
                WayPoints.Pop();
            }

            // if the current position is also a TargetDestination also pop the current TargetDestination
            if (TargetDestinations.Count > 0 && Vector2.Distance(WorldPosition, TargetDestinations.Peek()) <= ReachedTolerance) {
                OnReachedTargetDestination();
            }
        }

        /// <summary>
        /// Called whenever a moving reaches a target-destination
        /// </summary>
        public void OnReachedTargetDestination() {
            NextTargetDestination();
        }

        /// <summary>
        /// Called to get the next TargetDestination from the Stack.
        /// </summary>
        public void NextTargetDestination() {
            if (TargetDestinations.Count > 0) {
                TargetDestinations.Pop();
            }
        }

        /// <summary>
        /// Handles local steering. Override to change behavior. Default is no steering.
        /// </summary>
        /// <param name="currentHeading">The current heading</param>
        /// <returns>The new heading.</returns>
        public Vector2 Steer(Vector2 currentHeading) {
            return currentHeading;
        }

        /// <summary>
        /// Handles basic movement towards the next target. Override to change behavior. Default is moving towards the target at constant given speed.
        /// </summary>
        /// <param name="targetPosition">The target Vector to move towards.</param>
        /// <param name="gameTime">The GameTime. Used to sync speed with the frame-rate.</param>
        public void MoveTowardsNextTarget(Vector2 targetPosition, GameTime gameTime) {
            // the direction to move in
            var heading = Vector2.Normalize(targetPosition - WorldPosition);

            if (this is ICollidable) {
                heading = Steer(heading);
            }

            var addedDistance = Speed * gameTime.ElapsedGameTime.Milliseconds / 1000f;
            WorldPosition += heading * addedDistance;

            if (this is Minion) {
                SpaceTrouble.StatsManager.AddValue(Statistic.MinionTraveledDistance, addedDistance);
            }
        }

        /// <summary>
        /// Moves the moving towards its next way-point
        /// </summary>
        public void Move(GameTime gameTime) {
            if (float.IsNaN(WorldPosition.X) || float.IsNaN(WorldPosition.Y) || TargetDestinations.Count <= 0) {
                return;
            }

            if (WayPoints.Count > 0) {
                // a new target-destination could have been added while the object is already moving
                if (WayPoints.ToArray()[^1] != TargetDestinations.Peek()) {
                    // if that's the case push a new way-point on top of the current way-point stack 
                    WayPoints = GetWayPoints();
                }

                // if the creature has reached the current way-point get the next one
                if (WayPoints.Count > 0) {
                    if (Vector2.Distance(WorldPosition, WayPoints.Peek()) <= ReachedTolerance) {
                        OnReachedWayPoint();
                    } else {
                        // otherwise move the object closer to the next way-point
                        MoveTowardsNextTarget(WayPoints.Peek(), gameTime);
                    }
                }
            } else {
                // if no way-points are present try to get new ones
                WayPoints = GetWayPoints();
            }
        }
    }
}