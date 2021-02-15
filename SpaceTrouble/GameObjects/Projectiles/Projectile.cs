using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;
using Point = Microsoft.Xna.Framework.Point;

namespace SpaceTrouble.GameObjects.Projectiles {
    internal abstract class Projectile : GameObject {
        // World interaction
        [JsonIgnore] public float Speed { get; }

        [JsonIgnore] private Vector2 mTarget;

        [JsonIgnore] protected float Angle { get; private set; }

        [JsonProperty] public Vector2 Target {
            get => mTarget;
            set {
                mTarget = value;
                Angle = VectorMath.VectorToRadians(Target - WorldPosition);
            }
        }

        protected Projectile() {
            Dimensions = new Point(25, 1);
            Pivot = new Vector2(0f, 0.5f);
            Speed = 600f; // in units per second
        }

        internal override void Update(GameTime gameTime) {
            if (Target.X is float.NaN || Target.Y is float.NaN) {
                return;
            }

            base.Update(gameTime);

            // Update the Position
            WorldPosition += Vector2.Normalize(Target - WorldPosition) * Speed * (float) gameTime.ElapsedGameTime.TotalSeconds;

            // Projectiles should be deleted once they are close enough to their destination. Threshold needs to be quite big since they move so fast
            // and could miss the point between two updates (and just go back and forth around the target).
            if (Vector2.Distance(WorldPosition, Target) <= 20f) {
                SpawnExplosionAndRemove();
            }
        }

        internal override void OnOffWorld() {
            SpawnExplosionAndRemove();
        }

        protected void SpawnExplosionAndRemove() {
            WorldGameState.ObjectManager.CreateObject(WorldPosition, GameObjectEnum.ProjectileExplosion);
            base.OnOffWorld();
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            if (Texture == null || !IsVisible) {
                return;
            }

            spriteBatch.Draw(Texture, DrawPosition, null, Color, Angle, Pivot, DrawScale, SpriteEffects.None, 1);
        }
    }
}