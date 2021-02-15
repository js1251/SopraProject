using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles.Interfaces {
    public interface ICanHavePriority {
        public bool HasPriority { get; set; }
        public float PriorityAlpha { get; }
        public Vector2 WorldPosition { get; }

        public void Draw(SpriteBatch spriteBatch) {
            if (HasPriority) {
                var priorityTex = this is PlatformTile ? Assets.Textures.UtilTextures.PriorityFull : Assets.Textures.UtilTextures.PriorityHalf;
                var priorityScale = Global.TileWidth / (float)priorityTex.Height;
                var priorityPos = new Vector2 {
                    X = WorldPosition.X - priorityTex.Width / 2f * priorityScale,
                    Y = WorldPosition.Y - priorityTex.Height * 0.72f * priorityScale
                };
                spriteBatch.Draw(priorityTex, priorityPos, null, Color.White * PriorityAlpha, 0, Vector2.Zero, priorityScale, SpriteEffects.None, 0);
            }
        }
    }
}