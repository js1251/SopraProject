using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.util.Tools.Assets;

// created by Jakob Sailer

namespace SpaceTrouble.World.UserInterface {
    internal sealed class ResourceUi : UiElement {
        private ConstructionUi ConstructionUi { get; }
        private Label[] Resources { get; set; }
        private ResourceVector OldConstructionCost { get; set; }
        private Label MinionLabel { get; set; }
        public ResourceUi(Vector4 screenBounds, ConstructionUi constructionUi) : base(screenBounds) {
            ConstructionUi = constructionUi;
            OldConstructionCost = ResourceVector.Empty;
        }

        internal override void LoadContent() {
            Resources = new[] {
                new Label(Assets.Fonts.GuiFont01, default, "", 20f),
                new Label(Assets.Fonts.GuiFont01, default, "", 20f),
                new Label(Assets.Fonts.GuiFont01, default, "", 20f),
                new Label(Assets.Fonts.GuiFont01, default, "", 14f),
                new Label(Assets.Fonts.GuiFont01, default, "", 14f),
                new Label(Assets.Fonts.GuiFont01, default, "", 14f)
            };

            var iconPanel = new Panel(new Vector4(0.07f, 0.3f, 0.1f, 0.35f), new Vector2(0.15f, 0.05f), new MenuElement[,] {
                {new Image(Assets.Textures.Objects.EnergyIcon)},
                {new Image(Assets.Textures.Objects.MassIcon)},
                {new Image(Assets.Textures.Objects.FoodIcon)}
            });

            var namePanel = new Panel(new Vector4(0.26f, 0.3f, 0.2f, 0.35f), new Vector2(0.1f, 0), new MenuElement[,] {
                {new Label(Assets.Fonts.GuiFont01, default, "Energy :", 14f)},
                {new Label(Assets.Fonts.GuiFont01, default, "Mass   :", 14f)},
                {new Label(Assets.Fonts.GuiFont01, default, "Food   :", 14f)}
            });

            var availablePanel = new Panel(new Vector4(0.62f, 0.3f, 0.3f, 0.35f),  new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {Resources[0]},
                {Resources[1]},
                {Resources[2]}
            });

            var promisedPanel = new Panel(new Vector4(0.94f, 0.3f, 0.3f, 0.35f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {Resources[3]},
                {Resources[4]},
                {Resources[5]}
            });

            MinionLabel = new Label(Assets.Fonts.GuiFont01, default, "", 22f);
            var minionIcon = new Panel(new Vector4(0.1f,0.95f,0.25f,0.4f), Vector2.Zero, new MenuElement[,] {}, Assets.Textures.Thumbnails.Minion);
            var minionCount = new Panel(new Vector4(0.65f, 0.85f, 0.4f, 0.25f), Vector2.Zero, new MenuElement[,] { {MinionLabel} });

            Panel = new Panel(ScreenBounds, new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {iconPanel, namePanel, availablePanel, promisedPanel},
                {minionIcon, minionCount, null, null}
            }, Assets.Textures.InterfaceTextures.GuiResources);
        }

        internal override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            base.Update(gameTime, inputs);
            var (available, /*_,*/ construction) = GetAvailableResources();
            if (!construction.AllLessOrEqualThan(OldConstructionCost)) {
                ConstructionUi.CreatePopUp(construction - OldConstructionCost);
            }

            OldConstructionCost = construction;

            Resources[0].Text = available.Energy.ToString();
            Resources[1].Text = available.Mass.ToString();
            Resources[2].Text = available.Food.ToString();
            Resources[3].Text = construction.Energy > 0 ? "(-" + construction.Energy + ")" : "";
            Resources[4].Text = construction.Mass > 0 ? "(-" + construction.Mass + ")" : "";
            Resources[5].Text = construction.Food > 0 ? "(-" + construction.Food + ")" : "";

            var minionCount = WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.Minion).Count;
            var barrackCount = WorldGameState.ObjectManager.GetAllObjects(ObjectProperty.RequiresSpawnResources).Count;
            var maxMinionCount = barrackCount * WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.Miscellaneous, DifficultyAttribute.MaxMinionPerBarrack);

            MinionLabel.Text = minionCount + " / " + maxMinionCount;
        }

        private static (ResourceVector/*, ResourceVector*/, ResourceVector) GetAvailableResources() {
            var available = ResourceVector.Empty;
            var promised = ResourceVector.Empty;
            var construction = ResourceVector.Empty;

            foreach (var gameObject in WorldGameState.ObjectManager.GetAllObjects(ObjectProperty.ResourceContainer)) {
                if (!(gameObject is IResourceContainer container)) {
                    continue;
                }

                available += container.Resources;
                promised += container.PromisedResources;
            }

            foreach (var gameObject in WorldGameState.ObjectManager.GetAllObjects(ObjectProperty.Buildable)) {
                if (gameObject is IBuildable buildable) {
                    construction += buildable.RequiredResources - buildable.OnTheWayResources;
                }
            }

            return (available/*, promised*/, construction);
        }
    }
}