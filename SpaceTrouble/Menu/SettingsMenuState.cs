using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.Menu {
    internal sealed class SettingsMenuState : GameState.GameState {
        private Panel Panel { get; set; }
        private MenuButton mAudioButton;
        private MenuButton mGraphicsButton;
        private MenuButton mBackButton;

        internal SettingsMenuState(string stateName) : base(stateName) {
        }

        internal override void Initialize() {
        }

        internal override void LoadContent() {
            var buttonTexture = Assets.Textures.InterfaceTextures.Button;
            var font = Assets.Fonts.ButtonFont;

            mAudioButton = new MenuButton(buttonTexture, font, "Audio");
            mGraphicsButton = new MenuButton(buttonTexture, font, "Graphics");
            mBackButton = new MenuButton(buttonTexture, font, "Back");

            var buttonPanel = new Panel(new Vector4(0.5f, 0.6f, 0.7f, 0.8f), new Vector2(0.05f, 0.025f), new MenuElement[,] {
                {mAudioButton},
                {mGraphicsButton},
                {mBackButton}
            });

            Panel = new Panel(new Vector4(0.5f, 0.5f, 0.3f, 0.4f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {buttonPanel},
            }, Assets.Textures.InterfaceTextures.GuiMenu);
        }

        internal override void CheckForStateChanges(GameStateManager stateManager, Dictionary<ActionType, InputAction> inputs) {
            if (mAudioButton.GetPushState(true)) {
                stateManager.ActivateGameState("AudioMenu");
            }

            if (mGraphicsButton.GetPushState(true)) {
                stateManager.ActivateGameState("GraphicsMenu");
            }

            if (mBackButton.GetPushState(true)) {
                stateManager.RemoveActiveGameState();
            }

            base.CheckForStateChanges(stateManager, inputs);
        }

        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            Panel.Update(inputs);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            Panel.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}