using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;

namespace SpaceTrouble.Menu.MenuElements {
    public abstract class MenuElement {
        protected Texture2D mTexture;
        internal Rectangle mBounds = Rectangle.Empty;
        internal readonly bool mVisible;

        protected MenuElement(Texture2D backgroundTexture = null) {
            mTexture = backgroundTexture;
            mVisible = true;
        }

        internal abstract void Update(Dictionary<ActionType, InputAction> inputs);

        internal virtual void Draw(SpriteBatch spriteBatch, float alpha) {
            if (mVisible) {
                spriteBatch.Draw(mTexture, mBounds, Color.White * alpha);
            }
        }
    }
}