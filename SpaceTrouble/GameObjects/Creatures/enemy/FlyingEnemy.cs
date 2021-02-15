using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;

// Expanded upon by Jakob Sailer

namespace SpaceTrouble.GameObjects.Creatures.enemy {
    internal sealed class FlyingEnemy : Creature, IEnemy, IMoving, IAnimating {
        [JsonIgnore] public float AttackDamage { get; }
        [JsonIgnore] public bool IsAttacking { get; set; }
        [JsonIgnore] private int AttackAnimationLength { get; }
        [JsonIgnore] private int ElapsedFrames { get; set; }

        // animation
        [JsonIgnore]public float CurrentFrame { get; set; }
        [JsonIgnore]public int CurrentLayer { get; set; }
        [JsonIgnore]public float Angle { get; set; }
        [JsonIgnore]public float AnimationSpeed { get; }
        [JsonIgnore] public AnimationEffect Effect { get; set; }

        public FlyingEnemy() {
            Dimensions = new Point(32, 32);
            Pivot = new Vector2(0.5f, 0.5f);
            DrawFlying = true;
            Speed = 30f;
            AttackDamage = 20;
            IsAttacking = false;
            AttackAnimationLength = 60;
            ElapsedFrames = 0;
            AiImplementation = new FlyingEnemyAi(this);
            HitPoints = WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.FlyingEnemy, DifficultyAttribute.HitPoints) - RandomBehavior;

            LocalSteering = new LocalSteering(this, 2.5f);
            ReachedTolerance = 10f;

            // Animation
            TotalFrames = new Point(12, 8);
            CurrentFrame = 0;
            CurrentLayer = 0;
            AnimationSpeed = 12f; // 12 fps (nice for pixel-art)
            Effect = AnimationEffect.PlayLooping;
        }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (IsAttacking) {
                if (ElapsedFrames >= AttackAnimationLength) {
                    IsAttacking = false;
                    ElapsedFrames = 0;
                } else {
                    ElapsedFrames++;
                    return;
                }
            }

            LocalSteering.Update();
            ((IMoving)this).Move(gameTime);
            AiImplementation.UpdateAi(gameTime);
            HasChanged = true;

            ((IAnimating)this).Update(gameTime);
            ((IAnimating)this).GetAngleFromHeading(Heading);
            ((IAnimating)this).SetLayerFromAngle();
        }

        void IMoving.OnReachedTargetDestination() {
            ((IMoving)this).NextTargetDestination();
            AiImplementation.OnReachedTargetDestination();
        }

        Vector2 IMoving.Steer(Vector2 currentHeading) {
            var objectsToAvoid = new List<GameObjectEnum> {
                GameObjectEnum.FlyingEnemy,
                GameObjectEnum.WalkingEnemy
            };

            var steerAwayHeading = LocalSteering.SteerAway(objectsToAvoid);
            var steerTowardsHeading = LocalSteering.SteerTowards(new List<GameObjectEnum> { GameObjectEnum.Minion });
            SteerVector = LocalSteering.CombineSteering(steerAwayHeading, steerTowardsHeading);

            // take tile vector field into account

            var currentTile = WorldGameState.ObjectManager.GetTile(CoordinateManager.WorldToTile(WorldPosition));
            if (currentTile != null) {
                var distanceFromTile = Vector2.Distance(WorldPosition, currentTile.WorldPosition);
                SteerVector = LocalSteering.CombineSteering( SteerVector, currentTile.HeadingBias * 1 / distanceFromTile);
            }

            var newHeading = LocalSteering.CombineSteering(currentHeading, SteerVector);
            Heading = LocalSteering.LerpHeading(Heading, newHeading, 0.2f);
            return Heading;
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            ((IAnimating)this).Draw(spriteBatch);
        }
    }
}