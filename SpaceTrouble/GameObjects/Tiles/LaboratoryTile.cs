using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.util.Tools.Assets;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Tiles {

    internal sealed class LaboratoryLaser : IAnimating {
        public Texture2D Texture { get; }
        public float DrawScale { get; set; }
        public float CurrentFrame { get; set; }
        public Point TotalFrames { get; }
        public int CurrentLayer { get; set; }
        public float Angle { get; set; }
        public float AnimationSpeed { get; }
        public bool IsVisible { get; }
        public Vector2 DrawPosition { get; set; }
        public Color Color { get; set; }
        public AnimationEffect Effect { get; set; }

        public LaboratoryLaser() {
            Texture = Assets.Textures.Tiles.Laboratory;

            TotalFrames = new Point(12, 1);
            CurrentLayer = 0;
            CurrentFrame = 0;
            AnimationSpeed = 2f;
            IsVisible = true;
            Effect = AnimationEffect.None;
        }

        internal void Update(GameTime gameTime) {
            ((IAnimating)this).Update(gameTime);
        }

        internal void Draw(SpriteBatch spriteBatch) {
            ((IAnimating) this).Draw(spriteBatch);
        }
    }

    internal sealed class LaboratoryTile : PortalTile {
        [JsonProperty] internal bool IsClosed { get; set; }
        [JsonIgnore] private LaboratoryLaser Laser { get; }
        public LaboratoryTile() {
            RequiredResources = new ResourceVector(0, 0, 0);
            RequiredResources = new ResourceVector(5, 5, 0); // TODO: balance
            SpawnType = GameObjectEnum.FlyingEnemy; // this seems counter-intuitive but laboratories are not fully built by default
            Laser = new LaboratoryLaser();
        }

        internal void UpdateLaser(GameTime gameTime) {
            // pass the color through to the lasertile
            Laser.Color = Color;
            Laser.DrawPosition = DrawPosition - Vector2.UnitY * 4;
            Laser.DrawScale = DrawScale;
            Laser.Update(gameTime);
        }

        internal override void Update(GameTime gameTime) {
            UpdateLaser(gameTime);
            // if the laboratory is under construction its behaviour is the same as the portalTile
            // (spawn double the enemies (walking AND flying)
            if (BuildingFinished) {
                // if construction is finished this tile acts as a simple CreatureSpawner, spawning walkingEnemies only
                SpawnBoth = false;
                SpawnType = GameObjectEnum.WalkingEnemy;
                Laser.Effect = AnimationEffect.PlayLooping;
            }

            base.Update(gameTime);
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            if (Texture == null || !IsVisible) {
                return;
            }

            if (!BuildingFinished) {
                var color = Color;
                Color = Color.White;
                base.Draw(spriteBatch);
                Color = color;
                Laser.Draw(spriteBatch);
            } else {
                base.Draw(spriteBatch);
                Laser.Draw(spriteBatch);
            }

            if (!BuildingFinished) {
                ((IBuildable)this).DrawResources(spriteBatch);
            }

            ((ICanHavePriority)this).Draw(spriteBatch);
        }
    }
}