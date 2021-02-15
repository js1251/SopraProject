using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Tiles {
    internal sealed class StockTile : PlatformTile, IResourceContainer {
        [JsonProperty] public ResourceVector Resources { get; set; }
        [JsonProperty] public ResourceVector ResourceCapacity { get; }
        [JsonProperty] public ResourceVector PromisedResources { get; set; }
        [JsonProperty] public override bool IsWalkable => true;

        public StockTile() {
            RequiredResources = new ResourceVector(1, 1, 0);
            var startResource = WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.Miscellaneous,
                DifficultyAttribute.StartingResources);
            Resources = new ResourceVector(startResource, startResource, startResource);
            ResourceCapacity = Resources;
        }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);

            // once the stock-Tile is empty and served its purpose replace it with a simple platformTile
            if (Resources.IsEmpty()) {
                WorldGameState.ObjectManager.CreateTile(CoordinateManager.WorldToTile(WorldPosition), GameObjectEnum.PlatformTile, true);
            }
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
            if (!BuildingFinished) {
                ((IBuildable) this).DrawResources(spriteBatch);
            } else {
                ((IResourceContainer)this).DrawResources(spriteBatch);
            }
        }
    }
}