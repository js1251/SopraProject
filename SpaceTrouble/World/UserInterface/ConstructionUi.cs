using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.InputOutput;
using SpaceTrouble.InputOutput.Cursor;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.util.Tools.Assets;

// created by Jakob Sailer

namespace SpaceTrouble.World.UserInterface {
    internal sealed class ConstructionUi : UiElement {
        private SpriteFont Font { get; set; }
        private Texture2D[] ResourceIcons { get; set; }
        private Texture2D ConstructionPanelTexture { get; set; }
        private Dictionary<GameObjectEnum, MenuButton> ButtonMapping { get; }
        private Dictionary<GameObjectEnum, (Panel, Label, Label, Label)> ConstructionCards { get; }
        private Dictionary<Panel, float> CostPopUps { get; }
        private float PopUpDelay { get; }


        public ConstructionUi(Vector4 screenBounds) : base(screenBounds) {
            PopUpDelay = 0.5f;
            ButtonMapping = new Dictionary<GameObjectEnum, MenuButton>();
            ConstructionCards = new Dictionary<GameObjectEnum, (Panel, Label, Label, Label)>();
            CostPopUps = new Dictionary<Panel, float>();
        }

        internal override void LoadContent() {
            Font = Assets.Fonts.GuiFont01;
            ResourceIcons = new[] {
                Assets.Textures.Objects.EnergyIcon,
                Assets.Textures.Objects.MassIcon,
                Assets.Textures.Objects.FoodIcon
            };
            ConstructionPanelTexture = Assets.Textures.InterfaceTextures.GuiCard;

            ButtonMapping.Add(GameObjectEnum.PlatformTile, new MenuButton(Assets.Textures.Thumbnails.Platform) {ToolTip = "Platform"});
            ButtonMapping.Add(GameObjectEnum.TowerTile, new MenuButton(Assets.Textures.Thumbnails.Tower) { ToolTip = "Tower" });
            ButtonMapping.Add(GameObjectEnum.BarrackTile, new MenuButton(Assets.Textures.Thumbnails.Barrack) { ToolTip = "Barrack" });
            ButtonMapping.Add(GameObjectEnum.GeneratorTile, new MenuButton(Assets.Textures.Thumbnails.Generator) { ToolTip = "Generator" });
            ButtonMapping.Add(GameObjectEnum.ExtractorTile, new MenuButton(Assets.Textures.Thumbnails.Extractor) { ToolTip = "Extractor" });
            ButtonMapping.Add(GameObjectEnum.KitchenTile, new MenuButton(Assets.Textures.Thumbnails.Kitchen) { ToolTip = "Kitchen" });
            ButtonMapping.Add(GameObjectEnum.LaboratoryTile, new MenuButton(Assets.Textures.Thumbnails.Laboratory) { ToolTip = "Laboratory" });
            ButtonMapping.Add(GameObjectEnum.EmptyTile, new MenuButton(Assets.Textures.InterfaceTextures.ButtonDelete));

            var backgroundPanel = new Panel(new Vector4(-0.01f, 1, 1.035f, 0.95f), Vector2.Zero, new MenuElement[,] { }, Assets.Textures.InterfaceTextures.GuiConstruction);

            var constructionPanel = new Panel(new Vector4(0.01f, 0, 0.88f, 1), new Vector2(0.01f, 0.05f), new MenuElement[,] {
                {
                    CreateConstructionPanel(GameObjectEnum.PlatformTile),
                    CreateConstructionPanel(GameObjectEnum.TowerTile),
                    CreateConstructionPanel(GameObjectEnum.BarrackTile),
                    CreateConstructionPanel(GameObjectEnum.GeneratorTile),
                    CreateConstructionPanel(GameObjectEnum.ExtractorTile),
                    CreateConstructionPanel(GameObjectEnum.KitchenTile),
                    CreateConstructionPanel(GameObjectEnum.LaboratoryTile),
                }
            });

            var deleteButtonPanel = new Panel(new Vector4(0.99f, 0.13f, 0.08f, 0.2f), Vector2.Zero, new MenuElement[,] {
                {ButtonMapping[GameObjectEnum.EmptyTile]}
            });

            Panel = new Panel(ScreenBounds, new Vector2(0.01f, 0.05f), new MenuElement[,] {
                {backgroundPanel, constructionPanel, deleteButtonPanel }
            });
        }

