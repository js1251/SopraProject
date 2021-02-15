using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Projectiles;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.Statistics;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures {
    internal abstract class Creature : GameObject, ICollidable, IMoving {
        // health and collision
        [JsonIgnore] public RectangleF BoundingBox => ((IBoundingBox) this).GetBoundingBox();
        [JsonProperty] public float HitPoints { get; set; }
        [JsonProperty] protected float RegenerationRate { get; set; }
        [JsonProperty] public List<GameObjectEnum> IgnoreCollision { get; set; }

        // world interaction
        [JsonProperty] public CreatureAi AiImplementation { get; set; } // TODO: fix loading
        [JsonProperty] public Tile SpawnOrigin { get; set; } // TODO: fix loading

        // moving
        [JsonProperty] public Stack<Vector2> TargetDestinations { get; set; }
        [JsonProperty] public float Speed { get; set; }
        [JsonIgnore] public Vector2 Heading { get; protected set; }
        [JsonProperty] public Stack<Vector2> WayPoints { get; set; }
        [JsonIgnore] public float ReachedTolerance { get; protected set; } // how close the creature needs to be to its next target to consider it reached (in world units)
        [JsonIgnore] protected float RandomBehavior { get; }


        // local steering
        [JsonProperty] public LocalSteering LocalSteering { get; set; }
        [JsonIgnore] protected Vector2 SteerVector { get; set; }

        protected Creature() {
            IgnoreCollision = new List<GameObjectEnum>();
            TargetDestinations = new Stack<Vector2>();
            WayPoints = new Stack<Vector2>();
            RandomBehavior = new Random().Next(3,10) * 0.1f;
            ReachedTolerance = 2f;
        }

        protected void Damage(float damage) {
            // checking for > 0 first because of below comment
            if (HitPoints > 0) {
                HitPoints -= damage;
                if (HitPoints <= 0f) {
                    // Die seems to be called multiple times on a single Update?! this brakes things. See above comment
                    Die();
                }
            }
        }

        internal override void OnOffWorld() {
            Damage(HitPoints);
            System.Diagnostics.Debug.WriteLine("OF WORLD!!: " + this + " @ " + WorldPosition);
        }

        private void Die() {
            if (SpawnOrigin != null && SpawnOrigin is CreatureSpawnerTile spawner) {
                spawner.CreatureHasDied(this);
            }

            AiImplementation.OnCreatureDies();
            WorldGameState.ObjectManager.Remove(this);

            if (this is IFriendly) {
                SpaceTrouble.SoundManager.PlaySound(Sound.Dying);
                SpaceTrouble.StatsManager.AddValue(Statistic.MinionsKilled, 1);
            }

            if (this is IEnemy) {
                SpaceTrouble.StatsManager.AddValue(Statistic.EnemiesKilled, 1);
            }
        }


        public virtual void OnCollide(GameObject collisionObject) {
            // If an Enemy is hit by a friendly Bullet or if an Ally is hit by an Enemy => Damage the Object
            if (collisionObject is RealProjectile cObject) {
                if (this is IEnemy && cObject.IsFriendly || this is IFriendly && !cObject.IsFriendly) {
                    Damage(cObject.ProjectileDamage);
                }
            }
        }

        internal override void DrawDebug(SpriteBatch spriteBatch, DebugMode mode) {
            base.DrawDebug(spriteBatch, mode);
            ((IMoving) this).DrawDebug(spriteBatch, mode);
            ((IBoundingBox) this).DrawDebug(spriteBatch, mode);
            LocalSteering.DrawDebug(spriteBatch, mode, SteerVector);
        }
    }
}