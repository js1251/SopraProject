using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.InputOutput.Camera;
using SpaceTrouble.InputOutput.Cursor;
using SpaceTrouble.Menu;
using SpaceTrouble.Menu.Statistics;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;
using SpaceTrouble.World;
using SpaceTrouble.World.UserInterface;
using Music = SpaceTrouble.InputOutput.Music;

namespace SpaceTrouble {
    internal sealed class SpaceTrouble : Game {
        public static GraphicsDeviceManager Graphics { get; private set; }
        private SpriteBatch SpriteBatch { get; set; }
        internal static CameraManager Camera { get; private set; }

        private InputManager InputManager { get; set; }
        private GameStateManager StateManager { get; set; }
        internal static SoundManager SoundManager { get; private set; }
        internal static Background Background { get; private set; }
        internal static StatsManager StatsManager { get; private set; }

        internal SpaceTrouble() {
            StatsManager = new StatsManager();
            Graphics = new GraphicsDeviceManager(this);
            Background = new Background();
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize() {
            SoundManager = new SoundManager();
            InputManager = new InputManager();
            StateManager = new GameStateManager(InputManager);

            var currentScreenResolution = new Point(GraphicsDevice.Adapter.CurrentDisplayMode.Width, GraphicsDevice.Adapter.CurrentDisplayMode.Height);

            StateManager.AddGameState(new SettingsMenuState("Settings"));
            StateManager.AddGameState(new MainMenuState("MainMenu"), true);
            StateManager.AddGameState(new AudioMenuState("AudioMenu"));
            StateManager.AddGameState(new DifficultyMenuState("DifficultyMenu"));
            StateManager.AddGameState(new StatsMenuState("StatsMenu"));
            StateManager.AddGameState(new WorldGameState("WorldGame"));
            StateManager.AddGameState(new TutorialMenuState("Tutorials"));
            StateManager.AddGameState(new GraphicsMenuState("GraphicsMenu", currentScreenResolution));

            // credit overlay over-top all menus
            StateManager.AddOverlay(new CreditsOverlay("credits"));
            StateManager.AddOverlaysToStates(new[] {"credits"}, new List<string> {
                "Settings", "MainMenu", "AudioMenu", "DifficultyMenu", "StatsMenu", "GraphicsMenu", "Tutorials"
            });

            // GUI overlay for actual GameState
            StateManager.AddOverlay(new GuiOverlay("gui"));
            // GameOver / game won overlay
            StateManager.AddOverlay(new GameFinishedOverlay("GameFinished"));

            StateManager.AddOverlaysToStates(new[] {"gui", "GameFinished" }, new List<string> {"WorldGame"});

            // achievements overlay over-top everything at all times
            StateManager.AddOverlay(new AchievementOverlay("achievements"));
            StateManager.AddOverlaysToStates(new[] { "achievements" });

            // cursor overlay over-top everything at all times
            StateManager.AddOverlay(new CursorOverlay("cursor"));
            StateManager.AddOverlaysToStates(new[] {"cursor"});

            StateManager.Initialize();
            base.Initialize();
            SoundManager.PlayMusic(Music.Regular);
        }

        protected override void LoadContent() {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Assets.LoadContent(Content);
            SoundManager.LoadContent();
            StateManager.LoadContents();
            Camera = new CameraManager(true);
            Background.LoadContent();
            Background.CreateStarFieldScreen();
        }

        protected override void Update(GameTime gameTime) {
            WorldGameState.DebugManager.UpdateTimer.Restart();
            // I shouldn't pass the entire Game, only relevant values - extract global values to seperate Class?
            InputManager.Update(this, StateManager.ActiveGameState);
            var activeStates = StateManager.Update(gameTime);
            if (activeStates == 0) {
                Exit();
            }

            SoundManager.Update();
            Camera.Update(InputManager.GetMappedInputActions());
            base.Update(gameTime);
            WorldGameState.DebugManager.UpdateEnded();
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);
            SpriteBatch.Begin();
            SpriteBatch.Draw(Assets.Textures.InterfaceTextures.Background, new Rectangle(0,0, Global.WindowWidth, Global.WindowHeight), null, Color.White);
            SpriteBatch.End();

            if (!(StateManager.ActiveGameState is WorldGameState)) {
                Background.DrawBackGround(SpriteBatch);
            }
            StateManager.Draw(SpriteBatch);
            if (!(StateManager.ActiveGameState is WorldGameState)) {
                Background.DrawForeGround(SpriteBatch);
            }
            base.Draw(gameTime);
        }
    }
}