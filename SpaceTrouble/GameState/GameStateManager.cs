using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;

namespace SpaceTrouble.GameState {
    /// <summary name="GameStateManager">
    /// The GameStateManager enables the separation of unrelated GameStates and centralizes switching between them.  <br/>
    /// It holds the individual GameStates and Overlays that could be shown and passes on
    /// Update and Draw calls from the main game-loop to these GameStates. <br/>
    /// It also makes additional calls to signify GameStates when they are about to be switched from or switched to. <br/>
    /// Additionally the GameStateManager distributes InputActions from the InputManager to the
    /// active GameState and any associated Overlays in order of their priority.
    /// <para>GameStates and Overlays are usually referenced by their name.</para>
    /// </summary>
    internal sealed class GameStateManager {
        private readonly InputManager mInputManager;

        private readonly Dictionary<string, GameState> mGameStates = new Dictionary<string, GameState>();
        private readonly List<GameState> mActiveGameStateStack = new List<GameState>();

        private readonly Dictionary<string, GameStateOverlay> mOverlays = new Dictionary<string, GameStateOverlay>();
        private readonly Dictionary<string, List<string>> mMapStateToOverlays = new Dictionary<string, List<string>>();
        private readonly HashSet<string> mStateMessages = new HashSet<string>();

        public GameState ActiveGameState => mActiveGameStateStack.LastOrDefault();

        public GameStateManager(InputManager inputManager) {
            mInputManager = inputManager;
        }

        /// <summary>
        /// Add a GameState. Optionally add it to the Stack of active GameStates.
        /// </summary>
        public void AddGameState(GameState gameState, bool activateState = false) {
            if (!mGameStates.ContainsKey(gameState.mStateName)) {
                mGameStates.Add(gameState.mStateName, gameState);
                mMapStateToOverlays.Add(gameState.mStateName, new List<string>());
                if (activateState) {
                    ActivateGameState(gameState.mStateName);
                }
            }
        }

        public void AddOverlay(GameStateOverlay overlay) {
            if (!mOverlays.ContainsKey(overlay.mOverlayName)) {
                mOverlays.Add(overlay.mOverlayName, overlay);
            }
        }

        /// <summary>
        /// Add a list of Overlays to a GameState. 
        /// If the priority of Overlays conflict the one added later is drawn on top and receives updates first.
        /// </summary>
        public void AddOverlaysToStates(IEnumerable<string> overlayNames, IEnumerable<string> stateNames = null) {
            stateNames ??= mGameStates.Keys;
            stateNames = mGameStates.Keys.Intersect(stateNames);

            // make sure only valid stateNames are chosen (intersect with the list of valid states)
            overlayNames = new List<string>(mOverlays.Keys.Intersect(overlayNames));

            // for every state the overlay is to be inserted...
            foreach (var stateToInsert in stateNames) {
                // TODO: priorities are simply ignored atm
                mMapStateToOverlays[stateToInsert].AddRange(overlayNames);
            }
        }

        /// <summary>
        /// Remove a list of Overlays from a GameState. <br/>
        /// </summary>
        
        /*
        public void RemoveOverlaysFromState(IEnumerable<string> overlayNames, string stateName) {
            if (mGameStates.ContainsKey(stateName)) {
                var overlayList = mMapStateToOverlays[stateName];
                foreach (var name in mOverlays.Keys.Intersect(overlayNames)) {
                    overlayList.Remove(name);
                }
            }
        }
        */

        internal void ActivateGameState(string newGameStateName, bool replaceActiveState = false, string message = "") {
            if (mGameStates.ContainsKey(newGameStateName)) {
                if (replaceActiveState) {
                    RemoveActiveGameState();
                }

                mActiveGameStateStack.Add(mGameStates[newGameStateName]);
                if (message != "") {
                    mStateMessages.Add(message);
                }
            }
        }

        internal void RemoveActiveGameState(string message = "") {
            if (mActiveGameStateStack.Count > 0) {
                mActiveGameStateStack.RemoveAt(mActiveGameStateStack.Count - 1);
                if (message != "") {
                    mStateMessages.Add(message);
                }
            }
        }

        /*
        internal void SendStateMessage(string message) {
            mStateMessages.Add(message);
        }

        internal bool ClearStateMessage(string message) {
            if (message == "") {
                mStateMessages.Clear();
                return true;
            }

            return mStateMessages.Remove(message);
        }
        */

        /// <summary>
        /// Initialize the GameStates to allow them to add their own Overlays.
        /// </summary>
        public void Initialize() {
            foreach (var gameState in mGameStates.Values) {
                gameState.Initialize();
            }
        }

        /// <summary>
        /// Let GameStates and Overlays load their respective content.
        /// </summary>
        public void LoadContents() {
            foreach (var gameState in mGameStates.Values) {
                gameState.LoadContent();
            }

            foreach (var overlay in mOverlays.Values) {
                overlay.LoadContent();
            }
        }

        /// <summary>
        /// Update the Overlays and the active GameState and distribute Inputs from the InputManager. <br />
        /// Change the active game state if necessary, before updates are send.
        /// </summary>
        /// <returns>The number of active GameStates.</returns>
        public int Update(GameTime gameTime) {
            mInputManager.UpdateInputs(gameTime);
            var inputs = mInputManager.GetMappedInputActions();
            var activeState = ActiveGameState;

            // Ask the active GameState if StateChanges are required.
            ActiveGameState.CheckForStateChanges(this, inputs);
            InputManager.RemoveUsedActions(inputs);

            if (activeState != ActiveGameState) {
                activeState.Deactivated(gameTime);

                if (ActiveGameState == null) {
                    return 0;
                }

                ActiveGameState.Activated(gameTime, mStateMessages);
            }

            // Send Updates to Overlays and the Active GameState.
            foreach (var overlayName in Enumerable.Reverse(mMapStateToOverlays[ActiveGameState.mStateName])) {
                var overlay = mOverlays[overlayName];
                if (!overlay.Active) {
                    continue;
                }

                overlay.Update(gameTime, inputs);
                InputManager.RemoveUsedActions(inputs);
            }

            ActiveGameState.Update(gameTime, inputs);

            return mActiveGameStateStack.Count;
        }

        public void Draw(SpriteBatch spriteBatch) {
            // draw the active state first
            ActiveGameState.Draw(spriteBatch);

            // then draw its overlays
            foreach (var overlayName in mMapStateToOverlays[ActiveGameState.mStateName]) {
                spriteBatch.Begin();
                mOverlays[overlayName].Draw(spriteBatch);
                spriteBatch.End();
            }
        }
    }
}