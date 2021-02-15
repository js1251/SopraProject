using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Tiles {
    internal class PortalTile : CreatureSpawnerTile, IAnimating {
        [JsonProperty] private float WaitTimeBetweenSpawns { get; set; }
        [JsonIgnore] private double NextSpawnTime { get; set; }
        [JsonProperty] public bool SpawnBoth { get; set; } // set to true by NavigationManager once Portal is next to a platform
        [JsonProperty] public float CurrentFrame { get; set; }
        [JsonProperty] public int CurrentLayer { get; set; }
        [JsonIgnore] public float Angle { get; set; }
        [JsonIgnore] public float AnimationSpeed { get; }

        public PortalTile() {
            Pivot = new Vector2(0.5f, 0.6f);
            SpawnType = GameObjectEnum.FlyingEnemy;
            WaitTimeBetweenSpawns = .5f; // time to wait between each spawned enemy
            RequiredResources = new ResourceVector(0, 0, 0); // Portals can never be built
            RequiredResourcesForSpawn = new ResourceVector(0, 0, 0);

            // Animation
            TotalFrames = new Point(12, 3);
            CurrentFrame = 0;
            CurrentLayer = 0;
            AnimationSpeed = 8f; // 12 fps (nice for pixel-art)
            Effect = AnimationEffect.PlayLooping;
        }

        public AnimationEffect Effect { get; set; }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);

            HandleEffect();

            ((IAnimating)this).Update(gameTime);
            // instead of controlling spawning through resources its set to always spawn
            // but the MaxSpawnNumber is set from the GameMaster and decremented to 0 once all Creatures spawned
            if (MaxSpawnNumber > 0) {
                if (NextSpawnTime < gameTime.TotalGameTime.TotalSeconds) {
                    if (SpawnBoth) {
                        SpawnType = MaxSpawnNumber % 2 == 0 ? GameObjectEnum.WalkingEnemy : GameObjectEnum.FlyingEnemy;
                    }

                    SpawnCreature(SpawnType);
                    MaxSpawnNumber--;
                    NextSpawnTime = gameTime.TotalGameTime.TotalSeconds + WaitTimeBetweenSpawns;
                }
            }
        }

        private void HandleEffect() {
            if (MaxSpawnNumber == 0 && (CurrentLayer == 1 && CurrentFrame == 0 || CurrentLayer == 2)) {
                if (CurrentLayer == 2) {
                    CurrentLayer = 1;
                    CurrentFrame = TotalFrames.X - 1;
                    Effect = AnimationEffect.PlayOnceReverse;
                    return;
                }

                if (CurrentLayer == 1 && CurrentFrame == 0) {
                    CurrentLayer = 0;
                    Effect = AnimationEffect.PlayLooping;
                }
            } else if (MaxSpawnNumber > 0 || CurrentLayer == 1) {
                if (CurrentLayer == 0) {
                    CurrentLayer = 1;
                    CurrentFrame = 0;
                    Effect = AnimationEffect.PlayOnce;
                    return;
                }

                if (CurrentLayer == 1 && CurrentFrame >= TotalFrames.X - 1) {
                    CurrentLayer = 2;
                    Effect = AnimationEffect.PlayLooping;
                }
            }
        }

        public bool WaveIsDefeated() {
            return MaxSpawnNumber <= 0 && CreaturesSpawned.Count <= 0;
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            ((IAnimating) this).Draw(spriteBatch);
        }
    }
}