using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles {
    internal class PlatformTile : Tile, IBuildable {
        [JsonProperty] public ResourceVector OnTheWayResources { get; set; }
        [JsonProperty] public ResourceVector RequiredResources { get; set; }
        [JsonProperty] public bool BuildingFinished {get; set; }
        [JsonIgnore] public override bool IsWalkable => BuildingFinished;
        [JsonIgnore] public override bool IsEnterable => !BuildingFinished; // platforms should only be enterable during construction

        public PlatformTile() {
            Pivot = new Vector2(0.5f, 0.5f); // TODO: move all tileSprites to new standard (pivot: 0.5, 0.5)
            RequiredResources = new ResourceVector(1, 1, 0);
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
            if (!BuildingFinished) {
                ((IBuildable) this).DrawResources(spriteBatch);
            }
        }
    }
}