        private Panel CreateConstructionPanel(GameObjectEnum type) {
            var buttonPanel = new Panel(new Vector4(0.5f, 0, 0.75f, 0.52f), new Vector2(0.2f, 0.2f), new MenuElement[,] {
                {ButtonMapping[type]}
            });

            var resourceIconPanel = new Panel(new Vector4(0, 0.5f, 0.35f, 1), new Vector2(0.2f, 0.1f), new MenuElement[,] {
                {new Image(ResourceIcons[0])},
                {new Image(ResourceIcons[1])},
                {new Image(ResourceIcons[2])}
            });

            var energyLabel = new Label(Font, default, "", 24f);
            var massLabel = new Label(Font, default, "", 24f);
            var foodLabel = new Label(Font, default, "", 24f);
            var constructionCostPanel = new Panel(new Vector4(1,0.5f, 0.65f, 1), new Vector2(0.1f, 0.1f), new MenuElement[,] {
                {energyLabel},
                {massLabel},
                {foodLabel},
            });

            var infoPanel = new Panel(new Vector4(0.5f, 0.92f, 0.7f, 0.4f), new Vector2(0.2f, 0.1f), new MenuElement[,] {
                {resourceIconPanel, constructionCostPanel}
            });

            var constructionPanel = new Panel(default, new Vector2(0.02f, 0.02f), new MenuElement[,] {
                {buttonPanel},
                {infoPanel}
            }, ConstructionPanelTexture);

            constructionPanel.Offset = new Vector2(0, .5f);
            ConstructionCards.Add(type, (constructionPanel, energyLabel, massLabel, foodLabel));
            return constructionPanel;
        }

        internal override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            base.Update(gameTime, inputs);
            UpdateCosts();
            HighlightSelected(gameTime, inputs);
            CheckButtonSelection(inputs);
            UpdatePopUps(gameTime, inputs);
        }

        private void UpdateCosts() {
            foreach (var (type, (_, energy, mass, food)) in ConstructionCards) {
                var tile = WorldGameState.ObjectManager.CopyObject(type);
                if (tile is IBuildable buildable) {
                    energy.Text = buildable.RequiredResources.Energy.ToString();
                    mass.Text = buildable.RequiredResources.Mass.ToString();
                    food.Text = buildable.RequiredResources.Food.ToString();
                }
            }
        }

        public void CreatePopUp(ResourceVector cost) {
            var popUpsInSingleFrame = 0;
            if (cost.Energy > 0) {
                CostPopUps.Add(CreatePopUpHelper(cost.Energy, 0), 0);
                popUpsInSingleFrame++;
            }
            if (cost.Mass > 0) {
                CostPopUps.Add(CreatePopUpHelper(cost.Mass, 1), 0.5f * popUpsInSingleFrame * PopUpDelay);
                popUpsInSingleFrame++;
            }
            if (cost.Food > 0) {
                CostPopUps.Add(CreatePopUpHelper(cost.Food, 2), 0.5f * popUpsInSingleFrame * PopUpDelay);
            }
        }

        private Panel CreatePopUpHelper(int resource, int resourceIndex) {
            if (resource <= 0) {
                return null;
            }
            return new Panel(new Vector4(0f, 0.8f, 0.06f, 0.06f), new Vector2(0.02f, 0.02f), new MenuElement[,] {
                {new Label(Font, Color.White, "-" + resource, 32f), new Image(ResourceIcons[resourceIndex])}
            });
        }

        private void UpdatePopUps(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            foreach (var (popUp, delay) in CostPopUps.ToList()) {
                CostPopUps[popUp] -= (float) gameTime.ElapsedGameTime.TotalSeconds;
                if (delay <= 0) {
                    popUp.Update(inputs);

                    popUp.Offset = new Vector2(0, popUp.Offset.Y - (float)gameTime.ElapsedGameTime.TotalSeconds * 2f);
                    popUp.Alpha -= (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;

                    if (popUp.Alpha <= 0) {
                        CostPopUps.Remove(popUp);
                    }
                }
            }
        }

        private void HighlightSelected(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            if (!inputs.TryGetValue(ActionType.MouseMoved, out var input)) {
                return;
            }

            var panelOffset = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (var (type, (panel, _, _, _)) in ConstructionCards) {
                var triggerBounds = new Rectangle(panel.mBounds.X, panel.mBounds.Y - (int) (panel.mBounds.Height * panel.Offset.Y), panel.mBounds.Width, panel.mBounds.Height);
                
                if (triggerBounds.Contains(input.Origin)) {
                    panel.Offset = new Vector2(0, Math.Clamp(panel.Offset.Y - panelOffset * 4f, 0.05f, 0.5f));
                } else if (type != WorldGameState.BuildingMenu.mLockedBuilding) {
                    panel.Offset = new Vector2(0, Math.Clamp(panel.Offset.Y + panelOffset * 2f, 0, 0.5f));
                }
            }
        }

        private void CheckButtonSelection(Dictionary<ActionType, InputAction> inputs) {
            if (inputs.ContainsKey(ActionType.MouseRightClick)) {
                WorldGameState.BuildingMenu.mLockedBuilding = null;
                CursorOverlay.CursorState(false);
                return;
            }

            foreach (var (type, button) in ButtonMapping) {
                if (button.GetPushState(true)) {
                    if (WorldGameState.BuildingMenu.mLockedBuilding == GameObjectEnum.EmptyTile) {
                        break;
                    }

                    WorldGameState.BuildingMenu.mLockedBuilding = type;

                    if (type == GameObjectEnum.EmptyTile) {
                        CursorOverlay.CursorState(true);
                    }
                    break;
                }
            }
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
            foreach (var (popUp, delay) in CostPopUps) {
                if (delay <= 0) {
                    popUp.Draw(spriteBatch);
                }
            }
        }
    }
}