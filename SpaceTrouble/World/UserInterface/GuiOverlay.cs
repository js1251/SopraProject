using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;

// created by Jakob Sailer

namespace SpaceTrouble.World.UserInterface {
    internal sealed class GuiOverlay : GameStateOverlay {
        private List<UiElement> UiElements { get; }

        public GuiOverlay(string overlayName) : base(overlayName) {
            var constructionUi = new ConstructionUi(new Vector4(0.375f, 1, 0.6f, 0.2f));
            UiElements = new List<UiElement> {
                constructionUi,
                new ProgressUi(new Vector4(0.5f, 0, 0.15f, 0.08f)),
                new ResourceUi(new Vector4(0, 1, 0.15f, 0.2f), constructionUi),
                new MinionTasksUi(new Vector4(1, 1, 0.25f, 0.2f)),
                new SettingsUi(new Vector4(0.73f, 0.99f, 0.064f, 0.11f))
            };
        }

        internal override void LoadContent() {
            foreach (var uiElement in UiElements) {
                uiElement.LoadContent();
            }
        }

        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            foreach (var uiElement in UiElements) {
                uiElement.Update(gameTime, inputs);
                if (inputs.ContainsKey(ActionType.Pause) || inputs.ContainsKey(ActionType.StateBackAction))
                {
                    SpaceTrouble.SoundManager.PlaySound(Sound.OpenMenu);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch) {
            foreach (var uiElement in UiElements) {
                uiElement.Draw(spriteBatch);
            }
        }
    }
}