using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles {
    internal sealed class EmptyTile : Tile {

        [JsonIgnore] public float FadeAmount { get; set; } // used by Highlighting.cs
        public EmptyTile() {
            Pivot = new Vector2(0.5f, 0.5f);
            Color = Color.White;
            IsEnterable = false;
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            if (Texture == null || !IsVisible) {
                return;
            }

            spriteBatch.Draw(Texture, DrawPosition, null, Color * FadeAmount, 0, Vector2.Zero, DrawScale, SpriteEffects.None, 0);
        }
    }
}