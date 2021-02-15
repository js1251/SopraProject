using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.Menu.Statistics {
    internal sealed class StatisticsPanel {
        private Dictionary<Statistic, (Label, Label)> StatisticLabel { get; }
        private float MeterConversion { get; }
        private Vector4 RelativeBounds { get; }

        public StatisticsPanel(Vector4 relativeBounds) {
            StatisticLabel = new Dictionary<Statistic, (Label, Label)>();
            RelativeBounds = relativeBounds;
            const float minionHeightInMeters = 2f; // assuming a minion is 2m tall
            MeterConversion = minionHeightInMeters / 25;
            // 25 is the Minion.Dimension.Y - I cant get it from a new Minion since that will try to access classes that don't exist yet
        }

        internal void UpdateValues() {
            foreach (var (type, (_, content)) in StatisticLabel) {
                var value = SpaceTrouble.StatsManager.Statistics[type];
                if (type == Statistic.MinionTraveledDistance) {
                    value = (float) Math.Round(value);
                    content.Text = value + "u / " + (float) Math.Round(value / Global.TileWidth) + "t / ";

                    content.Text += (float) Math.Round(value * MeterConversion) + "m";
                    continue;
                }

                if (type == Statistic.TimePlayed) {
                    if (value > 89999) {
                        content.Text = new TimeSpan(0, 0, 0, (int)value).ToString(@"dd\:hh\:mm\:ss") + " ... over a day of time";
                    } else if (value < 8643599) {
                        content.Text = new TimeSpan(0, 0, 0, (int)value).ToString(@"hh\:mm\:ss");
                    } else {
                        content.Text = "You need to stop";
                    }
                    continue;
                }

                content.Text = value + "";
            }
        }

        internal Panel CreatePanel() {
            // add two more labels into this panel : description -> value
            foreach (var statEnum in (Statistic[]) Enum.GetValues(typeof(Statistic))) {
                StatisticLabel.Add(statEnum, (
                    new Label(Assets.Fonts.GuiFont01, default, StatsMenuState.EnumToString(statEnum.ToString())),
                    new Label(Assets.Fonts.GuiFont01))
                );
            }

            // the panel that actually displays the values
            return new Panel(RelativeBounds, new Vector2(0.05f, 0.05f), GetStatEntries());
        }

        // Generates an array of menuElements for all statistics
        private MenuElement[,] GetStatEntries() {
            var count = StatisticLabel.Keys.Count;
            var entries = new MenuElement[count, 2];
            var index = 0;
            foreach (var (_, (description, content)) in StatisticLabel) {
                entries[index, 0] = description;
                entries[index, 1] = content;
                index++;
            }

            return entries;
        }
    }
}