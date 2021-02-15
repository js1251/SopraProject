using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.Statistics;

// Created by Jakob Sailer

namespace SpaceTrouble.World {
    internal sealed class GameMaster {
        [JsonIgnore] private ObjectManager ObjectManager { get; }
        [JsonProperty] private int WaveSize { get; set; }
        [JsonProperty] public int WaveNumber { get; private set; }
        [JsonProperty] public double TimeSinceLastWave { get; private set; }
        [JsonProperty] public double WaitTime { get; private set; }
        [JsonIgnore] public bool GameLost { get; private set; }
        [JsonIgnore] public bool GameWon { get; private set; }
        [JsonIgnore] public bool GameFinished { get; private set; }
        [JsonIgnore] private int ContinuesSpawnAmount { get; set; }
        [JsonProperty] public bool IsFinalWave { get; set; }
        [JsonProperty] public float FinalWaveCountdown { get; private set; }
        [JsonIgnore] private Tuple<Sound, SoundEffectInstance> ActivePortalSound { get; set; }

        public GameMaster() {
            ObjectManager = WorldGameState.ObjectManager;
            WaveNumber = 1; // which wave the game is currently on
            WaveSize = WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.GameMaster, DifficultyAttribute.BaseWaveSize);
            GameWon = false;
            GameLost = false;
            GameFinished = false;
            ContinuesSpawnAmount = 1000;
            FinalWaveCountdown = WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.GameMaster, DifficultyAttribute.FinalWaveWaitLength);
            WaitTime = WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.GameMaster, DifficultyAttribute.FirstWaveWaitLength);
        }

        public void Update(GameTime gameTime) {
            if (WorldGameState.IsPaused || GameFinished) {
                return;
            }

            var allPortals = ObjectManager.GetAllObjects(GameObjectEnum.PortalTile);

            CheckForGameFinished(allPortals);
            TryPlaySpawnSounds(allPortals.Any(gameObject => gameObject is PortalTile portal && !portal.WaveIsDefeated()));

            if (!WorldGameState.IsTechDemo) {
                CheckForNewWave(gameTime, allPortals);
            } else {
                SpawnContinuously(allPortals);
                SpaceTrouble.SoundManager.PlayMusic(Music.FinalWave);
            }
        }

        private void CheckForGameFinished(List<GameObject> allPortals) {
            if (ObjectManager.GetAllObjects(GameObjectEnum.Minion).Count <= 0) {
                GameLost = true;
                GameFinished = true;
                SpaceTrouble.SoundManager.PlaySound(Sound.LostGame);
                SpaceTrouble.StatsManager.AddValue(Statistic.GamesLost, 1);
            }

            if (allPortals.All(gameObject => gameObject is LaboratoryTile laboratory && laboratory.IsClosed)) {
                if (WorldGameState.ObjectManager.GetAllObjects(ObjectProperty.Enemy).Count == 0) {
                    GameWon = true;
                    GameFinished = true;
                    SpaceTrouble.SoundManager.PlaySound(Sound.WonGame);
                    SpaceTrouble.SoundManager.PlaySound(Sound.PortalDestroyed);
                    SpaceTrouble.StatsManager.AddValue(Statistic.GamesWon, 1);
                }
            }
        }

        private void CheckForNewWave(GameTime gameTime, List<GameObject> allPortals) {
            // if all laboratories have been built start the final wave
            if (!IsFinalWave && FinalWaveCountdown > 0 && allPortals.All(portal => portal is LaboratoryTile laboratory && laboratory.BuildingFinished)) {
                IsFinalWave = true;
                ContinuesSpawnAmount = WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.GameMaster,
                    DifficultyAttribute.FinalWaveMaxContinuesSize);
                SpaceTrouble.SoundManager.PlayMusic(Music.FinalWave);
            }

            if (IsFinalWave) {
                SpawnContinuously(allPortals, true);
                FinalWaveCountdown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (FinalWaveCountdown > 0) {
                    return;
                }

                IsFinalWave = false;
                foreach (var gameObject in allPortals) {
                    if (gameObject is LaboratoryTile laboratory) {
                        laboratory.IsClosed = true;
                        laboratory.MaxSpawnNumber = 0;
                    }
                }
                return;
            }

            // only initiate a new wave if the last one has been defeated
            var waveNotDefeated = allPortals.Any(gameObject => gameObject is PortalTile portal && !portal.WaveIsDefeated());
            
            if (waveNotDefeated) {
                return;
            }

            TimeSinceLastWave += gameTime.ElapsedGameTime.TotalSeconds;

            // if the wait-time between waves has passed start a new one
            if (TimeSinceLastWave >= WaitTime) {
                // play the action music
                SpaceTrouble.SoundManager.PlayMusic(Music.Attacking);
                SpaceTrouble.StatsManager.AddValue(Statistic.WavesDefeated, 1);

                SpawnWave(allPortals);
                WaveNumber++;
                WaveSize = (int)(WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.GameMaster, DifficultyAttribute.WaveIncreasePerWave) * WaveNumber + WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.GameMaster, DifficultyAttribute.BaseWaveSize));
                TimeSinceLastWave -= WaitTime;
                WaitTime = WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.GameMaster, DifficultyAttribute.WaveWaitlength);
            } else {
                SpaceTrouble.SoundManager.PlayMusic(Music.Regular);
            }
        }

        private void TryPlaySpawnSounds(bool isActiveWave) {
            if (!isActiveWave) {
                if (ActivePortalSound != null) {
                    ActivePortalSound.Item2?.Stop(false);
                    ActivePortalSound = null;
                }
                return;
            }

            if (ActivePortalSound == null) {
                var newSound = SpaceTrouble.SoundManager.PlaySound(Sound.PortalSpawnBegin, 0.5f);
                ActivePortalSound = new Tuple<Sound, SoundEffectInstance>(Sound.PortalSpawnBegin, newSound);
            }
        }

        private void SpawnWave(List<GameObject> allPortals) {

            // spawn enemies from all portals
            foreach (var gameObject in allPortals) {
                if (!(gameObject is PortalTile portal)) {
                    continue;
                }

                portal.MaxSpawnNumber = WaveSize;

                if (portal.SpawnBoth || portal is LaboratoryTile && portal.BuildingFinished) {
                    portal.MaxSpawnNumber = (int)(portal.MaxSpawnNumber * WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.GameMaster, DifficultyAttribute.WalkingEnemyAmountMultiplier));
                }
            }
        }

        private void SpawnContinuously(List<GameObject> allPortals, bool forceSpawnBoth = false) {
            foreach (var gameObject in allPortals) {
                if (!(gameObject is PortalTile portal)) {
                    continue;
                }

                if (forceSpawnBoth) {
                    portal.SpawnBoth = true;
                }

                var spawnNumberPerPortal = ContinuesSpawnAmount / allPortals.Count;
                if (portal.CreaturesSpawned.Count <= spawnNumberPerPortal) {
                    portal.MaxSpawnNumber = spawnNumberPerPortal;
                }
            }
        }
    }
}