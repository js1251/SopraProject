using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles {
    internal sealed class KitchenTile : Tile, IBuildable, IResourceGenerator {
        [JsonProperty] public ResourceVector OnTheWayResources { get; set; }
        [JsonProperty] public ResourceVector RequiredResources { get; set; }
        [JsonProperty] public bool BuildingFinished { get; set; }
        [JsonProperty] public ResourceVector Resources { get; set; }
        [JsonProperty] public ResourceVector ResourceCapacity { get; }
        [JsonProperty] public ResourceVector PromisedResources { get; set; }
        [JsonProperty] public ResourceVector GeneratedResources { get; }
        [JsonProperty] public ResourceVector ResourcesUsedForGenerate { get; }
        [JsonProperty] public double BaseGenerationSpeed { get; }
        [JsonProperty] public double TimeSinceLastGenerate { get; set; }

        public KitchenTile() {
            Pivot = new Vector2(0.5f, 0.625f);
            RequiredResources = new ResourceVector(2, 2, 4);
            ResourceCapacity = new ResourceVector(0, 0, 4);
            GeneratedResources = ResourceVector.FoodUnit;
            ResourcesUsedForGenerate = ResourceVector.Empty;
            BaseGenerationSpeed = 4;
        }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);

            // generate Resources once building is finished
            if (BuildingFinished) {
                ((IResourceGenerator)this).Generate(gameTime);
            }
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
            if (BuildingFinished) {
                ((IResourceContainer)this).DrawResources(spriteBatch);
            } else {
                ((IBuildable)this).DrawResources(spriteBatch);
            }
        }
    }
}