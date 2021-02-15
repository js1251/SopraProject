using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.util.Tools;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace SpaceTrouble.GameObjects.Tiles.Interfaces {
    /// <summary>
    /// An interface that defines and object with a rectangle
    /// </summary>
    public interface IBoundingBox {
        /// <summary>
        /// A bounding box around the object. Can be used for collisions etc
        /// </summary>
        RectangleF BoundingBox { get; }
        Point Dimensions { get; }
        Vector2 Pivot { get; }
        Vector2 DrawPosition { get; }
        Vector2 WorldPosition { get; }

        public void DrawDebug(SpriteBatch spriteBatch, DebugMode mode) {
            if (mode is DebugMode.Collision) {
                var boundingVertices = new[] {
                    new Vector2(BoundingBox.Left, BoundingBox.Top),
                    new Vector2(BoundingBox.Right, BoundingBox.Top),
                    new Vector2(BoundingBox.Right, BoundingBox.Bottom),
                    new Vector2(BoundingBox.Left, BoundingBox.Bottom),
                };
                spriteBatch.DrawPolygon(boundingVertices, boundingVertices.Length, Color.Green, 0.5f);
            }
        }

        public RectangleF GetBoundingBox() {
            return new RectangleF(WorldPosition.X - Dimensions.X * Pivot.X, DrawPosition.Y, Dimensions.X, Dimensions.Y);
        }
    }
}