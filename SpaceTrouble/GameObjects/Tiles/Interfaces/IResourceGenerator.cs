using Microsoft.Xna.Framework;
using SpaceTrouble.Menu.Statistics;
using SpaceTrouble.util.DataStructures;

namespace SpaceTrouble.GameObjects.Tiles.Interfaces {
    internal interface IResourceGenerator : IResourceContainer {
        public ResourceVector GeneratedResources { get; }
        public ResourceVector ResourcesUsedForGenerate { get; }
        public double BaseGenerationSpeed { get; }
        public double TimeSinceLastGenerate { get; set; }

        public double GetGenerationSpeed() {
            return BaseGenerationSpeed;
        }

        public bool HasSpaceForGeneratedResource() {
            return (Resources + GeneratedResources).AllLessOrEqualThan(ResourceCapacity);
        }

        public void Generate(GameTime gameTime) {
            TimeSinceLastGenerate += gameTime.ElapsedGameTime.TotalSeconds;

            if (TimeSinceLastGenerate >= GetGenerationSpeed()) {
                TimeSinceLastGenerate -= GetGenerationSpeed();

                if (HasResources(ResourcesUsedForGenerate) && HasSpaceForGeneratedResource())
                {
                    SpaceTrouble.StatsManager.AddValue(Statistic.ResourceGenerated, 1);
                    GetResources(ResourcesUsedForGenerate); // removes one resource that's required to generate another
                    GeneratedResources.TransferResources(this); // creates a new generated resource and transfers it to the container
                }
            }
        }
    }
}