using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.Menu.Statistics {
    internal sealed class StatsMenuState : GameState.GameState {
        private Panel Panel { get; set; }
        private StatisticsPanel StatisticPanel { get; }
        private AchievementsPanel AchievementsPanel { get; }
        private MenuButton BackButton { get; set; }

        public StatsMenuState(string stateName) : base(stateName) {
            StatisticPanel = new StatisticsPanel(new Vector4(0.05f, 0.5f, 0.46f, 0.7f));
            AchievementsPanel = new AchievementsPanel(new Vector4(0.95f, 0.65f, 0.48f, 0.8f));
        }

        internal override void Initialize() {
        }

        // Any time the StatsMenu is activated update all values
        internal override void Activated(GameTime gameTime, HashSet<string> messages) {
            base.Activated(gameTime, messages);
            UpdateValues();
        }

        // reads all values from the statsManager
        private void UpdateValues() {
            StatisticPanel.UpdateValues();
            //AchievementsPanel.UpdateValues();
        }

        // create the basic layout of the menu
        internal override void LoadContent() {
            // a button to return to the main menu
            BackButton = new MenuButton(Assets.Textures.InterfaceTextures.Button, Assets.Fonts.ButtonFont, "Back");
            var backButtonPanel = new Panel(new Vector4(0.2f,0.95f,0.2f,0.1f), Vector2.Zero, new MenuElement[,] {
                {BackButton},
            });

            var headerPanel = new Panel(new Vector4(0.11f,0,0.3f,0.1f), Vector2.Zero, new MenuElement[,] {
                { new Label(Assets.Fonts.GuiFont01, default, "Stats & Achievements", 28f) }
            });

            // the over-arcing panel containing the statistic panel and back button
            Panel = new Panel(new Vector4(0.5f, 0.5f, 0.95f, 0.8f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {headerPanel},
                {StatisticPanel.CreatePanel()},
                {AchievementsPanel.CreatePanel()},
                {backButtonPanel}
            }, Assets.Textures.InterfaceTextures.GuiAchievements);
        }

        internal override void CheckForStateChanges(GameStateManager stateManager, Dictionary<ActionType, InputAction> inputs) {
            if (BackButton.GetPushState(true)) {
                stateManager.RemoveActiveGameState();
            }

            base.CheckForStateChanges(stateManager, inputs);
        }


        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            AchievementsPanel.UpdateValues(); // since bars reset on every frame. TODO: maybe fix?
            Panel.Update(inputs);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            Panel.Draw(spriteBatch);
            spriteBatch.End();
        }

        // Returns a formatted string from an enum "MinionsKilled -> Minions Killed:"
        internal static string EnumToString(string enumString) {
            var chars = enumString.ToCharArray();
            var output = "";
            var index = 0;
            while (index < chars.Length) {
                if (char.IsDigit(chars[index])) {
                    output += " ";
                    while (index < chars.Length && char.IsDigit(chars[index])) {
                        output += chars[index];
                        index++;
                    }

                    if (index >= chars.Length) {
                        break;
                    }
                    output += " ";
                    output += chars[index];
                } else if (char.IsUpper(chars[index])) {
                    output += " " + chars[index];
                } else {
                    output += chars[index];
                }
                index++;
            }

            return output + ":";
        }
    }
}