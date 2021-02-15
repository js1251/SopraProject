using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures.friendly {
    internal enum MinionFeeling {
        None,
        Alarmed,
        Confused
    }

    internal abstract class MinionAi : CreatureAi {
        [JsonProperty] protected Minion Minion { get; set; } // The Minion this AI is assigned to
        [JsonIgnore] private float IdleWaitTime { get; set; }
        [JsonIgnore] private float NextIdleaskTime { get; set; }
        [JsonProperty] protected bool HasFear { get; set; }
        [JsonProperty] protected Vector2? RefugeTarget { get; set; }
        [JsonIgnore] private float FearSprintBoost { get; }
        [JsonIgnore] private float OriginalSpeed { get; }
        [JsonIgnore] private float SprintDuration { get; }
        [JsonIgnore] private int FleeDistance { get; }

        protected MinionAi(Minion minion) {
            IdleWaitTime = 0.5f;
            NextIdleaskTime = IdleWaitTime;
            Minion = minion;
            OriginalSpeed = Minion.Speed;
            FearSprintBoost = 1.5f;
            SprintDuration = 2f;
            FleeDistance = 500;
        }

        public override void InitializeAi() {
        }

        public override void UpdateAi(GameTime gameTime) {
            if (HasFear) {
                Minion.Speed = Minion.Speed.Lerp(OriginalSpeed, (float) gameTime.ElapsedGameTime.TotalSeconds / SprintDuration);
            }

            NextIdleaskTime -= (float) gameTime.ElapsedGameTime.TotalSeconds;
            if (NextIdleaskTime > 0) {
                return;
            }

            NextIdleaskTime += IdleWaitTime;

            CheckForFear();

            // skip algorithms if minion has an active task or has fear
            if (HasFear || !IsIdle() && Minion.EnteredBuilding == null) {
                return;
            }

            // first try to get a new pending Ai
            if (TryGetNewAi()) {
                return;
            }

            // second try to get a new target
            var newTargets = GetNewTargets();
            if (newTargets.Count > 0) {
                // if the minion is currently in a building exit it
                if (Minion.EnteredBuilding != null) {
                    ExitBuilding();
                }

                // add the new target to the stack of targets
                while (newTargets.Count > 0) {
                    Minion.TargetDestinations.Push(newTargets.Pop());
                }

                return;
            }

            // third enter a building to be idle in
            if (Minion.EnteredBuilding == null) {
                EnterBuilding(Minion.SpawnOrigin);
            }

            // if the minion has been assigned a building to enter see if it arrived
            if (Minion.EnteredBuilding != null) {
                CheckIfReachedBuilding();
            }
        }

        private void CheckForFear() {
            var nearObjects = Minion.LocalSteering.NearObjects;
            foreach (var gameObject in nearObjects) {
                if (!(gameObject is IEnemy)) {
                    continue;
                }

                if (HasFear) {
                    StopHavingFear();
                }

                RefugeTarget = GetFearRefuge(gameObject);
                if (RefugeTarget != null) {

                    // if the Minion is currently within a building exit it
                    if (!Minion.IsVisible) {
                        ExitBuilding();
                    }

                    HasFear = true;
                    Minion.Speed *= FearSprintBoost;
                    Minion.Feeling = MinionFeeling.Alarmed;

                    if (RefugeTarget != null) { // stupid check but I guess ExitBuilding() could have changed it. oh well...
                        Minion.TargetDestinations.Push((Vector2) RefugeTarget);
                    }

                    NextIdleaskTime += 5f;
                }

                return;
            }

            // if there are no enemies nearby anymore stop having fear
            if (HasFear) {
                StopHavingFear();
            }
        }

        private void StopHavingFear() {
            HasFear = false;
            Minion.Feeling = MinionFeeling.None;
            Minion.Speed = OriginalSpeed;
            if (Minion.TargetDestinations.Count > 0 && Minion.TargetDestinations.Peek().Equals(RefugeTarget)) {
                Minion.TargetDestinations.Pop();
            }
        }

        /* This algorithm will search for a walkable tile within a half circle pointing away from the enemy
         * At first a tile at the greatest distance will be searched. The radius is gradually shortened.
         * The first vector is pointing directly away from the enemy. Every time no refuge has been found
         * It is rotated a bit and it is checked again. Between each rotate the direction is flipped
         * So no side is biased. If no valid tile is found the Minion will not flee (there's probably no escape).
         */
        private Vector2? GetFearRefuge(GameObject fleeingFrom) {
            var fleeingDirection = Vector2.Normalize(fleeingFrom.WorldPosition - Minion.WorldPosition) * -1f;

            // just in case the direction summed up to 0
            if (fleeingDirection.Length() is float.NaN) {
                return null;
            }

            var searchRadius = FleeDistance;
            while (searchRadius > 0) {
                var halfCircumference = Math.PI * searchRadius;
                var iterations = halfCircumference / Global.TileHeight;

                var potentialRefugeLeft = Minion.WorldPosition + searchRadius * fleeingDirection;
                var potentialRefugeRight = Minion.WorldPosition + searchRadius * fleeingDirection;
                for (var i = 0; i < iterations / 2f; i++) {
                    var leftTile = WorldGameState.ObjectManager.GetTile(CoordinateManager.WorldToTile(potentialRefugeLeft));
                    if (leftTile is PlatformTile platform1 && platform1.BuildingFinished) {
                        return platform1.WorldPosition;
                    }

                    var rightTile = WorldGameState.ObjectManager.GetTile(CoordinateManager.WorldToTile(potentialRefugeRight));
                    if (rightTile is PlatformTile platform2 && platform2.BuildingFinished) {
                        return platform2.WorldPosition;
                    }

                    var rotatedVector = Vector2.Normalize(VectorMath.Rotate90ClockWise(fleeingDirection)) * Global.TileHeight;
                    potentialRefugeLeft -= rotatedVector;
                    potentialRefugeRight += rotatedVector;
                }

                searchRadius -= Global.TileHeight;
            }

            return null;
        }

        private bool TryGetNewAi() {
            var newAi = WorldGameState.TaskManager.GetNewAi(Minion);
            if (newAi.Equals(Minion.AiImplementation)) {
                return false;
            }

            Minion.AiImplementation = newAi;
            return true;
        }

        private void CheckIfReachedBuilding() {
            var currentTile = WorldGameState.ObjectManager.GetTile(CoordinateManager.WorldToTile(Minion.WorldPosition));
            if (currentTile is null || !currentTile.Equals(Minion.EnteredBuilding)) {
                return;
            }

            if (!Minion.IgnoreCollision.Contains(GameObjectEnum.Minion)) {
                Minion.IgnoreCollision.Add(GameObjectEnum.Minion);
                Minion.IsVisible = false;
            }
        }

        public override void OnReachedTargetDestination() {
            if (HasFear) {
                StopHavingFear();
            }
        }

        public override void OnCreatureDies() {
            WorldGameState.TaskManager.MinionHasDied(Minion);
        }


        protected virtual Stack<Vector2> GetNewTargets() {
            return new Stack<Vector2>();
        }

        /// <summary>
        /// True whenever a minion has finished a SINGLE task
        /// </summary>
        /// <returns>True if the minion currently doesn't have a task.</returns>
        public override bool IsIdle() {
            return Minion.TargetDestinations.Count <= 0;
        }

        private void EnterBuilding(Tile building) {
            if (Minion.EnteredBuilding == null) {
                if (building != null) {
                    if (building.IsEnterable) {
                        Minion.TargetDestinations.Push(building.WorldPosition);
                        Minion.EnteredBuilding = building;
                        IdleWaitTime *= 2f;
                    } else {
                        System.Diagnostics.Debug.WriteLine("Building " + building.Type + " is not enterable!");
                    }
                } else {
                    System.Diagnostics.Debug.WriteLine("The building to enter is null!");
                }
            } else {
                System.Diagnostics.Debug.WriteLine("Minion is already inside a building (" + Minion.EnteredBuilding + ")");
            }
        }

        private void ExitBuilding() {
            // the minion might still be on its way to the building to enter so reset the target stack
            // note: since the main update skips the algorithms when the minion has fear
            // it can always be assumed that the stack only contains a target for the building to enter
            Minion.TargetDestinations = new Stack<Vector2>();
            Minion.IgnoreCollision.Remove(GameObjectEnum.Minion);
            Minion.IsVisible = true;
            Minion.EnteredBuilding = null;
            IdleWaitTime /= 2f;
        }
    }
}