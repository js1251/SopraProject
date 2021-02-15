using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.Menu.Statistics {
    internal sealed class AchievementsPanel {
        private Vector4 RelativeBounds { get; }
        private Dictionary<Achievement, (Label, MenuBar)> AchievementBars { get; }

        public AchievementsPanel(Vector4 relativeBounds) {
            RelativeBounds = relativeBounds;
            AchievementBars = new Dictionary<Achievement, (Label, MenuBar)>();
        }

        internal void UpdateValues() {
            foreach (var (achievement, (label, bar)) in AchievementBars) {
                var color = SpaceTrouble.StatsManager.Achievements[achievement].Item2 >= 1 ? Color.LimeGreen : default;
                bar.FillAmount = SpaceTrouble.StatsManager.Achievements[achievement].Item2; 
                
                string barText;
                if (achievement.ToString().StartsWith("Played")) {
                    var current = SpaceTrouble.StatsManager.Achievements[achievement].Item1 / 3600;
                    var limit = SpaceTrouble.StatsManager.AchievementLimits[achievement] / 3600;
                    barText = Math.Round(current, 2) + " / " + limit;
                } else {
                    barText = SpaceTrouble.StatsManager.Achievements[achievement].Item1 + " / " + SpaceTrouble.StatsManager.AchievementLimits[achievement];
                }

                bar.Text = barText;
                
                bar.TextColor = color;
                label.TextColor = color;
            }
        }

        internal Panel CreatePanel() {
            foreach (var achievement in (Achievement[])Enum.GetValues(typeof(Achievement))) {
                AchievementBars.Add(achievement, (
                    new Label(Assets.Fonts.GuiFont01, default, StatsMenuState.EnumToString(achievement.ToString())),
                    new MenuBar(Assets.Textures.InterfaceTextures.GuiBar, Color.DarkBlue, Assets.Fonts.GuiFont01))
                );
            }

            return new Panel(RelativeBounds, new Vector2(0.05f, 0.01f), GetAchievementEntries());
        }

        private MenuElement[,] GetAchievementEntries() {
            var count = AchievementBars.Keys.Count;
            var entries = new MenuElement[count, 2];
            var index = 0;
            foreach (var (_, (label, bar)) in AchievementBars) {
                entries[index, 0] = label;
                var relations = new Vector4(0.5f, 0.5f, 1, 1);
                
                var backGroundPanel = new Panel(relations, Vector2.Zero, new MenuElement[,]{}, Assets.Textures.InterfaceTextures.GuiBar);
                var barPanel = new Panel(relations, Vector2.Zero, new MenuElement[,] {
                    {bar}
                });

                var enclosingPanel = new Panel(default, Vector2.Zero, new MenuElement[,] {
                    {backGroundPanel, barPanel}
                });

                entries[index, 1] = enclosingPanel;
                index ++;
            }
            

            return entries;
        }
    }
}
