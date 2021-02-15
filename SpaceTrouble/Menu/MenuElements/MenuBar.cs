using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.Tools;

// Created by Jakob Sailer

namespace SpaceTrouble.Menu.MenuElements {
    internal sealed class MenuBar : Label {
        private float TrueFillAmount { get; set; }
        public float FillAmount { get; set; }

        private Color mBarColor;
        private Color DefaultBarColor { get; } = Color.White;
        public Color BarColor {
            get => mBarColor;
            set => mBarColor = value != default ? value : DefaultBarColor;
        }
        private Rectangle FillRectangle { get; set; }

        public MenuBar(Texture2D texture, Color barColor = default, SpriteFont font = null, string text = "", Color textcolor = default, float fontSize = 16f) : base(font, textcolor, text, fontSize) {
            mTexture = texture;
            DefaultColor = Color.AntiqueWhite;
            TextColor = textcolor;
            BarColor = barColor;
        }

        internal override void Update(Dictionary<ActionType, InputAction> inputs) {
            base.Update(inputs);

            TrueFillAmount = TrueFillAmount.Lerp(FillAmount, 0.2f);

            FillRectangle = new Rectangle {
                X = 0,
                Y = 0,
                Width = (int) (mTexture.Width * TrueFillAmount),
                Height = mTexture.Height
            };

            mBounds = new Rectangle {
                X = mBounds.X,
                Y = mBounds.Y,
                Width = (int)(mBounds.Width * TrueFillAmount),
                Height = mBounds.Height
            };
        }

        internal override void Draw(SpriteBatch spriteBatch, float alpha) {
            spriteBatch.Draw(mTexture, mBounds, FillRectangle, BarColor * alpha, 0, Vector2.Zero, SpriteEffects.None, 0);
            if (mFont != null) {
                base.Draw(spriteBatch, alpha);
            }
        }
    }
}
