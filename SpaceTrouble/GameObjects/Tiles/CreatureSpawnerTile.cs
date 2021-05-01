using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Creatures;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Tiles {
    internal abstract class CreatureSpawnerTile : Tile, IBuildable {
        // IBuildable
        [JsonProperty] public ResourceVector OnTheWayResources { get; set; }
        [JsonProperty] public ResourceVector RequiredResources { get; set; }
        [JsonProperty] public bool BuildingFinished { get; set; }

        // spawning
        [JsonProperty] public GameObjectEnum SpawnType { get; set; }
        [JsonProperty] public virtual int MaxSpawnNumber { get; set; }
        [JsonProperty] public ResourceVector RequiredResourcesForSpawn { get; set; }
        [JsonProperty] public HashSet<Creature> CreaturesSpawned { get; set; } = new HashSet<Creature>();

        public /*ResourceVector*/ void AddResource(ResourceVector resource) {

            // note: for portal and laboratory Tiles this is always true.
            // spawning is controlled by setting the MaxSpawnNumber.
            if (BuildingFinished && RequiredResourcesForSpawn.AllLessOrEqualThan(resource)) {
                if (MaxSpawnNumber > 0) {
                    SpawnCreature(SpawnType);
                }
            }

            /*return*/ ((IBuildable) this).AddResourceHelper(resource);
        }

        // "overrides" IBuildable OnBuildingFinished()
        public void OnBuildingFinished() {
            BuildingFinished = true;
            HasChanged = true;
            Color = Color.White;
            RequiredResources = RequiredResourcesForSpawn * (MaxSpawnNumber - CreaturesSpawned.Count);
            if (this is ICanHavePriority prioritizable) {
                prioritizable.HasPriority = false;
            }
        }

        protected virtual Creature SpawnCreature(GameObjectEnum type) {
            var newGameObject = WorldGameState.ObjectManager.CreateObject(WorldPosition, type);

            if (newGameObject is Creature newCreature) {
                CreaturesSpawned.Add(newCreature);
                newCreature.SpawnOrigin = this;

                return newCreature;
            }

            // in case an object has been created that wasn't a creature
            System.Diagnostics.Debug.WriteLine("Cannot spawn non-creatures from CreatureSpawners! (" + newGameObject + ")");
            WorldGameState.ObjectManager.Remove(newGameObject);

            return default;
        }

        public void CreatureHasDied(Creature creature) {
            CreaturesSpawned.Remove(creature);

            // just to be sure (there should only ever be minions coming from finished buildings!)
            if (BuildingFinished) {
                RequiredResources += RequiredResourcesForSpawn;
            }
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
            ((IBuildable) this).DrawResources(spriteBatch);
        }
    }
}