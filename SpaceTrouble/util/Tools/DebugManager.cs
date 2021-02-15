using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.Menu.Statistics;
using SpaceTrouble.World;

// Created by Jakob Sailer

namespace SpaceTrouble.util.Tools {
    /// <summary>
    /// Draws a bunch of debug information over the game-world.
    /// - where the cursor is on the screen
    /// - where the cursor is in the world
    /// - which cell the cursor is in
    /// - which tile the cursor is over
    /// </summary>
    public enum DebugMode {
        None,
        InformationOnly,
        Drawing,
        NavigationWalkingMesh,
        NavigationWalkingNodes,
        NavigationFlying,
        ObstacleAvoidance,
        Collision
    }

    internal sealed class DebugManager {
        private int mFrameTimeDraw;
        private int mFrameTimeUpdate;
        public System.Diagnostics.Stopwatch UpdateTimer { get; } = new System.Diagnostics.Stopwatch();
        private ObjectManager ObjectManager { get; }
        private NavigationManager NavigationManager { get; }

        private DebugMode Mode { get; set; }
        private Vector2 mCursorPos;

        private Panel mPanel;
        private SpriteFont DebugFont { get; set; }
        private readonly Dictionary<string, Label> mDebugValues = new Dictionary<string, Label>();

        public DebugManager() {
            mFrameTimeDraw = 0;
            mFrameTimeUpdate = 0;
            ObjectManager = WorldGameState.ObjectManager;
            NavigationManager = WorldGameState.NavigationManager;

            SileceReshaperMethod();
        }

        private void SileceReshaperMethod() {
            // since the enum "InformationOnly" IS used for a label but Reshaper doesn't know it this is done
            Mode = DebugMode.InformationOnly;
            Mode = DebugMode.None;
        }

        internal void LoadContent() {
            DebugFont = Assets.Assets.Fonts.ButtonFont;

            mPanel = new Panel(new Vector4(0,0,0.3f,0.3f), new Vector2(0.05f, 0.05f), new [,] {
                {AddDebugValueTracker(DebugFont, "Debug Mode", Color.White), mDebugValues["Debug Mode"]},
                {null, null},
                {AddDebugValueTracker(DebugFont, "UpdateFPS", Color.White), mDebugValues["UpdateFPS"]},
                {AddDebugValueTracker(DebugFont, "MS per Update", Color.White), mDebugValues["MS per Update"]},
                {AddDebugValueTracker(DebugFont, "DrawFPS", Color.White), mDebugValues["DrawFPS"]},
                {AddDebugValueTracker(DebugFont, "Cursor", Color.White), mDebugValues["Cursor"]},
                {AddDebugValueTracker(DebugFont, "World", Color.White), mDebugValues["World"]},
                {AddDebugValueTracker(DebugFont, "Cell", Color.White), mDebugValues["Cell"]},
                {AddDebugValueTracker(DebugFont, "Tile", Color.White), mDebugValues["Tile"]},
                {AddDebugValueTracker(DebugFont, "Objects sorted on Update", Color.White), mDebugValues["Objects sorted on Update"]},
                {AddDebugValueTracker(DebugFont, "Objects drawn", Color.White), mDebugValues["Objects drawn"]},
                {AddDebugValueTracker(DebugFont, "Nav-Quads", Color.White), mDebugValues["Nav-Quads"]}
            });
        }

        /// <summary>
        /// Add a row to to the panel tracking the debug values. Is a function to make things easier to read.
        /// </summary>
        private MenuElement AddDebugValueTracker(SpriteFont font, string name, Color color = default, string message = null) {
            message ??= name;
            var description = new Label(font, color, message);
            mDebugValues.Add(name, new Label(font, color));
            return description;
        }

        internal void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            GetModeAndMouse(inputs);
            UpdateInformation(gameTime, inputs);
            HandleForceBuild(inputs);
        }

        private void GetModeAndMouse(Dictionary<ActionType, InputAction> inputs) {
            // toggle through debugModes
            if (inputs.ContainsKey(ActionType.DebugToggled)) {
                Mode = (DebugMode) ((int) (Mode + 1) % Enum.GetValues(typeof(DebugMode)).Length);
                SpaceTrouble.StatsManager.IncreaseAchievement(Achievement.BehindTheScenes, 1);
            }

            // set the debug mode of the draw data-structure to tru if in mode "Drawing"
            ObjectManager.DataStructure.DrawData.IsDebug = Mode is DebugMode.Drawing;

            // get the cursorPosition
            if (inputs.TryGetValue(ActionType.MouseMoved, out var input)) {
                mCursorPos = input.Origin;
            }
        }

