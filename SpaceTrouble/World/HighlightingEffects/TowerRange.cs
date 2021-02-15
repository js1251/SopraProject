using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.World.HighlightingEffects {
    internal enum TowerRangeMode {
        Hover,
        None
    }

    internal sealed class TowerRange {
        internal Dictionary<TowerTile, float> TowersToShowRange { get; }
        private Texture2D TowerRangeTexture { get; set; }
        private Vector2 TowerRangeTextureVirtualSize { get; set; }
        private float TowerRangeFadeTime { get; }
        private TowerRangeMode mMode;
        internal TowerRangeMode Mode {
            get => mMode;
            set {
                mMode = value;
                var allModeCount = Enum.GetValues(typeof(TowerRangeMode)).Length;
                if ((int) mMode >= allModeCount) {
                    mMode = 0;
                } else if ((int) mMode < 0) {
                    mMode = (TowerRangeMode) allModeCount - 1;
                }
            }
        }

        public TowerRange() {
            TowerRangeFadeTime = 1f;
            TowersToShowRange = new Dictionary<TowerTile, float>();
        }

        internal void Reset() {
            TowersToShowRange.Clear();
            Mode = TowerRangeMode.Hover;
        }

        internal void LoadContent() {
            TowerRangeTexture = Assets.Textures.UtilTextures.TowerRange;

            // the image was cropped on all borders to cut away half a tile (since the circle doesn't touch them)
            var textureCropOffset = Vector2.One * 2f;

            // there were four tiles within the texture when making it
            var textureSizePerTile = new Vector2 {
                X = TowerRangeTexture.Width,
                Y = TowerRangeTexture.Height
            } / 4f;

            // this is the actual size of the image if you add the cropped parts
            TowerRangeTextureVirtualSize = new Vector2 {
                X = TowerRangeTexture.Width + textureCropOffset.X * textureSizePerTile.X,
                Y = TowerRangeTexture.Height + textureCropOffset.Y * textureSizePerTile.Y
            };
        }

        internal void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            var alphaChangeAmount = (float)gameTime.ElapsedGameTime.TotalSeconds / TowerRangeFadeTime;

            if (Mode == TowerRangeMode.None) {
                DecreaseFade(alphaChangeAmount);
                return;
            }

            if (Mode == TowerRangeMode.Hover) {
                if (inputs.TryGetValue(ActionType.MouseMoved, out var input)) {
                    var hoveredTile = GetHoveredTower(input.Origin);
                    if (hoveredTile != null) {
                        IncreaseFade(alphaChangeAmount, hoveredTile);
                    }
                    DecreaseFade(alphaChangeAmount, hoveredTile);
                }
            }
        }

        private Tile GetHoveredTower(Vector2 cursorPos) {
            var tilePos = CoordinateManager.ScreenToTile(cursorPos);
            var tileAtPos = WorldGameState.ObjectManager.GetTile(tilePos);

            if (tileAtPos is TowerTile tower && tower.BuildingFinished) {
                if (!TowersToShowRange.ContainsKey(tower)) {
                    TowersToShowRange.Add(tower, 0f);
                }

                return tileAtPos;
            }

            return null;
        }


        private void IncreaseFade(float alphaChangeAmount, Tile hoveredTile = null) {
            foreach (var tower in TowersToShowRange.Keys.ToList()) {
                if (hoveredTile != null && tower != hoveredTile) {
                    continue;
                }

                TowersToShowRange[tower] = Math.Clamp(TowersToShowRange[tower] + alphaChangeAmount, 0, 0.7f);
            }
        }

        private void DecreaseFade(float alphaChangeAmount, Tile hoveredTile = null) {
            foreach (var tower in TowersToShowRange.Keys.ToList()) {
                if (tower.Equals(hoveredTile)) {
                    continue;
                }
                var alpha = TowersToShowRange[tower];
                alpha -= alphaChangeAmount;
                if (alpha <= 0) {
                    TowersToShowRange.Remove(tower);
                    continue;
                }

                TowersToShowRange[tower] = alpha;
            }
        }

        internal void Draw(SpriteBatch spriteBatch) {
            foreach (var (tower, alpha) in TowersToShowRange) {
                var scale = tower.AttackRadius * 2f / TowerRangeTextureVirtualSize.X;
                var position = tower.WorldPosition - new Vector2(TowerRangeTexture.Width, TowerRangeTexture.Height) / 2f * scale;
                spriteBatch.Draw(TowerRangeTexture, position, null, Color.Turquoise * alpha, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
        }
    }
}
