using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.Menu {
    internal sealed class CreditsOverlay : GameStateOverlay {

        private Panel Panel { get; set; }
        public CreditsOverlay(string overlayName/*, int priority*/, bool active = true) : base(overlayName/*, priority,*/, active) {
        }

        internal override void LoadContent() {
            var font = Assets.Fonts.ButtonFont;
            var creditString = "Created by Jakob Sailer, Niklas Stahl (partially), Viktor Gange, Kai Koenig, Luca Haist, Vanessa Lienhart and Franziska Kordowich. ";
            creditString += "Part of \"Sopra\" 2020/2021 Albert-Ludwigs Universitaet Freiburg, Technische Fakultaet.";
            Panel = new Panel(new Vector4(0, 0.95f, 1, 0.05f), Vector2.Zero, new MenuElement[,] {
                {new Label(font, default, creditString)}
            });
            Panel.Offset = new Vector2(1.5f,0);
        }

        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            Panel.Offset -= new Vector2((float)gameTime.ElapsedGameTime.TotalSeconds * 0.05f, 0);
            if (Panel.Offset.X < -1.5f) {
                Panel.Offset = new Vector2(1.5f, 0);
            }
            Panel.Update(inputs);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            Panel.Draw(spriteBatch);
        }
    }
}
