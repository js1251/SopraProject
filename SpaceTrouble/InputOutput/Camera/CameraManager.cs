using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceTrouble.util.Tools;
using Microsoft.Xna.Framework.Input;

// Created by Jakob Sailer

namespace SpaceTrouble.InputOutput.Camera {
    internal sealed class CameraManager {
        public Vector2 CameraPos { private set; get; } // the point at which the camera looks at in the world
        public Vector2 CameraOffset { get; private set; } // the focus point of the camera on-screen (aka in the middle etc)
        private Vector2 WorldCenter { get; set; }
        private KeyboardState mCurrentKeyboardState;

        private bool mCursorMode;
        public bool CursorMode {
            get => mCursorMode;
            set {
                mCursorMode = value;
                WorldCenter = value ? new Vector2(Global.WindowWidth, Global.WindowHeight) / 2f : CoordinateManager.TileToWorld(Global.mWorldOrigin);
                CameraPos = WorldCenter;
            }
        }

        // zooming
        public float CameraZoom { private set; get; }
        private float mWindowZoom;
        private readonly float[] mZoomLimit = {0.25f, 2f};

        // scrolling
        private static float TriggerFactor { get; } = 60f;
        private float ScrollTriggerWidth { get; set; } = Global.WindowHeight / TriggerFactor;
        private const float ScrollAmount = 4;
        private static Vector2 sEmptyTileBorder;
        private const int TilesVisibleOnScreen = 10;

        public CameraManager(bool cursorMode) {
            CursorMode = cursorMode;
            // camera focus is in the middle of the screen
            CameraOffset = new Vector2(Global.WindowWidth / 2f, Global.WindowHeight / 2f);

            // zooming
            mWindowZoom = (float) Global.WindowWidth / (Global.TileWidth * TilesVisibleOnScreen);
            CameraZoom = mWindowZoom;
            // scrolling
            const int limitOffsetTiles = 8;
            sEmptyTileBorder = new Vector2(Global.TileWidth, Global.TileHeight);
            sEmptyTileBorder *= limitOffsetTiles / CameraZoom;
        }

        /* Big thanks to David Amador
         * He has a great page about camera translation in XNA
         * http://www.david-amador.com/2009/10/xna-camera-2d-with-zoom-and-rotation/
         */
        public Matrix GetTransformation() {
            var transform =
                Matrix.CreateTranslation(new Vector3(-CameraPos.X, -CameraPos.Y, 0)) *
                Matrix.CreateScale(new Vector3(CameraZoom, CameraZoom, 1)) *
                Matrix.CreateTranslation(new Vector3(CameraOffset.X, CameraOffset.Y, 0));
            return transform;
        }

        public Matrix GetBackgroundTransformation(float distance) {
            var distanceOffset = (WorldCenter - CameraPos) / distance;
            var zoomOffset = mWindowZoom * 0.25f + CameraZoom / distance;

            var transform =
                Matrix.CreateTranslation(new Vector3(-WorldCenter.X, -WorldCenter.Y, 0)) *
                Matrix.CreateTranslation(new Vector3(distanceOffset.X, distanceOffset.Y, 0)) *
                Matrix.CreateScale(new Vector3(zoomOffset, zoomOffset, 1)) *
                Matrix.CreateTranslation(new Vector3(CameraOffset.X, CameraOffset.Y, 0));
            return transform;
        }

        /// <summary>
        /// Updates the camera-position based on User input (window-edge scrolling).
        /// </summary>
        public void Update(Dictionary<ActionType, InputAction> inputs) {
            mCurrentKeyboardState = Keyboard.GetState();

            if (!CursorMode) {
                // Update cameraPosition
                UpdateTranslation(inputs[ActionType.MouseMoved].Origin);
            } else {
                var cursorOffsetFromCenter = inputs[ActionType.MouseMoved].Origin - WorldCenter;
                CameraPos = WorldCenter + cursorOffsetFromCenter * 0.1f;
            }

            if (inputs.TryGetValue(ActionType.MouseScrolled, out var input)) {
                // Update cameraZoom
                UpdateZoom(input.Amount);
            }
        }

