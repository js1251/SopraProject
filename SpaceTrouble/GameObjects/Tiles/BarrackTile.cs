using Microsoft.Xna.Framework;
using SpaceTrouble.GameObjects.Creatures;
using SpaceTrouble.GameObjects.Creatures.friendly;
using SpaceTrouble.Menu.Statistics;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.World;

// Created by Jakob Sailer

namespace SpaceTrouble.GameObjects.Tiles {
    internal sealed class BarrackTile : CreatureSpawnerTile {

        public MinionAiType DefaultAi { get; set; }
        public BarrackTile() {
            Pivot = new Vector2(0.5f, 0.625f);
            RequiredResources = new ResourceVector(3, 3, 4);
            RequiredResourcesForSpawn = new ResourceVector(0, 0, 1);
            SpawnType = GameObjectEnum.Minion;
            MaxSpawnNumber = WorldGameState.DifficultyManager.GetAttribute(DifficultyObject.Miscellaneous, DifficultyAttribute.MaxMinionPerBarrack);
            DefaultAi = WorldGameState.TaskManager.DefaultAi;
        }
        
        protected override Creature SpawnCreature(GameObjectEnum type) {
            SpaceTrouble.StatsManager.AddValue(Statistic.MinionsSpawned, 1);
            var spawnedCreature = (Minion) base.SpawnCreature(type);
            spawnedCreature.AiImplementation = WorldGameState.TaskManager.CreateNewAi(DefaultAi, spawnedCreature);
            WorldGameState.TaskManager.MinionHasBeenCreated(spawnedCreature);
            return spawnedCreature;
        }
    }
}