using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.World {
    internal sealed class GameFinishedOverlay : GameStateOverlay {
        private Panel GameWonPanel { get; set; }
        private Panel GameLostPanel { get; set; }
        private Panel GamePausedPanel { get; set; }
        public GameFinishedOverlay(string overlayName/*, int priority*/, bool active = true) : base(overlayName/*, priority*/, active) {
        }

        internal override void LoadContent() {
            GameWonPanel = new Panel(new Vector4(0.5f, 0.5f, 0.8f,0.8f), Vector2.Zero, new MenuElement[,] { }, Assets.Textures.InterfaceTextures.WonTitle);
            GameLostPanel = new Panel(new Vector4(0.5f, 0.5f, 0.8f, 0.8f), Vector2.Zero, new MenuElement[,] { }, Assets.Textures.InterfaceTextures.LostTitle);
            GamePausedPanel = new Panel(new Vector4(0.5f, 0.5f, 0.8f, 0.8f), Vector2.Zero, new MenuElement[,] { }, Assets.Textures.InterfaceTextures.PausedTitle);
        }

        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            GameWonPanel.Update(inputs);
            GameLostPanel.Update(inputs);
            GamePausedPanel.Update(inputs);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            if (WorldGameState.GameMaster.GameWon) {
                GameWonPanel.Draw(spriteBatch);
            } else if (WorldGameState.GameMaster.GameLost) {
                GameLostPanel.Draw(spriteBatch);
            } else if (WorldGameState.IsPaused) {
                GamePausedPanel.Draw(spriteBatch);
            }
        }
    }
}
