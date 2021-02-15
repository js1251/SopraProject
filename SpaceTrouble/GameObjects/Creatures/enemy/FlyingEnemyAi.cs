﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures.enemy {
    internal sealed class FlyingEnemyAi : CreatureAi {
        [JsonProperty] private FlyingEnemy FlyingEnemy { get; set; } // The Minion this AI is assigned to

        public FlyingEnemyAi(FlyingEnemy flyingEnemy) {
            FlyingEnemy = flyingEnemy;
        }
        public override void InitializeAi() {
        }

        public override void UpdateAi(GameTime gameTime) {
            if (IsIdle()) {
                FlyingEnemy.TargetDestinations = GetNewTargets();
            }
        }

        public override void OnReachedTargetDestination() {
        }

        public override void OnCreatureDies() {
        }

        private static Stack<Vector2> GetNewTargets() {
            var stack = new Stack<Vector2>();

            var allMinion = WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.Minion);
            stack.Push(allMinion[new Random().Next(0, allMinion.Count)].WorldPosition);

            return stack;
        }

        public override bool IsIdle() {
            return FlyingEnemy.TargetDestinations.Count <= 0;
        }
    }
}
