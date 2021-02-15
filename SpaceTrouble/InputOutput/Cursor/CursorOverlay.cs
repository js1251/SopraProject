using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.InputOutput.Cursor {
    internal sealed class CursorOverlay : GameStateOverlay {
        private Vector2 mPosition;
        private static bool IsDeleteSelected { get; set; }

        public CursorOverlay(string overlayName/*, int priority*/) : base(overlayName/*, priority*/) {
        }

        internal override void LoadContent() {
        }

        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            if (inputs.TryGetValue(ActionType.MouseMoved, out var input)) {
                mPosition = input.Origin;
            }
        }

        public static void CursorState(bool cursorState) {
            IsDeleteSelected = cursorState;
        }

        public override void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(IsDeleteSelected ? Assets.Textures.InterfaceTextures.CursorDelete : Assets.Textures.InterfaceTextures.CursorRegular, mPosition, Color.White);
        }
    }
}