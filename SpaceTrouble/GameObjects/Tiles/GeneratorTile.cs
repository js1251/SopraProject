using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles {
    internal sealed class GeneratorTile : Tile, IBuildable, IResourceGenerator, IAnimating {
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

        // Animation
        [JsonProperty] public float CurrentFrame { get; set; }
        [JsonProperty] public int CurrentLayer { get; set; }
        [JsonIgnore] public float Angle { get; set; }
        [JsonProperty] public AnimationEffect Effect { get; set; }
        [JsonIgnore] public float AnimationSpeed { get; }


        public GeneratorTile() {
            Dimensions = new Point(64, 320); // the world unit size
            Pivot = new Vector2(0.5f, 0.5f); // the world center point relative to dimensions

            // Resource Generation
            RequiredResources = new ResourceVector(2, 5, 0);
            ResourceCapacity = new ResourceVector(0, 4, 0);
            GeneratedResources = ResourceVector.EnergyUnit;
            ResourcesUsedForGenerate = ResourceVector.Empty;
            BaseGenerationSpeed = 3;

            // Animation
            TotalFrames = new Point(12, 1);
            CurrentFrame = 0;
            CurrentLayer = 0;
            AnimationSpeed = 12f; // 12 fps (nice for pixel-art)
            Effect = AnimationEffect.None;
        }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);
            ((IAnimating)this).Update(gameTime);

            // generate Resources once building is finished
            if (BuildingFinished) {
                ((IResourceGenerator) this).Generate(gameTime);
            }
        }

        public void OnBuildingFinished() {
            BuildingFinished = true;
            HasChanged = true;
            Color = Color.White;
            Effect = AnimationEffect.PlayOnce;
            if (this is ICanHavePriority prioritizable) {
                prioritizable.HasPriority = false;
            }
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            ((IAnimating) this).Draw(spriteBatch);

            if (BuildingFinished) {
                ((IResourceContainer) this).DrawResources(spriteBatch);
            } else {
                ((IBuildable) this).DrawResources(spriteBatch);
            }

            ((ICanHavePriority)this).Draw(spriteBatch);
        }
    }
}