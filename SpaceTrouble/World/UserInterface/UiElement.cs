using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;

// created by Jakob Sailer

namespace SpaceTrouble.World.UserInterface {
    internal abstract class UiElement {
        protected Panel Panel { get; set; }
        protected Vector4 ScreenBounds { get; }

        protected UiElement(Vector4 screenBounds) {
            ScreenBounds = screenBounds;
        }

        internal abstract void LoadContent();

        internal virtual void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            Panel.Update(inputs);
        }

        internal virtual void Draw(SpriteBatch spriteBatch) {
            Panel?.Draw(spriteBatch);
        }
    }
}