using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.Menu.MenuElements {
    internal sealed class MenuButton : Label {
        private bool mMouseOverButton;

        private readonly Color mButtonColor = Color.White;
        private readonly Color mHoverColor = Color.Gray;

        private ActionType? mPushState;
        internal string ToolTip { get; set; }
        private Vector2 MousePos { get; set; }
        private float HoverTime { get; set; }

        public MenuButton(Texture2D texture, SpriteFont font = null, string text = "", Color color = default, float fontSize = 16f) : base(font, color, text, fontSize) {
            mTexture = texture;
            DefaultColor = Color.AntiqueWhite;
            TextColor = color;
            ToolTip = "";
        }

        internal override void Update(Dictionary<ActionType, InputAction> inputs) {
            base.Update(inputs);
            if (inputs.TryGetValue(ActionType.MouseMoved, out var input)) {
                mMouseOverButton = mBounds.Contains(input.Origin);
                MousePos = input.Origin;

                HoverTime += 0.0167f; // horrible hack
            }

            if (inputs.TryGetValue(ActionType.MouseLeftClick, out input) && mBounds.Contains(input.Origin)) {
                mPushState = ActionType.MouseLeftClick;
                SpaceTrouble.SoundManager.PlaySound(Sound.Clicking);
                input.mUsed = true;
                return;
            }

            if (inputs.TryGetValue(ActionType.MouseRightClick, out input) && mBounds.Contains(input.Origin)) {
                mPushState = ActionType.MouseRightClick;
                SpaceTrouble.SoundManager.PlaySound(Sound.Clicking);
                input.mUsed = true;
            }

            HoverTime = mMouseOverButton ? HoverTime : 0;
        }

        internal override void Draw(SpriteBatch spriteBatch, float alpha) {
            var buttonColor = mMouseOverButton ? mHoverColor : mButtonColor;
            spriteBatch.Draw(mTexture, mBounds, buttonColor * alpha);
            if (mFont != null) {
                base.Draw(spriteBatch, alpha);
            }

            if (mMouseOverButton && HoverTime > 0.5f) {
                DrawToolTip(spriteBatch);
            }
        }

        private void DrawToolTip(SpriteBatch spriteBatch) {
            spriteBatch.DrawString(Assets.Fonts.ButtonFont, ToolTip, MousePos - Vector2.UnitY * Global.WindowHeight / 50f, Color.White, 0, Vector2.Zero, Global.WindowWidth / 4000f, SpriteEffects.None, 0);
        }

        public bool GetPushState(bool reset = false, ActionType type = ActionType.MouseLeftClick) {
            var pushState = mPushState == type;
            if (reset && pushState) {
                mPushState = null;
            }
            return pushState;
        }
    }
}