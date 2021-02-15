using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.GameObjects.Tiles.Interfaces {
    public interface IResourceContainer {
        public ResourceVector Resources { get; set; }
        public ResourceVector ResourceCapacity { get; }
        public ResourceVector PromisedResources { get; set; }
        public Vector2 WorldPosition { get; }

        public /*ResourceVector*/ void AddResource(ResourceVector resource) {
            /*return*/ resource.TransferResources(this);
        }

        public bool HasResources(ResourceVector request) {
            return request.AllLessOrEqualThan(Resources - PromisedResources);
        }

        public ResourceVector HasAny(ResourceVector request) {
            return (Resources - PromisedResources).HasAny(request);
        }

        public ResourceVector GetResources(ResourceVector request) {
            var (newContent, notFulfilled) = Resources.SubtractResources(request);
            Resources = newContent;

            var givenResources = request - notFulfilled;

            // Note: what if resources are pulled directly for some reason -> extra check
            if (givenResources.AllLessOrEqualThan(PromisedResources)) {
                PromisedResources -= givenResources;
            }

            return givenResources;
        }

        public bool ContainerIsEmpty() {
            return Resources.IsEmpty();
        }

        public void DrawResources(SpriteBatch spriteBatch) {

            const float scale = 0.3f;
            var position = new Vector2(Assets.Textures.Objects.MassIcon.Width / 2f, Assets.Textures.Objects.MassIcon.Height / 2f) * scale;
            position += new Vector2(0, 4);

            var drawPos = WorldPosition - position + new Vector2(0, 7);
            for (var i = 0; i < Resources.Mass; i += 3) {
                spriteBatch.Draw(Assets.Textures.Objects.MassIcon, drawPos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                drawPos -= Vector2.UnitY;
            }

            drawPos = WorldPosition - position + new Vector2(-14, 0);
            for (var i = 0; i < Resources.Energy; i += 3) {
                spriteBatch.Draw(Assets.Textures.Objects.EnergyIcon, drawPos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                drawPos -= Vector2.UnitY;
            }

            drawPos = WorldPosition - position + new Vector2(14, 0);
            for (var i = 0; i < Resources.Food; i += 3) {
                spriteBatch.Draw(Assets.Textures.Objects.FoodIcon, drawPos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                drawPos -= Vector2.UnitY;
            }
        }
    }
}