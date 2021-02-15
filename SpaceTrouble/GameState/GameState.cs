using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;

namespace SpaceTrouble.GameState {
    /// <summary>
    /// An abstract class to implement basic functionality and interfaces that any GameState will need. <br/>
    /// </summary>
    public abstract class GameState {
        internal readonly string mStateName;

        internal bool CaptureCursor { get; set; }

        protected GameState(string stateName) {
            mStateName = stateName;
        }

        /// <summary>
        /// Called on the active GameState once every iteration of the game loop before update calls are made. <br/>
        /// The base method implements basic reactions to inputs (For now only to remove the active GameState). <br/>
        /// Overwrite it to control state changes yourself.
        /// </summary>
        internal virtual void CheckForStateChanges(GameStateManager stateManager, Dictionary<ActionType, InputAction> inputs) {
            if (inputs.ContainsKey(ActionType.StateBackAction)) {
                stateManager.RemoveActiveGameState();
            }
        }

        /// <summary>
        /// Called every time the GameState is switched to before updates are sent to Overlays or the GameState.
        /// </summary>
        internal virtual void Activated(GameTime gameTime, HashSet<string> messages) {
        }

        /// <summary>
        /// Called every time the GameState is switched away from.
        /// </summary>
        internal virtual void Deactivated(GameTime gameTime) {
        }

        /// <summary>
        /// Called oncy at the start of the Game.
        /// Setup Overlays and Global variables here.
        /// </summary>
        internal abstract void Initialize();

        /// <summary>
        /// Called oncy at the start of the Game after all GameStates are Initialized.
        /// Load content here and perform setup that depends on other GameStates.
        /// </summary>
        internal abstract void LoadContent();

        public abstract void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs);

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}