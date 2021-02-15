using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures {
    internal abstract class WalkingCreature : Creature, IMoving {
        // moving
        [JsonIgnore] public float Angle { get; set; }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);
            LocalSteering.Update();
            ((IMoving) this).Move(gameTime);
        }

        void IMoving.OnReachedTargetDestination() {
            ((IMoving) this).NextTargetDestination();
            AiImplementation.OnReachedTargetDestination();
        }

        Stack<Vector2> IMoving.GetWayPoints() {
            if (TargetDestinations.Count > 0) {
                var newWayPoints = WorldGameState.NavigationManager.FindPathOnTiles(WorldPosition, TargetDestinations.Peek());
                if (newWayPoints.Count > 0) {
                    WayPoints = newWayPoints;
                    return newWayPoints;
                }

                // if no way-point stack has been returned at this point remove the current targetDestination since it has been reached
                TargetDestinations.Pop();
            }

            return new Stack<Vector2>();
        }
    }
}