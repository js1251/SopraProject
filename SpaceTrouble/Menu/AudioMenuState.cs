using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.SaveGameManager;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.Menu {
    internal sealed class AudioMenuState : GameState.GameState {
        private Panel Panel { get; set; }
        private MenuSlider mMainSlider;
        private MenuSlider mMusicSlider;
        private MenuSlider mEffectSlider;
        private MenuButton mBackButton;

        public AudioMenuState(string stateName) : base(stateName) {
        }

        internal override void Initialize() {}

        internal override void LoadContent() {
            var buttonTexture = Assets.Textures.InterfaceTextures.Button;
            var font = Assets.Fonts.ButtonFont;
            mMainSlider = new MenuSlider(buttonTexture, font) {SliderState = (float) SaveLoadManager.LoadSettingAsDouble("MainVolume", 0.2f) };
            mMusicSlider = new MenuSlider(buttonTexture, font) {SliderState = (float) SaveLoadManager.LoadSettingAsDouble("MusicVolume", 0.2f) };
            mEffectSlider = new MenuSlider(buttonTexture, font) {SliderState = (float) SaveLoadManager.LoadSettingAsDouble("EffectVolume", 0.2f) };
            SpaceTrouble.SoundManager.SetVolume(mMainSlider.SliderState, mMusicSlider.SliderState, mEffectSlider.SliderState);
            mBackButton = new MenuButton(buttonTexture, font, "Back");

            var buttonPanel = new Panel(new Vector4(0.5f, 0.6f, 0.7f, 0.8f), new Vector2(0.05f, 0.025f), new MenuElement[,] {
                {mMainSlider},
                {mMusicSlider},
                {mEffectSlider},
                {mBackButton}
            });

            Panel = new Panel(new Vector4(0.5f, 0.5f, 0.3f, 0.5f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {buttonPanel},
            }, Assets.Textures.InterfaceTextures.GuiMenu);
        }

        internal override void CheckForStateChanges(GameStateManager stateManager, Dictionary<ActionType, InputAction> inputs) {
            if (mBackButton.GetPushState(true)) {
                stateManager.RemoveActiveGameState();
            }

            base.CheckForStateChanges(stateManager, inputs);
        }


        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            Panel.Update(inputs);
            mMainSlider.Text = "Main Volume: " + (int)(mMainSlider.SliderState * 100) + "%";
            mMusicSlider.Text = "Music Volume: " + (int)(mMusicSlider.SliderState * 100) + "%";
            mEffectSlider.Text = "Effect Volume: " + (int)(mEffectSlider.SliderState * 100) + "%";
            SpaceTrouble.SoundManager.SetVolume(mMainSlider.SliderState, mMusicSlider.SliderState, mEffectSlider.SliderState);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            Panel.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}