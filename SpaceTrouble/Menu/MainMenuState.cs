using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.SaveGameManager;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;
using SpaceTrouble.World;

namespace SpaceTrouble.Menu {
    internal sealed class MainMenuState : GameState.GameState {
        private Panel Panel { get; set; }
        private Panel LogoPanel { get; set; }
        private MenuButton mContinueButton;
        private MenuButton mNewGameButton;
        private MenuButton mTutorialsButton;
        private MenuButton mSettingsButton;
        private MenuButton mStatsButton;
        private MenuButton mTechDemoButton;
        private MenuButton mQuitButton;


        internal MainMenuState(string stateName) : base(stateName) {
        }

        internal override void Initialize() {
        }

        internal override void CheckForStateChanges(GameStateManager stateManager, Dictionary<ActionType, InputAction> inputs) {
            if (mContinueButton.GetPushState(true)) {
                if (SaveLoadManager.SavedGameExists()) {
                    if (WorldGameState.GameIsRunning)
                    {
                        Console.WriteLine("continue message");
                        stateManager.ActivateGameState("WorldGame", false, "continue");
                    } else
                    {
                        Console.WriteLine("load message");
                        stateManager.ActivateGameState("WorldGame", false, "load");
                    }
                }
            } else if (mNewGameButton.GetPushState(true)) {
                stateManager.ActivateGameState("DifficultyMenu");

            } else if (mSettingsButton.GetPushState(true)) {
                stateManager.ActivateGameState("Settings");

            } else if (mStatsButton.GetPushState(true)) {
                stateManager.ActivateGameState("StatsMenu");

            } else if (mQuitButton.GetPushState(true)) {
                stateManager.RemoveActiveGameState();

            } else if (mTechDemoButton.GetPushState(true)) {
                stateManager.ActivateGameState("WorldGame", false, "techdemo");

            } else if (mTutorialsButton.GetPushState(true)) {
                stateManager.ActivateGameState("Tutorials");
            }

            base.CheckForStateChanges(stateManager, inputs);
        }


        internal override void LoadContent() {
            var buttonTexture = Assets.Textures.InterfaceTextures.Button;
            var font = Assets.Fonts.ButtonFont;

            mContinueButton = new MenuButton(buttonTexture, font, "Continue");
            mNewGameButton = new MenuButton(buttonTexture, font, "New Game");
            mTutorialsButton = new MenuButton(buttonTexture, font, "Tutorial");
            mSettingsButton = new MenuButton(buttonTexture, font, "Settings");
            mStatsButton = new MenuButton(buttonTexture, font, "Statistics");
            mTechDemoButton = new MenuButton(buttonTexture, font, "Techdemo");
            mQuitButton = new MenuButton(buttonTexture, font, "Quit");


            var buttonPanel = new Panel(new Vector4(0.5f, 0.6f, 0.7f, 0.8f), new Vector2(0.05f, 0.025f), new MenuElement[,] {
                 {mContinueButton},
                 {mNewGameButton},
                 {mTutorialsButton},
                 {mSettingsButton},
                 {mStatsButton},
                 {mTechDemoButton},
                 {mQuitButton}
            });

            var backgroundPanel = new Panel(new Vector4(0.5f, 1, 0.8f, 0.75f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {buttonPanel}
            }, Assets.Textures.InterfaceTextures.GuiMenu);

            LogoPanel = new Panel(new Vector4(0.5f, 0, 1.5f, 0.3f), Vector2.Zero, new MenuElement[,] { }, Assets.Textures.InterfaceTextures.GameTitle);

            Panel = new Panel(new Vector4(0.5f, 0.5f, 0.3f, 0.8f), Vector2.Zero, new MenuElement[,] {
                {backgroundPanel},
                {LogoPanel}
            });
        }

        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            mContinueButton.TextColor = SaveLoadManager.SavedGameExists() ? default : Color.Gray;

            LogoPanel.Scale = MathExtension.Oscillation((float)gameTime.TotalGameTime.TotalSeconds, 3f, 0.9f, 1.5f);
            LogoPanel.Rotation = MathExtension.Oscillation((float)gameTime.TotalGameTime.TotalSeconds, 3f, -0.1f, 0.1f);

            Panel.Update(inputs);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            Panel.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}