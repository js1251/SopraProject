using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;

namespace SpaceTrouble.World.HighlightingEffects {
    internal sealed class Highlighting {
        internal EmptyTileEffect EmptyTileEffect { get; }
        internal TowerRange TowerRange { get; }
        internal TowerAmmunition TowerAmmunition { get; private set; }

        public Highlighting() {
            EmptyTileEffect = new EmptyTileEffect();
            TowerRange = new TowerRange();
            TowerAmmunition = new TowerAmmunition(TowerRange);
        }

        internal void HighlightPortals() {
            EmptyTileEffect.HighlightPortals();
        }


        public void LoadContent() {
            TowerRange.LoadContent();
        }

        internal void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            EmptyTileEffect.Update(gameTime, inputs);
            TowerRange.Update(gameTime, inputs);
            TowerAmmunition.Update(inputs);
        }

        internal void Draw(SpriteBatch spriteBatch) {
            TowerRange.Draw(spriteBatch);
            TowerAmmunition.Draw(spriteBatch);
        }

        internal void Reset() {
            TowerRange.Reset();
            TowerAmmunition = new TowerAmmunition(TowerRange);
        }
    }
}