using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;
using SpaceTrouble.World;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Creatures.friendly {
    internal sealed class Minion : WalkingCreature, IAnimating, IMoving, IFriendly {
        // General
        [JsonProperty] public ResourceVector CarryingResource { get; set; } = ResourceVector.Empty;
        [JsonProperty] public Tile EnteredBuilding { get; set; } // the building this minion is inside of (or null)
        [JsonIgnore] private MinionFeeling mFeeling;
        [JsonProperty] public MinionFeeling Feeling {
            get => mFeeling;
            set {
                mFeeling = value;
                FeelingAlpha = 1f;
                FeelingScale = 2f;
            }
        }
        [JsonProperty] private float FeelingAlpha { get; set; }
        [JsonProperty] private float FeelingScale { get; set; }

        // Animation
        [JsonProperty] public float CurrentFrame { get; set; }
        [JsonProperty] public int CurrentLayer { get; set; }
        [JsonProperty] public AnimationEffect Effect { get; set; }
        [JsonIgnore] public float AnimationSpeed { get; }

        public Minion() {
            // Constants
            Dimensions = new Point(10, 25);
            Pivot = new Vector2(0.5f, 0.8f);
            Speed = 27 + RandomBehavior * 2f;
            HitPoints = (float) WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.Minion, DifficultyAttribute.HitPoints);
            Feeling = MinionFeeling.None;

            // steering & collisions
            LocalSteering = new LocalSteering(this, 20f);
            IgnoreCollision.Add(GameObjectEnum.RealProjectile);
            IgnoreCollision.Add(GameObjectEnum.FakeProjectile);
            AiImplementation = new IdleMinionAi(this);
            AiImplementation.InitializeAi();
            ReachedTolerance = 5f;

            // Animation
            TotalFrames = new Point(12, 8);
            CurrentFrame = 0;
            CurrentLayer = 0;
            AnimationSpeed = 12f; // 12 fps (nice for pixel-art)
            Effect = AnimationEffect.PlayLooping;
        }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);
            ((IAnimating) this).Update(gameTime);
            ((IAnimating)this).GetAngleFromHeading(Heading);
            ((IAnimating)this).SetLayerFromAngle();

            AiImplementation.UpdateAi(gameTime);

            if (Feeling != MinionFeeling.None) {
                FeelingScale = FeelingScale.Lerp(1, 0.1f);
                FeelingAlpha = MathExtension.Oscillation((float) gameTime.TotalGameTime.TotalSeconds, 5f, 0.5f, 1f);
            }
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            Texture2D icon = null;
            if (!CarryingResource.IsEmpty()) {
                if (CarryingResource.Food > 0) {
                    icon = Assets.Textures.Objects.FoodIcon;
                } else if (CarryingResource.Mass > 0) {
                    icon = Assets.Textures.Objects.MassIcon;
                } else if (CarryingResource.Energy > 0) {
                    icon = Assets.Textures.Objects.EnergyIcon;
                }
            }

            if (Angle > 180) {
                DrawCarrying(spriteBatch, icon);
                ((IAnimating) this).Draw(spriteBatch);
            } else {
                ((IAnimating) this).Draw(spriteBatch);
                DrawCarrying(spriteBatch, icon);
            }

            DrawFeeling(spriteBatch);
        }

        private void DrawCarrying(SpriteBatch spriteBatch, Texture2D icon) {
            if (icon == null) {
                return;
            }

            const float iconScale = 0.3f;
            const float iconElevation = 15f;
            const float iconExtension = 10f;
            var iconDrawPos = WorldPosition - Vector2.UnitY * iconElevation;
            iconDrawPos.X += (float) Math.Cos(Angle * Math.PI / 180) * iconExtension;
            iconDrawPos -= new Vector2(icon.Width, icon.Height) / 2f * iconScale;

            spriteBatch.Draw(icon, iconDrawPos, null, Color.White, 0, Vector2.Zero, iconScale, SpriteEffects.None, 0);
        }

        private void DrawFeeling(SpriteBatch spriteBatch) {
            if (Feeling == MinionFeeling.None) {
                return;
            }

            var confusedTex = Assets.Textures.UtilTextures.Question;
            var alarmedTex = Assets.Textures.UtilTextures.Excalamation;

            var feelingPos = WorldPosition - new Vector2(confusedTex.Width / 2f, confusedTex.Height) * DrawScale * FeelingScale * 0.5f;
            feelingPos.Y -= Dimensions.Y;
            switch (Feeling) {
                case MinionFeeling.Alarmed:
                    spriteBatch.Draw(alarmedTex, feelingPos, null, Color.Red * FeelingAlpha, 0, Vector2.Zero, DrawScale * FeelingScale, SpriteEffects.None, 0);
                    return;
                case MinionFeeling.Confused:
                    spriteBatch.Draw(confusedTex, feelingPos, null, Color.Yellow * FeelingAlpha, 0, Vector2.Zero, DrawScale * FeelingScale, SpriteEffects.None, 0);
                    return;
            }
        }

        /// <summary>
        /// Calculates a steer vector that will be applied to the movement of the minion. Based on 
        /// </summary>
        /// <param name="currentHeading"></param>
        /// <returns></returns>
        Vector2 IMoving.Steer(Vector2 currentHeading) {
            // by setting the steer parameters to be the inverse of the ignore-collision list the minion will try to steer away from ANYTHING except the ignored types
            SteerVector = LocalSteering.SteerAway(IgnoreCollision, true);
            var newHeading = LocalSteering.CombineSteering(currentHeading, SteerVector, true);
            Heading = LocalSteering.LerpHeading(Heading, newHeading, 0.1f * RandomBehavior);
            return Heading;
        }

        /// <summary>
        /// Called whenever a minion has collided with another GameObject. Takes IgnoreList into account.
        /// </summary>
        /// <param name="collisionObject">The GameObject that is colliding with the minion.</param>
        public override void OnCollide(GameObject collisionObject) {
            if (collisionObject is IEnemy enemy && !enemy.IsAttacking) {
                enemy.IsAttacking = true;
                Damage(enemy.AttackDamage);
            }
        }
    }
}