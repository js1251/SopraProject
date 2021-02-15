using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// Created by Jakob Sailer

namespace SpaceTrouble.util.Tools.Assets {
    internal sealed class Fonts {
        internal SpriteFont ButtonFont { get; private set; }
        internal SpriteFont GuiFont01 { get; private set; }
        internal void LoadContent(ContentManager content) {
            ButtonFont = content.Load<SpriteFont>("fonts/DebugFont");
            GuiFont01 = content.Load<SpriteFont>("fonts/gui01");
        }
    }
}
