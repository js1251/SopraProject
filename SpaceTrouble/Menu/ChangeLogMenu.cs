using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.Menu {
    internal sealed class ChangeLogMenu : GameState.GameState {
        private Panel Panel { get; set; }
        private MenuButton BackButton { get; set; }
        private Dictionary<string, List<string>> ChangeLog { get; } 
        public ChangeLogMenu(string stateName) : base(stateName) {
            ChangeLog = new Dictionary<string, List<string>>();
        }

        internal override void Initialize() {
        }

        internal override void LoadContent() {
            LoadChangeLogs();

            var changeLogPanel = new Panel(new Vector4(0.54f,0.35f,0.8f,0.75f), new Vector2(0.05f, 0.05f), GetMenuElements());

            BackButton = new MenuButton(Assets.Textures.InterfaceTextures.Button, Assets.Fonts.ButtonFont, "Back");
            var backButtonPanel = new Panel(new Vector4(0.5f, 0.93f, 0.3f, 0.1f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {BackButton}
            });

            Panel = new Panel(new Vector4(0.5f,0.5f,0.6f,0.9f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {changeLogPanel},
                {backButtonPanel}
            }, Assets.Textures.InterfaceTextures.GuiMenu);
        }

        internal override void CheckForStateChanges(GameStateManager stateManager, Dictionary<ActionType, InputAction> inputs) {
            if (BackButton.GetPushState(true)) {
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

        private MenuElement[,] GetMenuElements() {
            var count = ChangeLog.Values.SelectMany(list => list).Count();

            var versionEntries = new MenuElement[count, 1];
            var changeLogEntries = new MenuElement[count, 1];

            var index = 0;
            foreach (var (version, entries) in ChangeLog) {
                versionEntries[index, 0] = new Label(Assets.Fonts.GuiFont01, default, "Version " + version);
                index++;

                for (var i = 0; i < entries.Count - 1; i++) {
                    versionEntries[index + i, 0] = null;
                }

                index += entries.Count - 1;
            }

            index = 0;
            foreach (var (_, entries) in ChangeLog) {
                foreach (var entry in entries) {
                    changeLogEntries[index, 0] = new Label(Assets.Fonts.GuiFont01, default, "> " + entry, 12f);
                    index++;
                }
            }

            var versionPanel = new Panel(new Vector4(0, 0, 0.2f, 1), new Vector2(0.05f, 0.05f), versionEntries);
            var entriesPanel = new Panel(new Vector4(1, 0, 0.8f, 1), new Vector2(0.05f, 0.05f), changeLogEntries);

            return new MenuElement[,] {{versionPanel, entriesPanel}};
        }

        private void LoadChangeLogs() {
            AddToChangeLog("1.0", "Initial release and completion of Uni-assignment");
            AddToChangeLog("1.1", "Added changelog menu");
            AddToChangeLog("1.1", "Buttons no longer NEED a background image");
            AddToChangeLog("1.1", "Fixed towers not shooting at enemies in certain situation");
            AddToChangeLog("1.1", "Fixed difficulty only affecting wave time");
        }

        private void AddToChangeLog(string version, string entry) {
            if (!ChangeLog.ContainsKey(version)) {
                ChangeLog.Add(version, new List<string>());
            }
            ChangeLog[version].Add(entry);
        }
    }
}
