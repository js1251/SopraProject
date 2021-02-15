using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures.enemy
{
    internal sealed class WalkingEnemyAi : CreatureAi
    {
        [JsonProperty] private WalkingEnemy WalkingEnemy { get; set; } // The Minion this AI is assigned to

        public WalkingEnemyAi(WalkingEnemy walkingEnemy)
        {
            WalkingEnemy = walkingEnemy;
        }

        public override void InitializeAi()
        {
        }

        public override void UpdateAi(GameTime gameTime)
        {
            if (IsIdle())
            {
                WalkingEnemy.TargetDestinations = GetNewTargets();
            }
        }

        public override void OnReachedTargetDestination()
        {
        }

        public override void OnCreatureDies()
        {
        }

        private Stack<Vector2> GetNewTargets()
        {
            var stack = new Stack<Vector2>();

            var allMinion = WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.Minion);
            stack.Push(allMinion[new Random().Next(0, allMinion.Count)].WorldPosition);

            return stack;
        }

        public override bool IsIdle()
        {
            return WalkingEnemy.TargetDestinations.Count <= 0;
        }
    }
}
