using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.World.UserInterface {
    internal sealed class ProgressUi : UiElement {
        private Label GameTime { get; set; }
        private Label WaveInfo { get; set; }
        private List<MenuButton> SpeedButtons { get; }
        private Panel SpeedPanel { get; set; }

        public ProgressUi(Vector4 screenBounds) : base(screenBounds) {
            SpeedButtons = new List<MenuButton>();
        }

        internal override void LoadContent() {
            GameTime = new Label(Assets.Fonts.GuiFont01);
            WaveInfo = new Label(Assets.Fonts.GuiFont01, default, "", 13f);

            var backgroundPanel = new Panel(new Vector4(0, 0, 1, 1), Vector2.Zero, new MenuElement[,] {
            }, Assets.Textures.InterfaceTextures.GuiTime);

            var timePanel = new Panel(new Vector4(0,0.3f,1,0.6f), new Vector2(0.05f,0.05f), new MenuElement[,] {
                {GameTime},
                {WaveInfo}
            });

            var speedBackground = Assets.Textures.InterfaceTextures.GuiGameSpeed;
            SpeedButtons.Add(new MenuButton(Assets.Textures.InterfaceTextures.ButtonPause));
            SpeedButtons.Add(new MenuButton(Assets.Textures.InterfaceTextures.Button1X));
            SpeedButtons.Add(new MenuButton(Assets.Textures.InterfaceTextures.Button2X));
            SpeedButtons.Add(new MenuButton(Assets.Textures.InterfaceTextures.Button3X));

            var buttonPanel = new Panel(new Vector4(0.5f, 0.6f, 0.85f, 0.4f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {SpeedButtons[0], SpeedButtons[1], SpeedButtons[2], SpeedButtons[3]}
            });

            SpeedPanel = new Panel(new Vector4(0.5f, 0.5f, 0.62f, 1f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {buttonPanel}
            }, speedBackground);

            Panel = new Panel(ScreenBounds, new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {SpeedPanel, backgroundPanel, timePanel}
            });
        }

        internal override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            base.Update(gameTime, inputs);
            UpdateTimers(gameTime);
            UpdateSpeedButtons(gameTime, inputs);
        }

        private void UpdateTimers(GameTime gameTime) {
            if (WorldGameState.IsPaused || WorldGameState.IsGameFinished) {
                return;
            }

            GameTime.Text = gameTime.TotalGameTime.ToString(@"hh\:mm\:ss");
            GameTime.Text += WorldGameState.UpdatesPerUpdate > 1 ? " (" + WorldGameState.UpdatesPerUpdate + "x)" : "";
            var countdown = (int) (WorldGameState.GameMaster.WaitTime - WorldGameState.GameMaster.TimeSinceLastWave);
            var enemyCount = WorldGameState.ObjectManager.GetAllObjects(ObjectProperty.Enemy).Count;
            if (enemyCount <= 0) {
                WaveInfo.Text = "Wave " + WorldGameState.GameMaster.WaveNumber + " in " + new TimeSpan(0, 0, countdown + 1).ToString(@"m\:ss");
            } else {
                if (WorldGameState.IsTechDemo) {
                    WaveInfo.Text = enemyCount + " Enemies!";
                } else if (WorldGameState.GameMaster.IsFinalWave) {
                    countdown = (int)WorldGameState.GameMaster.FinalWaveCountdown;
                    WaveInfo.Text = "Portals close in " + new TimeSpan(0, 0, countdown + 1).ToString(@"m\:ss");
                } else {
                    WaveInfo.Text = "Wave " + WorldGameState.GameMaster.WaveNumber + " - Enemies: " + enemyCount;
                }
            }

            WaveInfo.TextColor = countdown < 10 ? Color.OrangeRed : default;
        }

        private void UpdateSpeedButtons(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            if (!HighlightSpeedPanel(gameTime, inputs)) {
                return;
            }

            for (var i = 0; i < SpeedButtons.Count; i++) {
                var button = SpeedButtons[i];
                if (button.GetPushState(true)) {
                    if (i == 0) {
                        inputs.Add(ActionType.Pause, new InputAction(Vector2.Zero));
                    } else {
                        WorldGameState.UpdatesPerUpdate = i;
                        if (WorldGameState.IsPaused) {
                            inputs.Add(ActionType.Pause, new InputAction(Vector2.Zero));
                        }
                    }
                }
            }
        }

        private bool HighlightSpeedPanel(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            if (!inputs.TryGetValue(ActionType.MouseMoved, out var input)) {
                return false;
            }

            var panelOffset = (float) gameTime.ElapsedGameTime.TotalSeconds;
            var triggerBounds = new Rectangle {
                X = Panel.mBounds.X,
                Y = Panel.mBounds.Y,
                Width = Panel.mBounds.Width,
                Height = (int)(Panel.mBounds.Height * 1.5f)
            };

            if (triggerBounds.Contains(input.Origin)) {
                SpeedPanel.Offset = new Vector2(0, Math.Clamp(SpeedPanel.Offset.Y + panelOffset * 4f, 0, 0.6f));
                return true;
            }

            SpeedPanel.Offset = new Vector2(0, Math.Clamp(SpeedPanel.Offset.Y - panelOffset * 2f, 0, 1));

            return false;
        }
    }
}