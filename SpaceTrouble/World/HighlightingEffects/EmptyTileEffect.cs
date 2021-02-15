using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.Tools;

// Created by Jakob Sailer

namespace SpaceTrouble.World.HighlightingEffects {
    internal sealed class EmptyTileEffect {
        private int CursorRadius { get; }
        private float EmptyTileFadeLimit { get; }
        private double EmptyTileFadeTime { get; } // time it takes till fade is complete in seconds
        internal bool AlphaOverride { get; set; }

        public EmptyTileEffect() {
            CursorRadius = 8;
            EmptyTileFadeLimit = 0.25f;
            EmptyTileFadeTime = 0.75;

            foreach (var gameObject in WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.EmptyTile)) {
                if (gameObject is EmptyTile emptyTile) {
                    emptyTile.Color = Color.OrangeRed;
                }
            }
        }

        internal void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            if (inputs.TryGetValue(ActionType.MouseMoved, out var input)) {
                HighlightEmptyTiles(gameTime, input.Origin);
            }
        }

        internal void HighlightPortals() {
            foreach (var portal in WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.PortalTile)) {
                foreach (var (emptyTile, alpha) in GetEmptyTilesInRadius(CoordinateManager.WorldToTile(portal.WorldPosition))) {
                    emptyTile.Color = new Color(255, 300 * (1 + alpha), 300 * (1 + alpha)); //TODO: fix, but not really required
                }
            }
        }

        private void HighlightEmptyTiles(GameTime gameTime, Vector2 cursorPos) {
            if (!AlphaOverride) {
                foreach (var (emptyTile, alpha) in GetEmptyTilesInRadius(CoordinateManager.ScreenToTile(cursorPos))) {
                    SetFadeOut(emptyTile, alpha);
                }
            }

            ApplyFading(gameTime);
        }

        private List<(EmptyTile, float)> GetEmptyTilesInRadius(Vector2 tilePos) {
            var nearTiles = new List<(EmptyTile, float)>();
            for (var x = -CursorRadius; x < CursorRadius; x++) {
                for (var y = -CursorRadius; y < CursorRadius; y++) {
                    var addedVector = new Vector2(x, y);
                    var newTilePos = tilePos + addedVector;
                    var tileAtPos = WorldGameState.ObjectManager.GetTile(newTilePos);

                    if (tileAtPos == null) {
                        continue;
                    }

                    if (tileAtPos is EmptyTile emptyTile) {
                        var alpha = 1 - Vector2.Distance(Vector2.Zero, addedVector) / CursorRadius * 1.4f;
                        nearTiles.Add((emptyTile, alpha));
                    }
                }
            }

            return nearTiles;
        }

        private void SetFadeOut(EmptyTile emptyTile, float alpha) {
            if (alpha > EmptyTileFadeLimit) {
                emptyTile.FadeAmount += alpha;
            }
        }

        private void ApplyFading(GameTime gameTime) {
            foreach (var gameObject in WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.EmptyTile)) {
                if (gameObject is EmptyTile emptyTile) {
                    emptyTile.FadeAmount = (float)Math.Clamp(emptyTile.FadeAmount - gameTime.ElapsedGameTime.TotalSeconds / EmptyTileFadeTime, AlphaOverride ? 0 : EmptyTileFadeLimit, 1f);
                }
            }
        }
    }
}
