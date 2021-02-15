using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.World {

    internal sealed class Star {
        private Texture2D Texture { get; }
        private Vector2 Position { get; }
        private Vector2 Origin { get; }
        private float Scale { get; }
        private float Rotation { get; }
        private Color Color { get; }

        public Star(List<Texture2D> textures, Rectangle bounds, double scale, Color color, int colorShiftAmount) {
            var rng = new Random();

            Texture = textures[rng.Next(0, textures.Count)];
            if (Texture == Assets.Textures.Tiles.Mass) {
                Texture = textures[rng.Next(textures.Count - 1, textures.Count)];
            }

            Origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);

            Position = new Vector2 {
                X = rng.Next(bounds.X, bounds.Width),
                Y = rng.Next(bounds.Y, bounds.Height)
            };

            Scale = (float)Math.Clamp(rng.NextDouble() * scale, 0 , scale);
            Scale *= Texture == Assets.Textures.Tiles.Mass ? 2 : 64f / Texture.Height;

            Rotation = Texture == Assets.Textures.Tiles.Mass ? 0 : (float)rng.NextDouble() * 360;

            var rngColor = new Color(
                rng.Next(-colorShiftAmount, colorShiftAmount) + color.R - colorShiftAmount,
                rng.Next(-colorShiftAmount, colorShiftAmount) + color.G - colorShiftAmount,
                rng.Next(-colorShiftAmount, colorShiftAmount) + color.B - colorShiftAmount);
            Color = Texture == Assets.Textures.Tiles.Mass ? Color.Gray : new Color(rngColor.R, rngColor.G, rngColor.B, (int)(rng.NextDouble() * 255) + 20);
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(Texture, Position, null, Color, Rotation, Origin, Scale, SpriteEffects.None, 0);
        }
    }

    internal sealed class Background {

        private int NearStarAmount { get; }
        private int FarStarAmount { get; }
        private int DustParticleAmount { get; }
        private List<Star> NearStars { get; }
        private List<Star> FarStars { get; }
        private List<Star> DustParticles { get; }
        private List<Texture2D> StarTextures { get; }
        private Texture2D DustTexture { get; set; }
        private Vector2[] WorldEdges { get; set; }

        public Background() {
            NearStars = new List<Star>();
            FarStars = new List<Star>();
            DustParticles = new List<Star>();
            StarTextures = new List<Texture2D>();
            NearStarAmount = 500;
            FarStarAmount = 500;
            DustParticleAmount = 500;
        }

        internal void LoadContent() {
            StarTextures.Add(Assets.Textures.Stars.Star01);
            StarTextures.Add(Assets.Textures.Stars.Star02);
            StarTextures.Add(Assets.Textures.Stars.Star03);
            StarTextures.Add(Assets.Textures.Stars.Star04);
            StarTextures.Add(Assets.Textures.Stars.Star05);
            StarTextures.Add(Assets.Textures.Stars.Star06);
            StarTextures.Add(Assets.Textures.Stars.Star07);
            StarTextures.Add(Assets.Textures.Stars.Star08);
            StarTextures.Add(Assets.Textures.Stars.Star09);
            StarTextures.Add(Assets.Textures.Stars.Star10);
            StarTextures.Add(Assets.Textures.Stars.Star11);
            StarTextures.Add(Assets.Textures.Tiles.Mass);
            DustTexture = Assets.Textures.Stars.Star01;
        }

        internal void CreateStarFieldScreen() {
            var bounds = new Rectangle(0,0, Global.WindowWidth * 2, Global.WindowHeight * 2);
            CreateStarField(bounds);
        }

        internal void CreateStarFieldWorld() {
            var bounds = new Rectangle(0,0, Global.TileWidth * Global.WorldWidth, Global.TileHeight * Global.WorldHeight);
            CreateStarField(bounds);
        }

        internal void CreateWorldOutline() {
            WorldEdges = new[] {
                new Vector2(0, Global.TileHeight * Global.mWorldOrigin.Y), // left
                new Vector2(Global.TileWidth * Global.mWorldOrigin.X, 0), // top
                new Vector2(Global.TileWidth * Global.WorldWidth,Global.TileHeight * Global.mWorldOrigin.Y), // right
                new Vector2(Global.TileWidth * Global.mWorldOrigin.X, Global.TileHeight * Global.WorldHeight) // bottom
            };
        }

        private void CreateStarField(Rectangle bounds) {
            FarStars.Clear();
            for (var i = 0; i < FarStarAmount; i++) {
                FarStars.Add(new Star(StarTextures, bounds, 0.25f, Color.LightYellow, 30));
            }

            NearStars.Clear();
            for (var i = 0; i < NearStarAmount; i++) {
                NearStars.Add(new Star(StarTextures, bounds, 0.5f, Color.LightGoldenrodYellow, 30));
            }

            DustParticles.Clear();
            for (var i = 0; i < DustParticleAmount; i++) {
                DustParticles.Add(new Star(new List<Texture2D> { DustTexture }, bounds, .25f, Color.White, 10));
            }
        }

        internal void DrawBackGround(SpriteBatch spriteBatch) {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null, null, SpaceTrouble.Camera.GetBackgroundTransformation(50));
            foreach (var star in FarStars) {
                star.Draw(spriteBatch);
            }
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null, null, SpaceTrouble.Camera.GetBackgroundTransformation(10));
            foreach (var star in NearStars) {
                star.Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        internal void DrawWorld(SpriteBatch spriteBatch) {
            spriteBatch.DrawPolygon(WorldEdges, WorldEdges.Length, Color.WhiteSmoke * 0.5f);
        }

        internal void DrawForeGround(SpriteBatch spriteBatch) {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null, null, SpaceTrouble.Camera.GetBackgroundTransformation(.9f));
            foreach (var dust in DustParticles) {
                dust.Draw(spriteBatch);
            }
            spriteBatch.End();
        }
    }
}
