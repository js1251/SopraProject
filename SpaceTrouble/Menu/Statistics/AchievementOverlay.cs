using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.Menu.Statistics {
    internal sealed class AchievementOverlay : GameStateOverlay {

        private Panel PopUpPanel { get; set; }
        private Label AchievementDescription { get; set; }
        private bool FullyOffset { get; set; }
        private float DisplayDuration { get; }
        private float TimeSinceFullOffset { get; set; }
        public AchievementOverlay(string overlayName/*, int priority*/, bool active = true) : base(overlayName/*, priority*/, active) {
            DisplayDuration = 3f;
            TimeSinceFullOffset = 0f;
        }

        internal override void LoadContent() {
            AchievementDescription = new Label(Assets.Fonts.ButtonFont, default, "", 20f);

            var infoPanel = new Panel(new Vector4(0.5f,0.5f,0.8f,0.5f), new Vector2(0.05f, 0.05f), new MenuElement[,] {
                {new Label(Assets.Fonts.ButtonFont, default, "Achievement Unlocked!", 24f) },
                {AchievementDescription}
            });

            PopUpPanel = new Panel(new Vector4(0.95f, 0, 0.2f, 0.15f), new Vector2(0.05f, 0.01f), new MenuElement[,] {
                {infoPanel}
            }, Assets.Textures.InterfaceTextures.GuiPopup);

            PopUpPanel.Offset = new Vector2(0, -1f);
        }

        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            var pendingAchievements = SpaceTrouble.StatsManager.PendingAchievements;

            if (pendingAchievements.Count > 0) {
                UpdatePopup(gameTime, pendingAchievements);
            }

            PopUpPanel.Update(inputs);
        }

        private void UpdatePopup(GameTime gameTime, List<Achievement> pendingAchievements) {
            if (PopUpPanel.Offset.Y <= -1) {
                AchievementDescription.Text = StatsMenuState.EnumToString(pendingAchievements[0].ToString()).Replace(":", "");
                SpaceTrouble.SoundManager.PlaySound(Sound.Achievement);
            }

            if (PopUpPanel.Offset.Y < 0 && !FullyOffset) {
                PopUpPanel.Offset = new Vector2(0, PopUpPanel.Offset.Y + (float)gameTime.ElapsedGameTime.TotalSeconds);
            } else {
                FullyOffset = true;
                TimeSinceFullOffset += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (FullyOffset && TimeSinceFullOffset >= DisplayDuration) {
                PopUpPanel.Offset = new Vector2(0, PopUpPanel.Offset.Y - (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            if (PopUpPanel.Offset.Y <= -1) {
                PopUpPanel.Offset = new Vector2(0,-1f);
                FullyOffset = false;
                TimeSinceFullOffset = 0;
                pendingAchievements.RemoveAt(0);
            }
        }

        public override void Draw(SpriteBatch spriteBatch) {
            PopUpPanel.Draw(spriteBatch);
        }
    }
}
