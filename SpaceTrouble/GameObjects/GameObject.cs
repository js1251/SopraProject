using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects {
    internal abstract class GameObject {
        /// <summary>
        /// Pivot-point relative to object. So eg. {.5, .5} would be the center
        /// </summary>
        [JsonIgnore] public Vector2 Pivot { get; protected set; }
        /// <summary>
        /// World-unit size
        /// </summary>
        [JsonIgnore] public Point Dimensions { get; protected set; }
        /// <summary>
        /// The pivot-point on the GameObjectSprite
        /// </summary>
        [JsonIgnore] private Vector2 DrawOrigin { get; set; }
        /// <summary>
        /// The world-coordinates, where to draw this object
        /// </summary>
        [JsonIgnore] public Vector2 DrawPosition { get; private set; }
        /// <summary>
        /// The scale at which to draw this object
        /// </summary>
        [JsonIgnore] public float DrawScale { get; private set; }
        /// <summary>
        /// Sets the objects visibility
        /// </summary>
        [JsonProperty] public bool IsVisible { get; set; }
        /// <summary>
        /// Set this to true if you want the object to always draw above minions and other objects bound by gravity
        /// </summary>
        [JsonIgnore] internal bool DrawFlying { get; set; }
        /// <summary>
        /// The color of the object.
        /// </summary>
        [JsonProperty] public Color Color { get; set; }
        /// <summary>
        /// The GameObjectSprite
        /// </summary>
        [JsonIgnore] private Texture2D mTexture;
        [JsonIgnore] public Texture2D Texture {
            set {
                mTexture = value;
                DrawScale = Dimensions.Y / ((float)value.Height / TotalFrames.Y);
                DrawOrigin = new Vector2(value.Width, value.Height);
                DrawOrigin *= DrawScale;
                DrawOrigin /= TotalFrames.ToVector2();
                DrawOrigin *= Pivot;
                DrawPosition = GetDrawPos(WorldPosition); // this is done when objects are loaded instead of newly created
            }
            get => mTexture;
        }
        /// <summary>
        /// The amount of frames the GameObjectSprite has
        /// </summary>
        /// TODO: move to IAnimating!
        [JsonIgnore] public Point TotalFrames { get; protected set; } = new Point(1,1);

        // World interaction
        
        /// <summary>
        /// True, whenever the object fundamentally changed. Set this to true to let the Data-structure now this object needs to be re-evaluated
        /// </summary>
        [JsonProperty] public bool HasChanged { get; set; }
        /// <summary>
        /// A enum representation of the Type (this.GetType())
        /// </summary>
        [JsonIgnore] public GameObjectEnum Type { get; }
        /// <summary>
        /// The world-coordinates of this object
        /// </summary>
        [JsonIgnore] private Vector2 mWorldPosition;
        [JsonProperty] public Vector2 WorldPosition {
            set {
                mWorldPosition = value;
                DrawPosition = GetDrawPos(value);
            }
            get => mWorldPosition;
        }

        protected GameObject() {
            Color = Color.White;
            IsVisible = true;
            HasChanged = false;
            Type = (GameObjectEnum) Enum.Parse(typeof(GameObjectEnum), GetType().Name);
        }

        internal virtual void Update(GameTime gameTime) {
            if (OffWorld()) {
                OnOffWorld();
            }
        }

        private Vector2 GetDrawPos(Vector2 value) {
            return value - DrawOrigin;
        }

        internal virtual void Draw(SpriteBatch spriteBatch) {
            if (Texture == null || !IsVisible) {
                return;
            }
            spriteBatch.Draw(Texture, DrawPosition, null, Color, 0, Vector2.Zero, DrawScale, SpriteEffects.None, 1);
        }

        internal virtual void DrawDebug(SpriteBatch spriteBatch, DebugMode mode) {
        }

        internal virtual void OnOffWorld() {
            WorldGameState.ObjectManager.Remove(this);
        }

        private bool OffWorld() {
            const int rightBounds = Global.WorldWidth * Global.TileWidth;
            const int bottomBounds = Global.WorldHeight * Global.TileHeight;
            return WorldPosition.X < 0 || WorldPosition.X > rightBounds || WorldPosition.Y < 0 || WorldPosition.Y > bottomBounds;
        }
    }
}