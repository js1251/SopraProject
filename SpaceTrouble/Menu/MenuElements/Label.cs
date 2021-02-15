using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.Tools;

namespace SpaceTrouble.Menu.MenuElements {
    internal class Label : MenuElement {
        protected SpriteFont mFont;
        public string Text { get; set; }
        private Color mColor;
        protected Color DefaultColor { get; set; } = Color.Aquamarine;

        public Color TextColor {
            get => mColor;
            set => mColor = value != default ? value : DefaultColor;
        }
        private float FontSize { get; }
        private float FontScale { get; set; }
        internal Panel Parent { private get; set; }

        public Label(SpriteFont font, Color color = default, string text = "", float fontSize = 16) {
            mFont = font;
            Text = text;
            TextColor = color;
            FontSize = fontSize;
        }

        internal override void Update(Dictionary<ActionType, InputAction> inputs) {
            FontScale = FontSize * Global.WindowHeight * 0.00003f;
            if (Parent != null) {
                Text = WrapText();
            }
        }

        private string WrapText() {
            // oh lord that's some ugly code... Im sorry about that -Jakob
            var pointer = 0;
            var text = "";
            while (pointer < Text.Length) {
                var line = "";
                var currentChar = '\0';
                while (pointer < Text.Length && mFont.MeasureString(line).X * FontScale <= Parent.mBounds.Width - Parent.mBounds.Width * 0.2f) {
                    currentChar = Text[pointer++];
                    line += currentChar;
                    if (currentChar.Equals('\n')) {
                        break;
                    }
                }

                if (!currentChar.Equals('\n')) {
                    if (!currentChar.Equals(' ')) {
                        while (pointer < Text.Length) {
                            currentChar = Text[pointer++];
                            line += currentChar;
                            if (currentChar.Equals('\n') || currentChar.Equals(' ')) {
                                break;
                            }
                        }
                    }

                    line += '\n';
                }

                text += line;
            }

            return text;
        }

        internal override void Draw(SpriteBatch spriteBatch, float alpha) {
            var fontOffset = mBounds.Center.ToVector2() - (mFont.MeasureString(Text) / 2) * FontScale;
            spriteBatch.DrawString(mFont, Text, fontOffset, TextColor * alpha, 0, Vector2.Zero, FontScale, SpriteEffects.None, 0);
        }
    }
}