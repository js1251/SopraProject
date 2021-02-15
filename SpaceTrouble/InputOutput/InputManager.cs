using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SpaceTrouble.InputOutput {
    public enum ActionType {
        StateBackAction,
        MouseHover,
        MouseDragStart,
        MouseDrag,
        MouseDragStop,
        MouseMoved,
        MouseLeftClick,
        MouseRightClick,
        MouseScrolled,
        DebugToggled,
        ForceBuild,
        Pause
    }

    public enum InputType {
        Push, // started pressing the key
        Release, // stopped pressing the key
        HeldDown, // the key was pressed previously and is still held down
        HeldUp,
        FastKlick, // the key was pressed for a short time
        Any,
        Scroll
    }

    public enum MouseButton {
        Left,
        Right,
        ScrollWheel
    }

    public sealed class StateStore {
        internal readonly Dictionary<InputType, ActionType> mInputToAction = new Dictionary<InputType, ActionType>();
        private bool mWasPressed;
        private int mOldValue;
        private double mDuration;
        public int mAmount;

        private double mMaxDurationInMs = 300;

        public HashSet<ActionType> Update(bool isPressed, double passedTime, int value) {
            HashSet<ActionType> actions = new HashSet<ActionType>();

            mDuration += passedTime;
            mAmount = value - mOldValue;

            foreach (var type in mInputToAction.Keys) {
                if ((type == InputType.Push && isPressed && !mWasPressed) |
                    (type == InputType.Release && !isPressed && mWasPressed) |
                    (type == InputType.HeldDown && isPressed && mWasPressed) |
                    (type == InputType.HeldUp && !isPressed && !mWasPressed) |
                    (type == InputType.FastKlick && !isPressed && mWasPressed && mDuration <= mMaxDurationInMs) |
                    (type == InputType.Any) |
                    (type == InputType.Scroll && mAmount != 0)) {
                    actions.Add(mInputToAction[type]);
                }
            }

            if (isPressed != mWasPressed) {
                mDuration = 0;
            }

            mWasPressed = isPressed;
            mOldValue = value;
            return actions;
        }
    }

    public sealed class InputManager {
        private readonly Dictionary<ActionType, InputAction> mInputActions = new Dictionary<ActionType, InputAction>();
        private readonly Dictionary<Keys, StateStore> mKeyStates = new Dictionary<Keys, StateStore>();
        private readonly Dictionary<MouseButton, StateStore> mMouseStates = new Dictionary<MouseButton, StateStore>();

        private KeyboardState mCurrentKeyboardState;
        private MouseState mCurrentMouseState;
        private Point mOldMousePosition;
        private Point mCursorPosition;
        private bool mCaptureCursor;

        public InputManager() {
            SetKeyMapping(Keys.Escape, InputType.Release, ActionType.StateBackAction);
            SetKeyMapping(Keys.F3, InputType.Push, ActionType.DebugToggled);
            SetKeyMapping(Keys.B, InputType.HeldDown, ActionType.ForceBuild);
            SetKeyMapping(Keys.P, InputType.Push, ActionType.Pause);
            SetMouseMapping(MouseButton.Left, InputType.HeldDown, ActionType.MouseDrag);
            SetMouseMapping(MouseButton.Left, InputType.Release, ActionType.MouseDragStop);
            SetMouseMapping(MouseButton.Left, InputType.Any, ActionType.MouseMoved);
            SetMouseMapping(MouseButton.Left, InputType.HeldUp, ActionType.MouseHover);
            SetMouseMapping(MouseButton.Left, InputType.Push, ActionType.MouseLeftClick);
            SetMouseMapping(MouseButton.Right, InputType.Push, ActionType.MouseRightClick);
            SetMouseMapping(MouseButton.ScrollWheel, InputType.Scroll, ActionType.MouseScrolled);
        }

        private void SetKeyMapping(Keys key, InputType type, ActionType action) {
            if (!mKeyStates.ContainsKey(key)) {
                mKeyStates[key] = new StateStore();
            }

            mKeyStates[key].mInputToAction[type] = action;

            if (!mInputActions.ContainsKey(action)) {
                mInputActions[action] = new InputAction(Vector2.Zero /*, action*/); // TODO: this is commented out so Resharper doesnt complain
            }
        }

        private void SetMouseMapping(MouseButton button, InputType type, ActionType action) {
            if (!mMouseStates.ContainsKey(button)) {
                mMouseStates[button] = new StateStore();
            }

            mMouseStates[button].mInputToAction[type] = action;

            if (!mInputActions.ContainsKey(action)) {
                mInputActions[action] = new InputAction(Vector2.Zero /*, action*/); // TODO: this is commented out so Resharper doesnt complain
            }
        }

        public void Update(Game game, GameState.GameState activeGameState) {
            mCurrentKeyboardState = Keyboard.GetState();
            mCurrentMouseState = Mouse.GetState();

            if (game.IsActive && activeGameState.CaptureCursor) {
                mCaptureCursor = true;
                var bounds = game.GraphicsDevice.Viewport.Bounds;
                var mouseDelta = mCurrentMouseState.Position - mOldMousePosition;
                mCursorPosition += (mouseDelta.ToVector2() * 1.05f).ToPoint();
                mCursorPosition.X = Math.Clamp(mCursorPosition.X, 0, bounds.Width);
                mCursorPosition.Y = Math.Clamp(mCursorPosition.Y, 0, bounds.Height);
                if (mOldMousePosition.X < bounds.Center.X / 2 || mOldMousePosition.X > bounds.Center.X * 3 / 2 ||
                    mOldMousePosition.Y < bounds.Center.Y / 2 || mOldMousePosition.Y > bounds.Center.Y * 3 / 2) {
                    Mouse.SetPosition(bounds.Center.X, bounds.Center.Y);
                    mCurrentMouseState = Mouse.GetState();
                }

                mOldMousePosition = mCurrentMouseState.Position;
            } else {
                if (mCaptureCursor) {
                    Mouse.SetPosition(mCursorPosition.X, mCursorPosition.Y);
                    mCaptureCursor = false;
                }

                mCursorPosition = mCurrentMouseState.Position;
                mOldMousePosition = mCursorPosition;
            }
        }

        public void UpdateInputs(GameTime gameTime) {
            foreach (var inputAction in mInputActions.Values) {
                inputAction.mUsed = true;
            }

            var passedTime = gameTime.ElapsedGameTime.TotalMilliseconds;

            foreach (var key in mKeyStates.Keys) {
                var updatedActions = mKeyStates[key].Update(mCurrentKeyboardState.IsKeyDown(key), passedTime, 0);

                // var duration = mKeyStates[key].mDuration; // TODO: this is commented out so Resharper doesnt complain
                foreach (var action in updatedActions) {
                    mInputActions[action].Origin = mCursorPosition.ToVector2();
                    // mInputActions[action].Duration = duration; // TODO: this is commented out so Resharper doesnt complain
                    mInputActions[action].mUsed = false;
                }
            }

            foreach (var key in mMouseStates.Keys) {
                bool mousePressed = key == MouseButton.Left && mCurrentMouseState.LeftButton == ButtonState.Pressed;
                mousePressed |= key == MouseButton.Right && mCurrentMouseState.RightButton == ButtonState.Pressed;

                var updatedActions = mMouseStates[key].Update(mousePressed, passedTime, mCurrentMouseState.ScrollWheelValue);

                //var duration = mMouseStates[key].mDuration; // TODO: this is commented out so Resharper doesnt complain
                var amount = mMouseStates[key].mAmount;
                foreach (var action in updatedActions) {
                    mInputActions[action].Origin = mCursorPosition.ToVector2();
                    //mInputActions[action].Duration = duration; // TODO: this is commented out so Resharper doesnt complain
                    mInputActions[action].Amount = amount;
                    mInputActions[action].mUsed = false;
                }
            }
        }

        public Dictionary<ActionType, InputAction> GetMappedInputActions() {
            return mInputActions.Where(x => !x.Value.mUsed).ToDictionary(i => i.Key, i => i.Value);
        }

        public static void RemoveUsedActions(Dictionary<ActionType, InputAction> inputActions) {
            foreach (var key in inputActions.Keys) {
                if (inputActions[key].mUsed) {
                    inputActions.Remove(key);
                }
            }
        }
    }

    public sealed class InputAction {
        // public ActionType Type { get; } // TODO: this is commented out so Resharper doesnt complain
        public Vector2 Origin { get; set; }

        //public double Duration { get; set; } // TODO: this is commented out so Resharper doesnt complain

        public int Amount { get; set; }
        public bool mUsed;

        public InputAction(Vector2 origin /*, ActionType type*/) {
            // TODO: this is commented out so Resharper doesnt complain
            Origin = origin;
            // Type = type; // TODO: this is commented out so Resharper doesnt complain
        }
    }
}