using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpaceTrouble.World {
    internal enum DifficultyEnum {
        Easy,
        Normal,
        Hard,
        Legendary
    }

    internal enum DifficultyAttribute {
        HitPoints,
        WaveWaitlength,
        FirstWaveWaitLength,
        WaveIncreasePerWave,
        WalkingEnemyAmountMultiplier,
        BaseWaveSize,

        FinalWaveWaitLength,
        FinalWaveMaxContinuesSize,
        FinalWaveLength,

        MaxMinionPerBarrack,
        StartingResources
    }

    internal enum DifficultyObject {
        GameMaster,
        FlyingEnemy,
        WalkingEnemy,
        Minion,
        Miscellaneous
    }

    internal sealed class DifficultyManager {

        [JsonProperty] internal DifficultyEnum Difficulty { get; set; }
        [JsonProperty] internal int TowerCount { get; set; }
        [JsonIgnore] private Dictionary<DifficultyObject, Dictionary<DifficultyAttribute, Dictionary<DifficultyEnum, dynamic>>> DifficultyValues { get; }

        internal DifficultyManager() {
            DifficultyValues = new Dictionary<DifficultyObject, Dictionary<DifficultyAttribute, Dictionary<DifficultyEnum, dynamic>>>();


            // Minion
            DifficultyValues[DifficultyObject.Minion] =
                new Dictionary<DifficultyAttribute, Dictionary<DifficultyEnum, dynamic>> {
                    // Hitpoints
                    [DifficultyAttribute.HitPoints] = new Dictionary<DifficultyEnum, dynamic>
                    {
                        [DifficultyEnum.Easy] = 100f,
                        [DifficultyEnum.Normal] = 60f,
                        [DifficultyEnum.Hard] = 40f,
                        [DifficultyEnum.Legendary] = 20f
                    }
                };


            // FlyingEnemy
            DifficultyValues[DifficultyObject.FlyingEnemy] =
                new Dictionary<DifficultyAttribute, Dictionary<DifficultyEnum, dynamic>> {
                    // Hitpoints
                    [DifficultyAttribute.HitPoints] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 40f,
                        [DifficultyEnum.Normal] = 50f,
                        [DifficultyEnum.Hard] = 60f,
                        [DifficultyEnum.Legendary] = 80f
                    }
                };

            // WalkingEnemy
            DifficultyValues[DifficultyObject.WalkingEnemy] =
                new Dictionary<DifficultyAttribute, Dictionary<DifficultyEnum, dynamic>> {
                    // Hitpoints
                    [DifficultyAttribute.HitPoints] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 80f,
                        [DifficultyEnum.Normal] = 100f,
                        [DifficultyEnum.Hard] = 120f,
                        [DifficultyEnum.Legendary] = 160f
                    }
                };

            // GameMaster
            DifficultyValues[DifficultyObject.GameMaster] =
                new Dictionary<DifficultyAttribute, Dictionary<DifficultyEnum, dynamic>> {
                    // Time between Waves
                    [DifficultyAttribute.WaveWaitlength] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 240f,
                        [DifficultyEnum.Normal] = 180f,
                        [DifficultyEnum.Hard] = 150f,
                        [DifficultyEnum.Legendary] = 120f
                    },
                    // WaitTime for first Wave
                    [DifficultyAttribute.FirstWaveWaitLength] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 600f,
                        [DifficultyEnum.Normal] = 300f,
                        [DifficultyEnum.Hard] = 240f,
                        [DifficultyEnum.Legendary] = 180f
                    },
                    // Wave size multiplier
                    [DifficultyAttribute.WaveIncreasePerWave] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 3f,
                        [DifficultyEnum.Normal] = 5f,
                        [DifficultyEnum.Hard] = 7f,
                        [DifficultyEnum.Legendary] = 10f
                    },
                    // WaitTime for Final Wave
                    [DifficultyAttribute.FinalWaveWaitLength] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 600f,
                        [DifficultyEnum.Normal] = 300f,
                        [DifficultyEnum.Hard] = 180f,
                        [DifficultyEnum.Legendary] = 120f
                    },
                    // Final Wave duration
                    [DifficultyAttribute.FinalWaveLength] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 200f,
                        [DifficultyEnum.Normal] = 300f,
                        [DifficultyEnum.Hard] = 420f,
                        [DifficultyEnum.Legendary] = 600f
                    },
                    // WaitTime for Final Wave
                    [DifficultyAttribute.FinalWaveMaxContinuesSize] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 150,
                        [DifficultyEnum.Normal] = 200,
                        [DifficultyEnum.Hard] = 250,
                        [DifficultyEnum.Legendary] = 300
                    },
                    // Multiplier for amount of Walking Enemys when Portal is Closed
                    [DifficultyAttribute.WalkingEnemyAmountMultiplier] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 1.5f,
                        [DifficultyEnum.Normal] = 2f,
                        [DifficultyEnum.Hard] = 2.5f,
                        [DifficultyEnum.Legendary] = 3f
                    },
                    // BaseWaveSize
                    [DifficultyAttribute.BaseWaveSize] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 10,
                        [DifficultyEnum.Normal] = 12,
                        [DifficultyEnum.Hard] = 15,
                        [DifficultyEnum.Legendary] = 20
                    },
                };

            // Miscellaneous
            DifficultyValues[DifficultyObject.Miscellaneous] =
                new Dictionary<DifficultyAttribute, Dictionary<DifficultyEnum, dynamic>> {
                    // Starting Resources
                    // TODO: StockPile wird im Objectmanager als Object zum Clonen angelegt bevor der DifficultyManager die Schwierigkeit weiß
                    [DifficultyAttribute.StartingResources] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 30,
                        [DifficultyEnum.Normal] = 30,
                        [DifficultyEnum.Hard] = 30,
                        [DifficultyEnum.Legendary] = 30
                    },
                    // Maximal amount of Minions per Barack
                    [DifficultyAttribute.MaxMinionPerBarrack] = new Dictionary<DifficultyEnum, dynamic> {
                        [DifficultyEnum.Easy] = 5,
                        [DifficultyEnum.Normal] = 4,
                        [DifficultyEnum.Hard] = 4,
                        [DifficultyEnum.Legendary] = 3
                    }
                };
        }

        public dynamic GetAttribute(DifficultyObject difficultyObject, DifficultyAttribute difficultyAttribute) {
            return DifficultyValues[difficultyObject][difficultyAttribute][Difficulty];
        }
    }
}
