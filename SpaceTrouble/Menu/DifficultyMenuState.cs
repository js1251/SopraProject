using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.Menu.Statistics;
using SpaceTrouble.util.Tools.Assets;
using SpaceTrouble.World;

namespace SpaceTrouble.Menu {
    internal sealed class DifficultyMenuState : GameState.GameState {
        private Panel Panel { get; set; }
        private MenuButton mEasyButton;
        private MenuButton mMediumButton;
        private MenuButton mHardButton;
        private MenuButton mLegendaryButton;
        private MenuButton mBackButton;

        public DifficultyMenuState(string stateName) : base(stateName) {
        }

        internal override void Initialize() {
        }

        internal override void LoadContent() {
            var buttonTexture = Assets.Textures.InterfaceTextures.Button;
            var font = Assets.Fonts.ButtonFont;

            mEasyButton = new MenuButton(buttonTexture, font, "Easy");
            mMediumButton = new MenuButton(buttonTexture, font, "Medium");
            mHardButton = new MenuButton(buttonTexture, font, "Hard");
            mLegendaryButton = new MenuButton(buttonTexture, font, "Legendary");
            mBackButton = new MenuButton(buttonTexture, font, "Back");

            var buttonPanel = new Panel(new Vector4(0.5f, 0.6f, 0.7f, 0.8f), new Vector2(0.05f, 0.025f), new MenuElement[,] {
                {mEasyButton},
                {mMediumButton},
                {mHardButton},
                {mLegendaryButton},
                {mBackButton}
            });

            Panel = new Panel(new Vector4(0.5f, 0.5f, 0.3f, 0.6f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {buttonPanel},
            }, Assets.Textures.InterfaceTextures.GuiMenu);
        }

        internal override void CheckForStateChanges(GameStateManager stateManager, Dictionary<ActionType, InputAction> inputs) {
            if (mBackButton.GetPushState(true)) {
                stateManager.RemoveActiveGameState();
                return;
            }

            if (mEasyButton.GetPushState(true)) {
                WorldGameState.DifficultyManager.Difficulty = DifficultyEnum.Easy;

            } else if (mMediumButton.GetPushState(true)) {
                WorldGameState.DifficultyManager.Difficulty = DifficultyEnum.Normal;

            } else if (mHardButton.GetPushState(true)) {
                WorldGameState.DifficultyManager.Difficulty = DifficultyEnum.Hard;

            } else if (mLegendaryButton.GetPushState(true)) {
                WorldGameState.DifficultyManager.Difficulty = DifficultyEnum.Legendary;
            } else {
                base.CheckForStateChanges(stateManager, inputs);
                return;
            }

            SpaceTrouble.StatsManager.AddValue(Statistic.GamesStarted, 1);
            stateManager.RemoveActiveGameState();
            stateManager.ActivateGameState("WorldGame", false, "start new game");
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