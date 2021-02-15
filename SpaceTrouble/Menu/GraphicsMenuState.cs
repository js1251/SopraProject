using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.SaveGameManager;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.Menu {
    internal sealed class GraphicsMenuState : GameState.GameState {
        private Panel Panel { get; set; }
        private MenuButton mBackButton;
        private MenuButton FullscreenButton { get; set; }
        private MenuSlider ResolutionSlider { get; set; }
        private float OldSliderState { get; set; }
        private Point MinScreenResolution { get; }
        private Point MaxScreenResolution { get; }
        private bool IsFullScreen { get; set; }
        private Point mResolution;


        public GraphicsMenuState(string stateName, Point screenResolution) : base(stateName) {
            MaxScreenResolution = screenResolution;
            mResolution = Point.Zero;
            // the smallest resolution is 50% of the monitor max resolution
            MinScreenResolution = (screenResolution.ToVector2() * 0.5f).ToPoint();
        }

        internal override void Initialize() {
        }

        internal override void LoadContent() {
            var buttonTexture = Assets.Textures.InterfaceTextures.Button;
            var font = Assets.Fonts.ButtonFont;

            mBackButton = new MenuButton(buttonTexture, font, "Back");
            var sliderState = (float) SaveLoadManager.LoadSettingAsDouble("Resolution", (float)MinScreenResolution.X / MaxScreenResolution.X);
            ResolutionSlider = new MenuSlider(buttonTexture, font) {SliderState = sliderState};
            IsFullScreen = SaveLoadManager.LoadSettingAsInt("Fullscreen") == 1;
            FullscreenButton = new MenuButton(buttonTexture, font, IsFullScreen ? "Windowed Mode" : "Fullscreen");

            var buttonPanel = new Panel(new Vector4(0.5f, 0.6f, 0.7f, 0.8f), new Vector2(0.05f, 0.025f), new MenuElement[,] {
                { FullscreenButton},
                { ResolutionSlider},
                { mBackButton},
            });

            Panel = new Panel(new Vector4(0.5f, 0.5f, 0.3f, 0.4f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {buttonPanel},
            }, Assets.Textures.InterfaceTextures.GuiMenu);

            CalculateResolution();
            ChangeFullscreen(IsFullScreen);
            ChangeResolution(sliderState);
        }

        internal override void CheckForStateChanges(GameStateManager stateManager, Dictionary<ActionType, InputAction> inputs) {
            if (mBackButton.GetPushState(true)) {
                stateManager.RemoveActiveGameState();
            }

            if (FullscreenButton.GetPushState(true)) {
                IsFullScreen = !IsFullScreen;
                ChangeFullscreen(IsFullScreen);
            }

            if (inputs.ContainsKey(ActionType.MouseDragStop) && Math.Abs(ResolutionSlider.SliderState - OldSliderState) > 0.001f) {
                ChangeResolution(ResolutionSlider.SliderState);
                OldSliderState = ResolutionSlider.SliderState;
            }

            base.CheckForStateChanges(stateManager, inputs);
        }


        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            CalculateResolution();
            ResolutionSlider.Text = mResolution.X + "x" + mResolution.Y;
            Panel.Update(inputs);
        }

        private void CalculateResolution() {
            mResolution.X = (int)((MaxScreenResolution.X - MinScreenResolution.X) * ResolutionSlider.SliderState + MinScreenResolution.X);
            mResolution.Y = (int)((MaxScreenResolution.Y - MinScreenResolution.Y) * ResolutionSlider.SliderState + MinScreenResolution.Y);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            Panel.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void ChangeFullscreen(bool fullscreen) {
            IsFullScreen = fullscreen;
            SpaceTrouble.Graphics.IsFullScreen = fullscreen;
            SpaceTrouble.Graphics.ApplyChanges();

            FullscreenButton.Text = IsFullScreen ? "Windowed Mode" : "Fullscreen";
            SaveLoadManager.SaveSetting("Fullscreen", fullscreen ? 1 : 0);
        }

        private void ChangeResolution(float sliderState) {
            SpaceTrouble.Graphics.PreferredBackBufferWidth = mResolution.X;
            SpaceTrouble.Graphics.PreferredBackBufferHeight = mResolution.Y;
            Global.WindowWidth = SpaceTrouble.Graphics.PreferredBackBufferWidth;
            Global.WindowHeight = SpaceTrouble.Graphics.PreferredBackBufferHeight;
            ResolutionSlider.Text = Global.WindowWidth + "x" + Global.WindowHeight;
            SpaceTrouble.Graphics.IsFullScreen = false;
            SpaceTrouble.Graphics.ApplyChanges();
            SpaceTrouble.Graphics.IsFullScreen = IsFullScreen;
            SpaceTrouble.Graphics.ApplyChanges();
            SpaceTrouble.Camera?.UpdateResolution();

            SaveLoadManager.SaveSetting("Resolution", sliderState);
        }
    }
}