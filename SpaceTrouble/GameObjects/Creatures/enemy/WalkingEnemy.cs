using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures.enemy {
    internal sealed class WalkingEnemy : WalkingCreature, IAnimating, IEnemy, IMoving {
        // Animation
        [JsonProperty] public float CurrentFrame { get; set; }
        [JsonProperty] public int CurrentLayer { get; set; }
        [JsonProperty] public AnimationEffect Effect { get; set; }
        [JsonIgnore] public float AnimationSpeed { get; }
        [JsonIgnore] public float AttackDamage { get; }
        [JsonIgnore] public bool IsAttacking { get; set; }
        [JsonIgnore] private int AttackAnimationLength { get; }
        [JsonIgnore] private int ElapsedFrames { get; set; }

        public WalkingEnemy() {
            // Constants
            Dimensions = new Point(15, 18);
            Pivot = new Vector2(0.5f, 0.6f);
            Speed = 25f;
            HitPoints = WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.WalkingEnemy, DifficultyAttribute.HitPoints) - RandomBehavior;
            AttackDamage = 100;
            IsAttacking = false;
            AttackAnimationLength = 30;
            ElapsedFrames = 0;

            // steering & collisions
            LocalSteering = new LocalSteering(this, 2.5f);
            AiImplementation = new WalkingEnemyAi(this);
            AiImplementation.InitializeAi();

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

            ((IAnimating)this).Update(gameTime);
            ((IAnimating)this).GetAngleFromHeading(Heading);
            ((IAnimating)this).SetLayerFromAngle();
            AiImplementation.UpdateAi(gameTime);
        }

        Vector2 IMoving.Steer(Vector2 currentHeading) {
            var objectsToAvoid = new List<GameObjectEnum> {
                GameObjectEnum.FlyingEnemy,
                GameObjectEnum.WalkingEnemy
            };

            // steer away from other enemies but steer towards minions
            var steerAwayHeading = LocalSteering.SteerAway(objectsToAvoid);
            var steerTowardsHeading = LocalSteering.SteerTowards(new List<GameObjectEnum> {GameObjectEnum.Minion});

            // combine all steer influences
            SteerVector = LocalSteering.CombineSteering(steerAwayHeading, steerTowardsHeading, true);
            var newHeading = LocalSteering.CombineSteering(currentHeading, SteerVector, true);

            // apply steer influences
            Heading = LocalSteering.LerpHeading(Heading, newHeading, 0.2f);
            return Heading;
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            ((IAnimating)this).Draw(spriteBatch);
        }
    }
}