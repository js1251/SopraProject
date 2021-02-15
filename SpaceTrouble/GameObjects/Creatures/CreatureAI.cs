using Microsoft.Xna.Framework;

namespace SpaceTrouble.GameObjects.Creatures
{
    internal abstract class CreatureAi
    {
        /// <summary>
        /// Things to do when the ai-implementation is added to a creature
        /// </summary>
        public abstract void InitializeAi();

        /// <summary>
        /// Update call for the ai
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void UpdateAi(GameTime gameTime);

        /// <summary>
        /// Called when the creature reaches it's target. E.g. Creatures should then check for new targets. 
        /// </summary>
        public abstract void OnReachedTargetDestination();

        /// <summary>
        /// 
        /// </summary>
        public abstract void OnCreatureDies();

        /// <summary>
        /// Makes a decision whether or not a creature is currently idle
        /// </summary>
        /// <returns>True if creature is idle, False otherwise.</returns>
        public abstract bool IsIdle();
    }
}