using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles {
    internal sealed class ExtractorTile : Tile, IBuildable, IResourceGenerator, IAnimating {
        [JsonProperty] public ResourceVector OnTheWayResources { get; set; }
        [JsonProperty] public ResourceVector RequiredResources { get; set; }
        [JsonProperty] public ResourceVector Resources { get; set; }
        [JsonProperty] public ResourceVector ResourceCapacity { get; }
        [JsonProperty] public ResourceVector PromisedResources { get; set; }
        [JsonProperty] public ResourceVector GeneratedResources { get; }
        [JsonProperty] public ResourceVector ResourcesUsedForGenerate { get; }
        [JsonProperty] public double BaseGenerationSpeed { get; }
        [JsonProperty] public double TimeSinceLastGenerate { get; set; }
        [JsonProperty] public bool BuildingFinished { get; set; }

        // Animation
        [JsonIgnore] public float CurrentFrame { get; set; }
        [JsonIgnore] public int CurrentLayer { get; set; }
        [JsonIgnore] public float Angle { get; set; }
        [JsonIgnore] public AnimationEffect Effect { get; set; }
        [JsonIgnore] public float AnimationSpeed { get; }

        public ExtractorTile() {
            RequiredResources = new ResourceVector(2, 2, 0);
            ResourceCapacity = new ResourceVector(5, 0, 0,20);
            Resources = new ResourceVector(0, 0, 0, 20);

            GeneratedResources = ResourceVector.MassUnit;
            ResourcesUsedForGenerate = ResourceVector.UnrefinedMassUnit; // the resource it takes to generate another resource

            // HACK! unrefinedMass doesnt seem to work with HasResources()!
            ResourcesUsedForGenerate = ResourceVector.Empty;
            BaseGenerationSpeed = 3;

            // Animation
            Pivot = new Vector2(0.5f, 0.6875f);
            TotalFrames = new Point(8, 1);
            CurrentFrame = 0;
            CurrentLayer = 0;
            AnimationSpeed = 4f;
            Effect = AnimationEffect.None;
        }

        public void OnBuildingFinished() {
            BuildingFinished = true;
            HasChanged = true;
            Color = Color.White;
            Effect = AnimationEffect.PlayLooping;
            if (this is ICanHavePriority prioritizable) {
                prioritizable.HasPriority = false;
            }
        }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);
            ((IAnimating)this).Update(gameTime);

            // generate Resources once building is finished
            if (BuildingFinished) {
                ((IResourceGenerator) this).Generate(gameTime);
            }
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            ((IAnimating)this).Draw(spriteBatch);

            if (BuildingFinished) {
                ((IResourceContainer)this).DrawResources(spriteBatch);
            } else {
                ((IBuildable)this).DrawResources(spriteBatch);
            }

            ((ICanHavePriority)this).Draw(spriteBatch);
        }
    }
}