        public void UpdateResolution() {
            CameraOffset = new Vector2(Global.WindowWidth / 2f, Global.WindowHeight / 2f);
            ScrollTriggerWidth = Global.WindowHeight / TriggerFactor;
            mWindowZoom = (float) Global.WindowWidth / (Global.TileWidth * TilesVisibleOnScreen);
        }

        private void UpdateTranslation(Vector2 cursorPosition) {
            // left or right
            if ((cursorPosition.X <= ScrollTriggerWidth) || (mCurrentKeyboardState.IsKeyDown(Keys.A))) {
                CameraPos += new Vector2(-ScrollAmount, 0);
                if ((cursorPosition.X <= ScrollTriggerWidth) && (mCurrentKeyboardState.IsKeyDown(Keys.A))) {
                    CameraPos += new Vector2(-ScrollAmount, 0);
                }
            } else if ((cursorPosition.X >= Global.WindowWidth - ScrollTriggerWidth) || (mCurrentKeyboardState.IsKeyDown(Keys.D))) {
                CameraPos += new Vector2(ScrollAmount, 0);
                if ((cursorPosition.X >= Global.WindowWidth - ScrollTriggerWidth) && (mCurrentKeyboardState.IsKeyDown(Keys.D))) {
                    CameraPos += new Vector2(ScrollAmount, 0);
                }
            }

            // top or bottom
            if ((cursorPosition.Y <= ScrollTriggerWidth) || (mCurrentKeyboardState.IsKeyDown(Keys.W))) {
                CameraPos += new Vector2(0, -ScrollAmount);
                if ((cursorPosition.Y <= ScrollTriggerWidth) && (mCurrentKeyboardState.IsKeyDown(Keys.W))) {
                    CameraPos += new Vector2(0, -ScrollAmount);
                }
            } else if ((cursorPosition.Y >= Global.WindowHeight - ScrollTriggerWidth) || (mCurrentKeyboardState.IsKeyDown(Keys.S))) {
                CameraPos += new Vector2(0, ScrollAmount);
                if ((cursorPosition.Y >= Global.WindowHeight - ScrollTriggerWidth) && (mCurrentKeyboardState.IsKeyDown(Keys.S))) {
                    CameraPos += new Vector2(0, ScrollAmount);
                }
            }

            // the camera is limited to focus on the most outer tiles
            var xLimit = new[] {
                CoordinateManager.TileToWorld(new Vector2(0, Global.WorldWidth - 1)).X,
                CoordinateManager.TileToWorld(new Vector2(Global.WorldWidth - 1, 0)).X + Global.TileWidth
            };
            // since half of the screen is now empty the limit can be offset inwards by a couple tiles
            xLimit[0] += sEmptyTileBorder.X;
            xLimit[1] -= sEmptyTileBorder.X;

            // do the same limiting for the Y axis
            var yLimit = new[] {
                CoordinateManager.TileToWorld(Vector2.Zero).Y,
                CoordinateManager.TileToWorld(new Vector2(Global.WorldWidth - 1, Global.WorldHeight - 1)).Y + Global.TileHeight
            };
            yLimit[0] += sEmptyTileBorder.Y;
            yLimit[1] -= sEmptyTileBorder.Y;

            CameraPos = new Vector2 {
                X = Math.Clamp(CameraPos.X, xLimit[0], xLimit[1]),
                Y = Math.Clamp(CameraPos.Y, yLimit[0], yLimit[1])
            };
        }

        private void UpdateZoom(int scroll) {
            if (scroll < 0) {
                CameraZoom -= 0.2f;
            } else if (scroll > 0) {
                CameraZoom += 0.2f;
            }

            CameraZoom = Math.Clamp(CameraZoom, mWindowZoom * mZoomLimit[0], mWindowZoom * mZoomLimit[1]);
        }
    }
}