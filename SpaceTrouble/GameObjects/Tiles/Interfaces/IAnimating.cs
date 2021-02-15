using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.util.Tools;

namespace SpaceTrouble.GameObjects.Tiles.Interfaces {
    internal enum AnimationEffect {
        PlayLooping,
        PlayOnce,
        PlayOnceReverse,
        Sinus,
        None
    }

    internal interface IAnimating {
        public Texture2D Texture { get; }
        public float DrawScale { get; }
        public float CurrentFrame { get; set; }
        public Point TotalFrames { get; }
        public int CurrentLayer { get; set; }
        public float Angle { get; set; }
        public float AnimationSpeed { get; }
        public bool IsVisible { get; }
        public Vector2 DrawPosition { get; }
        public Color Color { get; }
        public AnimationEffect Effect { get; set; }

        public void Update(GameTime gameTime) {
            if (Effect is AnimationEffect.None) {
                return;
            }

            if (Effect is AnimationEffect.PlayLooping) {
                UpdatePlayLooping(gameTime);
            } else if (Effect is AnimationEffect.PlayOnce) {
                UpdatePlayOnce(gameTime);
            } else if (Effect is AnimationEffect.PlayOnceReverse) {
                UpdatePlayOnceReverse(gameTime);
            } else if (Effect is AnimationEffect.Sinus) {
                System.Diagnostics.Debug.WriteLine("Sinus animations not implemented yet");
            }
        }

        public void GetAngleFromHeading(Vector2 heading, float lerpAmount = 0.1f) {
            var angle = VectorMath.VectorToAngle(heading);
            Angle = VectorMath.LerpDegrees(Angle, angle, lerpAmount);
        }

        public void SetLayerFromAngle() {
            CurrentLayer = (int) Math.Round(Angle / 360 * TotalFrames.Y);
            CurrentLayer %= TotalFrames.Y;
        }

        public void SetFrameFromAngle() {
            CurrentFrame = (int) Math.Round(Angle / 360 * TotalFrames.X);
            CurrentFrame %= TotalFrames.X;
        }

        private void UpdatePlayLooping(GameTime gameTime) {
            CurrentFrame += AnimationSpeed * (float) gameTime.ElapsedGameTime.TotalSeconds;
            CurrentFrame %= TotalFrames.X;
        }

        private void UpdatePlayOnce(GameTime gameTime) {
            // play animation once
            if (CurrentFrame < TotalFrames.X - 1) {
                CurrentFrame += AnimationSpeed * (float) gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (CurrentFrame >= TotalFrames.X - 1) {
                // once fully played set effect to none
                CurrentFrame = TotalFrames.X - 1;
                Effect = AnimationEffect.None;
            }
        }

        private void UpdatePlayOnceReverse(GameTime gameTime) {
            // play animation once in reverse
            if (CurrentFrame > 0) {
                CurrentFrame -= AnimationSpeed * (float) gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (CurrentFrame <= 0) {
                // once fully played set effect to none
                CurrentFrame = 0;
                Effect = AnimationEffect.None;
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            if (Texture == null || !IsVisible) {
                return;
            }

            var frameSize = GetFrameSize();
            spriteBatch.Draw(Texture, DrawPosition, GetSourceRectangle(frameSize), Color, 0, Vector2.Zero, DrawScale, SpriteEffects.None, 1);
        }

        private Point GetFrameSize() {
            return new Point(Texture.Width / TotalFrames.X, Texture.Height / TotalFrames.Y);
        }

        private Rectangle GetSourceRectangle(Point frameSize) {
            return new Rectangle((int) CurrentFrame * frameSize.X, frameSize.Y * CurrentLayer, frameSize.X, frameSize.Y);
        }
    }
}