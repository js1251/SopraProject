using Microsoft.Xna.Framework;
using SpaceTrouble.GameObjects;
using SpaceTrouble.World;

// Created by Jakob Sailer

namespace SpaceTrouble.util.DataStructures.GameObjectStructure {
    internal sealed class GameDataStructure {
        public ObjectDataStructure ObjectData { get; } // all information about GameObjects
        public CollisionDataStructure CollisionData { get; } // all information about collision
        public DrawDataStructure DrawData { get; } // all information about drawing and draw-sorting

        public GameDataStructure() {
            ObjectData = new ObjectDataStructure(this);
            CollisionData = new CollisionDataStructure(this);
            DrawData = new DrawDataStructure(this);
        }

        public void Update(GameTime gameTime) {
            if (!WorldGameState.IsPaused && !WorldGameState.IsGameFinished) {
                ObjectData.Update(gameTime);
                CollisionData.Update();
            }

            DrawData.Update();
            WorldGameState.NavigationManager.Update();
        }

        /// <summary>
        /// Inserts a given GameObject into the data-structure.
        /// </summary>
        /// <param name="gameObject">The GameObject to insert.</param>
        public void Insert(GameObject gameObject) {
            ObjectData.Insert(gameObject);
        }

        /// <summary>
        /// Removes a given GameObject into the data-structure.
        /// </summary>
        /// <param name="gameObject">The GameObject to remove.</param>
        public void Remove(GameObject gameObject) {
            ObjectData.Remove(gameObject);
        }
    }
}