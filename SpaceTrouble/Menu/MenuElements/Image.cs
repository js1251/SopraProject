using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;

namespace SpaceTrouble.Menu.MenuElements {
    internal sealed class Image : MenuElement {

        public Image(Texture2D texture) : base(texture) {
        }

        public void SetTexture(Texture2D texture) {
            mTexture = texture;
        }

        internal override void Update(Dictionary<ActionType, InputAction> inputs) {
        }
    }
}
