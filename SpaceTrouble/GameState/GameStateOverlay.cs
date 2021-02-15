using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;

namespace SpaceTrouble.GameState {
    /// <summary>
    /// An Overlay that can be assigned to multiple GameStates and will then be drawn over them. <br/>
    /// An priority sets the order the Overlays will be drawn and receive updates.
    /// A higher priority will receive updates first and be drawn on top.
    /// </summary>
    internal abstract class GameStateOverlay {
        internal readonly string mOverlayName;
        internal bool Active { get; }
        //private readonly int mPriority;

        protected GameStateOverlay(string overlayName/*, int priority*/, bool active=true) {
            mOverlayName = overlayName;
            Active = active;
            //mPriority = priority;
        }

        internal abstract void LoadContent();

        public abstract void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs);

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}