using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using Point = Microsoft.Xna.Framework.Point;

namespace SpaceTrouble.GameObjects.Projectiles {
    internal sealed class ProjectileExplosion : GameObject, IAnimating, IBoundingBox {
        [JsonIgnore] public float CurrentFrame { get; set; }
        [JsonIgnore] public int CurrentLayer { get; set; }
        [JsonIgnore] public float Angle { get; set; }
        [JsonIgnore] public float AnimationSpeed { get; }
        [JsonIgnore] public AnimationEffect Effect { get; set; }
        [JsonIgnore] public RectangleF BoundingBox => ((IBoundingBox) this).GetBoundingBox();

        public ProjectileExplosion() {
            TotalFrames = new Point(12, 1);
            Dimensions = new Point(128, 128);
            Pivot = new Vector2(0.5f, 0.5f);
            DrawFlying = true;
            AnimationSpeed = 12f;
            Effect = AnimationEffect.PlayOnce;
        }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);
            ((IAnimating) this).Update(gameTime);
            if (CurrentFrame >= TotalFrames.X - 1) {
                OnOffWorld(); // effectively removes this object once the whole animation has played
            }
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            ((IAnimating)this).Draw(spriteBatch);
        }
    }
}