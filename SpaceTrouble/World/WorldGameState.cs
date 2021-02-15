using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.Statistics;
using SpaceTrouble.SaveGameManager;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World.HighlightingEffects;

// Created by Jakob Sailer but modified by others

namespace SpaceTrouble.World {
    internal sealed class WorldGameState : GameState.GameState {
        // Game Components
        internal static ObjectManager ObjectManager { get; private set; }
        internal static NavigationManager NavigationManager { get; private set; }
        internal static GameMaster GameMaster { get; set; }
        public static DifficultyManager DifficultyManager { get; set; }

        // interfaces
        internal static BuildingMenu BuildingMenu { get; private set; }
        internal static TaskManager TaskManager { get; set; }
        internal static PriorityManager PriorityManager { get; set; }

        // visuals
        internal static Highlighting Highlighting { get; private set; }

        // Debugging
        internal static bool IsTechDemo { get; private set; }
        internal static DebugManager DebugManager { get; private set; }

        // Pausing / GameSpeed
        internal static bool IsPaused { get; private set; }
        private TimeSpan OldGameTime { get; set; }
        private TimeSpan OldElapsedGameTime { get; set; }
        internal static bool IsGameFinished => GameMaster.GameFinished;
        private static int sUpdatesPerUpdate = 1;
        internal static int UpdatesPerUpdate {
            get => sUpdatesPerUpdate;
            set => sUpdatesPerUpdate = IsTechDemo ? 1 : value;
        }
        internal static bool GameIsRunning { get; private set; }

        public WorldGameState(string stateName) : base(stateName) {
            ObjectManager = new ObjectManager();
            NavigationManager = new NavigationManager();
            DifficultyManager = new DifficultyManager();
            GameMaster = new GameMaster();
            BuildingMenu = new BuildingMenu();
            TaskManager = new TaskManager();
            PriorityManager = new PriorityManager();
            Highlighting = new Highlighting();
            CaptureCursor = true;
            DebugManager = new DebugManager();
        }

        internal override void Initialize() {
            SaveLoadManager.LoadSettingsFromFile();
        }

        internal override void LoadContent() {
            ObjectManager.LoadContent();
            Highlighting.LoadContent();
            DebugManager.LoadContent();
        }

        internal override void Activated(GameTime gameTime, HashSet<string> messages) {
            SpaceTrouble.Camera.CursorMode = false;
            SpaceTrouble.Background.CreateWorldOutline();
            SpaceTrouble.Background.CreateStarFieldWorld();
            GameIsRunning = true;

            if (messages.Contains("continue")) {
                // simply continue the current active Game
                gameTime.TotalGameTime = OldGameTime;
                gameTime.ElapsedGameTime = OldElapsedGameTime;
                messages.Remove("continue");
                return;
            }

            // for all other cases reset the game
            ResetGame(gameTime);

            if (messages.Contains("load")) {
                // load a save-file
                LoadGame(gameTime);
                messages.Remove("load");
                return;
            }

            if (messages.Contains("start new game")) {
                // create a new game
                new WorldGenerator().CreateNewWorld();
                messages.Remove("start new game");
                return;
            }

            if (messages.Contains("techdemo")) {
                DifficultyManager.Difficulty = DifficultyEnum.Normal;
                new TechdemoGenerator().CreateTechdemo();
                IsTechDemo = true;
                messages.Remove("techdemo");
            }
        }

        private void LoadGame(GameTime gameTime) {
            NavigationManager.IsWorldCreation = true;
            var worldGenerator = new WorldGenerator();
            worldGenerator.InitializeGrid();
            SaveLoadManager.LoadGameObjects();
            worldGenerator.InitializeNavigation();
            SaveLoadManager.LoadGameState(gameTime);
            PriorityManager.LoadSavedMapping();
        }

        private void ResetGame(GameTime gameTime) {
            GameMaster = new GameMaster();
            BuildingMenu = new BuildingMenu();
            TaskManager = new TaskManager();
            PriorityManager = new PriorityManager();
            DifficultyManager = new DifficultyManager();

            ObjectManager.RemoveAll();
            Highlighting.Reset();
            Highlighting.HighlightPortals();
            UpdatesPerUpdate = 1;
            IsPaused = false;
            IsTechDemo = false;

            gameTime.TotalGameTime = new TimeSpan(0);
            gameTime.ElapsedGameTime = new TimeSpan(0);
            OldGameTime = gameTime.TotalGameTime;
            OldElapsedGameTime = gameTime.ElapsedGameTime;
        }

        internal override void Deactivated(GameTime gameTime) {
            SpaceTrouble.Camera.CursorMode = true;
            PriorityManager.CreateSaveLoadMapping();
            SpaceTrouble.Background.CreateStarFieldScreen();
            SpaceTrouble.StatsManager.SaveStatistics();
            if (!IsPaused)
            {
                OldGameTime = gameTime.TotalGameTime;
                OldElapsedGameTime = gameTime.ElapsedGameTime;
            }
            if (!IsTechDemo) {
                SaveLoadManager.SaveGameObjects();
                SaveLoadManager.SaveGameState(gameTime);
            }
        }
    
        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            if (inputs.ContainsKey(ActionType.Pause)) {
                if (!IsPaused) {
                    OldGameTime = gameTime.TotalGameTime; //memorizes GameTime to stop spawn of enemies while paused
                    OldElapsedGameTime = gameTime.ElapsedGameTime; // memorizes ElapsedGameTime
                    IsPaused = true;
                } else {
                    gameTime.TotalGameTime = OldGameTime; //sets GameTime back to the time, before game was paused
                    gameTime.ElapsedGameTime = OldElapsedGameTime; //sets ElapsedGameTime back to the time, before game was paused
                    IsPaused = false;
                }
            }

            for (var i = 0; i < UpdatesPerUpdate; i++) {
                if (!(IsPaused || IsGameFinished)) {
                    BuildingMenu.Update(inputs);
                }

                ObjectManager.Update(gameTime);
                GameMaster.Update(gameTime);

                // if the game runs faster the gameTime also needs to run faster
                if (i > 0) {
                    gameTime.TotalGameTime += gameTime.ElapsedGameTime;
                }
            }

            if (!IsPaused) {
                SpaceTrouble.StatsManager.AddValue(Statistic.TimePlayed, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            TaskManager.Update();
            Highlighting.Update(gameTime, inputs);
            PriorityManager.Update(gameTime, inputs);
            DebugManager.Update(gameTime, inputs);
        }

        public override void Draw(SpriteBatch spriteBatch) {

            // Draw anything relative to the background
            SpaceTrouble.Background.DrawBackGround(spriteBatch);

            // Draw anything that is relative to the world
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null, null, SpaceTrouble.Camera.GetTransformation());
            SpaceTrouble.Background.DrawWorld(spriteBatch);
            ObjectManager.Draw(spriteBatch);
            Highlighting.Draw(spriteBatch);
            DebugManager.DrawWorld(spriteBatch);
            spriteBatch.End();

            // Draw anything relative to the foreground
            SpaceTrouble.Background.DrawForeGround(spriteBatch);

            // Draw anything that is relative to the screen
            spriteBatch.Begin();
            DebugManager.DrawScreen(spriteBatch);
            spriteBatch.End();
        }
    }
}