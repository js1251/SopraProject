using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles {
    internal abstract class Tile : GameObject, ICanHavePriority {
        [JsonProperty] public virtual bool IsWalkable { get; set; } // whether or not a tile can be walked on
        [JsonProperty] public virtual bool IsEnterable { get; protected set; } = true; // whether or not a tile can be entered
        [JsonIgnore] public Vector2 HeadingBias { get; set; } // a vector that hints certain creatures where to steer locally
        [JsonProperty] public bool HasPriority { get; set; }
        [JsonIgnore] public float PriorityAlpha { get; set; } = 0.5f;

        protected Tile() {
            Dimensions = new Point(64,64);
            Pivot = new Vector2(0.5f, 0.75f); // TODO: move all tileSprites to new standard (pivot: 0.5, 0.5)
            Color = new Color(.5f, .5f, .5f, .5f); // the color for tiles that aren't built yet
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
            ((ICanHavePriority) this).Draw(spriteBatch);
        }
    }
}