using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.Tools;
using Point = Microsoft.Xna.Framework.Point;

namespace SpaceTrouble.GameObjects.Projectiles {
    internal sealed class RealProjectile : Projectile, IBoundingBox {
        [JsonProperty] public float ProjectileDamage { get; set; }
        [JsonProperty] public bool IsFriendly { get; set; }
        [JsonIgnore] public RectangleF BoundingBox => ((IBoundingBox) this).GetBoundingBox();

        public RealProjectile() {
            Dimensions = new Point(25, 1);
            Pivot = new Vector2(0f, 0.5f);
        }

        public void OnHit(/*ICollidable collidable = null*/) {
            SpawnExplosionAndRemove();
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            if (Texture == null || !IsVisible) {
                return;
            }

            spriteBatch.Draw(Texture, DrawPosition, null, Color, Angle, Pivot, DrawScale, SpriteEffects.None, 1);
        }

        internal override void DrawDebug(SpriteBatch spriteBatch, DebugMode mode) {
            base.DrawDebug(spriteBatch, mode);
            ((IBoundingBox) this).DrawDebug(spriteBatch, mode);
        }

        RectangleF IBoundingBox.GetBoundingBox() {
            return new RectangleF(WorldPosition.X - Dimensions.Y * 10, WorldPosition.Y - Dimensions.Y * 10, Dimensions.Y * 20, Dimensions.Y * 20);
        }
    }
}