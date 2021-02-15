using System;
using System.Collections.Generic;
using SpaceTrouble.SaveGameManager;
using SpaceTrouble.World;

namespace SpaceTrouble.Menu.Statistics {
    internal enum Statistic {
        TimePlayed,
        MinionsSpawned,
        MinionsKilled,
        MinionTraveledDistance,
        EnemiesKilled,
        BuildingsConstructed,
        LaboratoriesBuilt,
        ResourceGenerated,
        WavesDefeated,
        GamesStarted,
        GamesLost,
        GamesWon,
    }

    internal enum Achievement {
        Killed10Enemies,
        Killed100Enemies,
        Killed1000Enemies,
        Killed10000Enemies,
        Lost1Game,
        Lost10Games,
        Lost100Games,
        Won1Game,
        Won10Games,
        Won100Games,
        PlayedFor1Hour,
        PlayedFor10Hours,
        PlayedAWholeDay,
        YouCheated,
        BehindTheScenes,
    }

    internal sealed class StatsManager {
        internal Dictionary<Statistic, float> Statistics { get; }
        internal Dictionary<Achievement, float> AchievementLimits { get; }
        internal Dictionary<Achievement, (float, float)> Achievements { get; }
        internal List<Achievement> PendingAchievements { get; }
        private Dictionary<Statistic, Achievement[]> MapStatisticsToAchievements { get; }

        internal StatsManager() {
            Statistics = new Dictionary<Statistic, float>();
            Achievements = new Dictionary<Achievement, (float, float)>();
            PendingAchievements = new List<Achievement>();

            AchievementLimits = new Dictionary<Achievement, float> {
                {Achievement.Killed10Enemies, 10},
                {Achievement.Killed100Enemies, 100},
                {Achievement.Killed1000Enemies, 1000},
                {Achievement.Killed10000Enemies, 10000},
                {Achievement.Lost1Game, 1},
                {Achievement.Lost10Games, 10},
                {Achievement.Lost100Games, 100},
                {Achievement.Won1Game, 1},
                {Achievement.Won10Games, 10},
                {Achievement.Won100Games, 100},
                {Achievement.YouCheated, 1},
                {Achievement.PlayedFor1Hour, 3600},
                {Achievement.PlayedFor10Hours, 36000},
                {Achievement.PlayedAWholeDay, 86400},
                {Achievement.BehindTheScenes, 1},
            };

            MapStatisticsToAchievements = new Dictionary<Statistic, Achievement[]> {
                {Statistic.EnemiesKilled, new [] {
                    Achievement.Killed10Enemies,
                    Achievement.Killed100Enemies,
                    Achievement.Killed1000Enemies,
                    Achievement.Killed10000Enemies}
                },
                {Statistic.GamesLost, new [] {
                    Achievement.Lost1Game,
                    Achievement.Lost10Games,
                    Achievement.Lost100Games}
                },
                {Statistic.GamesWon, new [] {
                    Achievement.Won1Game,
                    Achievement.Won10Games,
                    Achievement.Won100Games}
                },
                {Statistic.TimePlayed, new [] {
                    Achievement.PlayedFor1Hour,
                    Achievement.PlayedFor10Hours,
                    Achievement.PlayedAWholeDay}
                },
            };
            LoadStatistics();
        }

        internal void AddValue(Statistic statistic, float value) {
            if (WorldGameState.IsTechDemo) {
                return;
            }

            Statistics[statistic] += value;
            if (MapStatisticsToAchievements.ContainsKey(statistic)) {
                foreach (var achievement in MapStatisticsToAchievements[statistic]) {
                    IncreaseAchievement(achievement, value);
                }
            }
        }

        internal void IncreaseAchievement(Achievement achievement, float value) {
            var (absolute, _) = Achievements[achievement];
            if (absolute >= AchievementLimits[achievement]) {
                Achievements[achievement] = (AchievementLimits[achievement], 1);
                return;
            }

            absolute += value;
            var percent = absolute / AchievementLimits[achievement];

            if (percent >= 1f) {
                PendingAchievements.Add(achievement);
            }

            Achievements[achievement] = (absolute, percent);
        }

        private void LoadStatistics() {
            foreach (var statEnum in (Statistic[]) Enum.GetValues(typeof(Statistic))) {
                Statistics.Add(statEnum, (float) SaveLoadManager.LoadSettingAsDouble(statEnum + "", 0, DictionarySavingFiles.Statistics));
            }

            foreach (var achievementEnum in (Achievement[]) Enum.GetValues(typeof(Achievement))) {
                var absoluteValue = (float) Math.Clamp(SaveLoadManager.LoadSettingAsDouble(achievementEnum + "", 0, DictionarySavingFiles.Statistics), 0, AchievementLimits[achievementEnum]);
                var percentValue = absoluteValue / AchievementLimits[achievementEnum];
                Achievements.Add(achievementEnum, (absoluteValue, percentValue));
            }
        }

        public void SaveStatistics() {
            foreach (var statEnum in (Statistic[]) Enum.GetValues(typeof(Statistic))) {
                SaveLoadManager.SaveSetting(statEnum + "", Statistics[statEnum], DictionarySavingFiles.Statistics);
            }

            foreach (var achievementEnum in (Achievement[]) Enum.GetValues(typeof(Achievement))) {
                SaveLoadManager.SaveSetting(achievementEnum + "", Achievements[achievementEnum].Item1, DictionarySavingFiles.Statistics);
            }
        }
    }
}