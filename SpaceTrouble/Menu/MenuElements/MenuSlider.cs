using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.Menu.MenuElements {
    internal sealed class MenuSlider : Label {
        private bool mMouseOverButton;
        private bool mDragStarted;
        private readonly Color mColor = Color.White;
        private readonly Color mHoverColor = Color.Gray;

        private int mSliderWidth;
        private float mSliderState;
        public float SliderState {
            get => mSliderState;
            set => mSliderState = Math.Clamp(value, 0, 1);
        }

        public MenuSlider(Texture2D texture, SpriteFont font = null, string text = "", Color color = default, float fontSize = 16f) : base(font, color, text, fontSize) {
            mFont = font;
            mTexture = texture;
        }

        internal override void Update(Dictionary<ActionType, InputAction> inputs) {
            base.Update(inputs);
            mSliderWidth = (int)(0.05f * mBounds.Width);

            if (inputs.TryGetValue(ActionType.MouseMoved, out var input)) {
                mMouseOverButton = mBounds.Contains(input.Origin);
            }

            if (inputs.TryGetValue(ActionType.MouseDrag, out input) && mDragStarted) {
                if (mBounds.Contains(input.Origin)) {
                    SliderState = (input.Origin.X - mBounds.X - mSliderWidth / 2.0f) / (mBounds.Width - mSliderWidth);
                }
            }

            if (inputs.TryGetValue(ActionType.MouseLeftClick, out input)) {
                mDragStarted = mBounds.Contains(input.Origin);
            }

            if (inputs.TryGetValue(ActionType.MouseDragStop, out _) && mDragStarted) {
                SpaceTrouble.SoundManager.PlaySound(Sound.Clicking);
                mDragStarted = false;
            }
        }

        internal override void Draw(SpriteBatch spriteBatch, float alpha) {
            var sliderColor = mMouseOverButton ? mHoverColor : mColor;
            spriteBatch.Draw(mTexture, mBounds, Color.White * alpha);
            if (mFont != null) {
                base.Draw(spriteBatch, alpha);
            }
            var bounds = new Rectangle((int) (mBounds.X + (mBounds.Width - mSliderWidth) * mSliderState), mBounds.Y, mSliderWidth, mBounds.Height);
            spriteBatch.Draw(Assets.Textures.InterfaceTextures.GuiBar, bounds, sliderColor * alpha * (mFont != null ? 0.75f : 1f));
        }
    }
}