        private void HandleForceBuild(Dictionary<ActionType, InputAction> inputs) {
            if (inputs.ContainsKey(ActionType.ForceBuild)) {
                var tile = ObjectManager.GetTile(CoordinateManager.ScreenToTile(mCursorPos));
                if (tile is IBuildable buildable && !buildable.BuildingFinished) {
                    if (NavigationManager.GetQuad(tile.WorldPosition) != null) {
                        buildable.AddResource(buildable.RequiredResources);
                        SpaceTrouble.StatsManager.IncreaseAchievement(Achievement.YouCheated, 1);
                    }
                }
            }
        }

        // called from the overarching class when ALL update calculations are done
        public void UpdateEnded() {
            UpdateTimer.Stop();
            mDebugValues["MS per Update"].Text = UpdateTimer.ElapsedMilliseconds.ToString();
        }

        private void UpdateInformation(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            // update the UPS time
            mFrameTimeUpdate = (int) (1 / gameTime.ElapsedGameTime.TotalSeconds);
            mDebugValues["UpdateFPS"].Text = mFrameTimeUpdate.ToString();

            // Screen-coordinates of cursor
            mDebugValues["Cursor"].Text = FormatVector(mCursorPos);

            // World-coordinates underneath cursor
            var worldCoordinates = CoordinateManager.ScreenToWorld(mCursorPos);
            worldCoordinates.X = (int) worldCoordinates.X;
            worldCoordinates.Y = (int) worldCoordinates.Y;
            mDebugValues["World"].Text = FormatVector(worldCoordinates);

            // Cell-coordinates underneath cursor
            mDebugValues["Cell"].Text = FormatVector(CoordinateManager.ScreenToCell(mCursorPos));

            // Tile-coordinates underneath cursor
            mDebugValues["Tile"].Text = FormatVector(CoordinateManager.ScreenToTile(mCursorPos));

            // Number of objects drawn out of total objects in the world
            var totalObjects = ObjectManager.GetAllObjects().Count;
            var drawnObjects = ObjectManager.DataStructure.DrawData.GameObjectDrawOrder;
            mDebugValues["Objects drawn"].Text = drawnObjects.Count + " / " + totalObjects;

            // Number of objects that are being actively sorted
            var sortedObjects = 0;
            foreach (var gameObject in drawnObjects) {
                if (!(gameObject is Tile)) {
                    sortedObjects++;
                }
            }

            mDebugValues["Objects sorted on Update"].Text = sortedObjects.ToString();

            // Number of navmesh quads drawn
            var drawQuads = NavigationManager.ConvexHulls.Count;
            mDebugValues["Nav-Quads"].Text = drawQuads.ToString();

            // The current debug mode
            mDebugValues["Debug Mode"].Text = Mode.ToString();

            // finally update the panel itself
            mPanel.Update(inputs);
        }

        private static string FormatVector(Vector2 vector) {
            return vector.X + ", " + vector.Y;
        }

        internal void DrawWorld(SpriteBatch spriteBatch) {
            if (Mode is DebugMode.Drawing) {
                ObjectManager.DataStructure.DrawData.DrawDebug(spriteBatch, DebugFont, mCursorPos);
            }

            NavigationManager.DrawDebug(spriteBatch, Mode);
            ObjectManager.DataStructure.ObjectData.DrawDebug(spriteBatch, Mode);
        }

        internal void DrawScreen(SpriteBatch spriteBatch) {
            if (Mode is DebugMode.None) {
                return;
            }

            DrawInfoPanel(spriteBatch);
        }

        private void DrawInfoPanel(SpriteBatch spriteBatch) {
            // Draw FPS Update !!This needs to be done in Draw! otherwise UPS is just measured twice
            var drawFps = (int) (1f / ((Environment.TickCount - mFrameTimeDraw) / 1000f));
            mFrameTimeDraw = Environment.TickCount;
            mDebugValues["DrawFPS"].Text = drawFps.ToString();

            // draw debug values
            mPanel.Draw(spriteBatch);
        }
    }
}