using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.World.UserInterface {
    internal sealed class SettingsUi : UiElement{

        private MenuButton TowerRangeModeButton { get; set; }
        private MenuButton TowerAmmunitionModeButton { get; set; }
        private MenuButton EmptyTileButton { get; set; }
        public SettingsUi(Vector4 screenBounds) : base(screenBounds) {
        }

        internal override void LoadContent() {
            var buttonTexture = Assets.Textures.InterfaceTextures.Button;
            var font = Assets.Fonts.ButtonFont;

            TowerRangeModeButton = new MenuButton(buttonTexture, font, "rang", default, 10f) {ToolTip = "Cycle TowerRange Mode"};
            TowerAmmunitionModeButton = new MenuButton(buttonTexture, font, "ammo", default, 10f) { ToolTip = "Cycle TowerAmmo Mode" };
            EmptyTileButton = new MenuButton(buttonTexture, font, "show", default, 10f) { ToolTip = "Toggle World Grid" };

            Panel = new Panel(ScreenBounds, new Vector2(0.125f, 0.125f), new MenuElement[,] {
                {TowerRangeModeButton, TowerAmmunitionModeButton},
                {EmptyTileButton, new MenuButton(buttonTexture) {ToolTip = "Does nothing :)"}},
            });
        }

        internal override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            if (TowerRangeModeButton.GetPushState(true)) {
                WorldGameState.Highlighting.TowerRange.Mode++;
            } else if (TowerRangeModeButton.GetPushState(true, ActionType.MouseRightClick)) {
                WorldGameState.Highlighting.TowerRange.Mode--;
            } else if (TowerAmmunitionModeButton.GetPushState(true)) {
                WorldGameState.Highlighting.TowerAmmunition.Mode++;
            } else if (TowerAmmunitionModeButton.GetPushState(true, ActionType.MouseRightClick)) {
                WorldGameState.Highlighting.TowerAmmunition.Mode--;
            } else if (EmptyTileButton.GetPushState(true)) {
                WorldGameState.Highlighting.EmptyTileEffect.AlphaOverride = !WorldGameState.Highlighting.EmptyTileEffect.AlphaOverride;
            }

            EmptyTileButton.Text = WorldGameState.Highlighting.EmptyTileEffect.AlphaOverride ? "Hide" : "Show";
            TowerRangeModeButton.Text = WorldGameState.Highlighting.TowerRange.Mode.ToString();
            TowerAmmunitionModeButton.Text = WorldGameState.Highlighting.TowerAmmunition.Mode.ToString();

            base.Update(gameTime, inputs);
        }
    }
}
