using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles {
    internal sealed class MassTile : Tile {

        [JsonIgnore] private Vector2 mMovement;
        [JsonIgnore] private readonly Vector2 mMoveAmount;
        [JsonIgnore] private float mSineVar;
        [JsonIgnore] private float mOffset;

        public MassTile() {
            Pivot = Vector2.One * 0.5f;
            IsEnterable = false;
            Color = Color.White;
            mMoveAmount = new Vector2(1f, 2f);
            mSineVar = 0f;
            mOffset = 0;
        }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);

            //TODO: animation should not be independent of frame-rate

            // I don't think this is a good way of doing this:
            if (mOffset == 0) {
                mOffset = new Random().Next(0, 100);
                mOffset /= 10;
            }

            mSineVar = (mSineVar + (float) gameTime.ElapsedGameTime.TotalSeconds) % 360;

            mMovement.X = (float) Math.Sin(mSineVar + mOffset) * mMoveAmount.X;
            mMovement.Y = (float) Math.Sin(mSineVar * 2f + mOffset) * mMoveAmount.Y;
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            if (Texture == null || !IsVisible) {
                return;
            }

            spriteBatch.Draw(Texture, DrawPosition + mMovement, null, Color, 0, Vector2.Zero, DrawScale, SpriteEffects.None, 0);
        }
    }
}