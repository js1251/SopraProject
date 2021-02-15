using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.GameObjects.Tiles.Interfaces {
    internal interface IBuildable {
        public ResourceVector OnTheWayResources { get; set; }
        public ResourceVector RequiredResources { get; set; }
        public bool BuildingFinished { get; set; }
        public bool HasChanged { get; set; }
        public Color Color { set; }
        public Vector2 WorldPosition { get; }

        public bool NeedsResource(ResourceVector resource) {
            return (OnTheWayResources + resource).AllLessOrEqualThan(RequiredResources);
        }

        public ResourceVector AddResource(ResourceVector resource) {
            return AddResourceHelper(resource);
        }

        public ResourceVector NeededResources() {
            return RequiredResources - OnTheWayResources;
        }

        /// <summary>
        /// Helper method for a workaround that substitutes base.AddResource() not being possible for interfaces. Always override AddResource() not this;
        /// Then call this method from the override as the "base" method
        /// </summary>
        public ResourceVector AddResourceHelper(ResourceVector resource) {
            ResourceVector leftoverResources;
            (RequiredResources, leftoverResources) = RequiredResources.SubtractResources(resource);

            // Note: what if resources are put in directly (e.g through force-built)
            if ((resource - leftoverResources).AllLessOrEqualThan(OnTheWayResources)) {
                OnTheWayResources -= resource - leftoverResources;
            }

            if (RequiredResources.IsEmpty() && !BuildingFinished) {
                OnBuildingFinished();
                OnTheWayResources = ResourceVector.Empty;
            }

            return leftoverResources;
        }

        public virtual void OnBuildingFinished() {
            BuildingFinished = true;
            HasChanged = true;
            Color = Color.White;
            if (this is ICanHavePriority prioritizable) {
                prioritizable.HasPriority = false;
            }
        }

        public void DrawResources(SpriteBatch spriteBatch) {
            const float scale = 0.3f;
            var position = new Vector2(Assets.Textures.Objects.MassIcon.Width / 2f, Assets.Textures.Objects.MassIcon.Height / 2f) * scale;
            var offset = new Vector2(48,24);

            var drawPos = WorldPosition - position + new Vector2(0, -0.334f) * offset;
            DrawResource(spriteBatch, drawPos, 1); // energy

            drawPos = WorldPosition - position + new Vector2(-0.167f, -0.167f) * offset;
            DrawResource(spriteBatch, drawPos, 0); // mass

            drawPos = WorldPosition - position + new Vector2(-0.334f, 0) * offset;
            DrawResource(spriteBatch, drawPos, 2); // food
        }

        private void DrawResource(SpriteBatch spriteBatch, Vector2 baseDrawPos, int type) {
            const float scale = 0.3f;
            const float fontScale = 0.18f;
            const int countLimit = 8;

            Texture2D icon;
            int resources;

            if (type == 0) {
                icon = Assets.Textures.Objects.MassIcon;
                resources = RequiredResources.Mass;
            } else if (type == 1) {
                icon = Assets.Textures.Objects.EnergyIcon;
                resources = RequiredResources.Energy;
            } else if (type == 2) {
                icon = Assets.Textures.Objects.FoodIcon;
                resources = RequiredResources.Food;
            } else {
                return;
            }

            for (var i = 0; i < resources; i++) {
                spriteBatch.Draw(icon, baseDrawPos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                if (i == countLimit - 1 && i + 1 < resources) {
                    var text = "+" + (resources - i - 1);
                    spriteBatch.DrawString(Assets.Fonts.GuiFont01, text, baseDrawPos + Vector2.UnitY * 2.5f, Color.LightCoral, 0, Vector2.Zero, fontScale, SpriteEffects.None, 0);
                    break;
                }

                baseDrawPos += 2 * new Vector2(1,0.5f);
            }
        }
    }
}