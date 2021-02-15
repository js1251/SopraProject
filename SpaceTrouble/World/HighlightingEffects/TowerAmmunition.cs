using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer and Kai König

namespace SpaceTrouble.World.HighlightingEffects {
    internal enum TowerAmmunitionMode {
        Hover,
        All,
        Critical,
        None
    }

    internal sealed class TowerAmmunition {
        private Dictionary<TowerTile, (MenuBar, MenuBar)> TowersToDrawAmmunition { get; }
        private float CriticalAmmunitionThreshold { get; }
        private TowerRange TowerRange { get; }
        private TowerAmmunitionMode mMode;
        internal TowerAmmunitionMode Mode {
            get => mMode;
            set {
                mMode = value;
                var allModeCount = Enum.GetValues(typeof(TowerAmmunitionMode)).Length;
                if ((int)mMode >= allModeCount) {
                    mMode = 0;
                } else if ((int)mMode < 0) {
                    mMode = (TowerAmmunitionMode)allModeCount - 1;
                }
            }
        }

        public TowerAmmunition(TowerRange towerRange) {
            TowerRange = towerRange;
            CriticalAmmunitionThreshold = 0.4f;
            TowersToDrawAmmunition = new Dictionary<TowerTile, (MenuBar, MenuBar)>();
        }

        internal void Update(Dictionary<ActionType, InputAction> inputs) {
            foreach (var t in WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.TowerTile)) {
                if (!(t is TowerTile tower) || !tower.BuildingFinished) {
                    continue;
                }

                if (Mode == TowerAmmunitionMode.None || (Mode == TowerAmmunitionMode.Hover && !TowerRange.TowersToShowRange.ContainsKey(tower))) {
                    continue;
                }

                if (!TowersToDrawAmmunition.ContainsKey(tower)) {
                    var newBeltBar = new MenuBar(Assets.Textures.UtilTextures.BarBulletsEmpty)
                        {FillAmount = 1};
                    var newAmmoBar = new MenuBar(Assets.Textures.UtilTextures.BarBulletsFull);
                    TowersToDrawAmmunition.Add(tower, (newBeltBar, newAmmoBar));
                }

                var leftAmmunition = (tower.NumberOfMagazines - tower.RequiredResources.Energy) * tower.ShotsPerMagazine - tower.ShotsFiredOfMagazine;
                var totalAmmunition = tower.NumberOfMagazines * tower.ShotsPerMagazine;

                var worldPosition = tower.WorldPosition.ToPoint();
                var barRectangle = new Rectangle(worldPosition.X, worldPosition.Y - 30, 50, 5);
                barRectangle.X -= barRectangle.Width / 2;
                var beltBar = TowersToDrawAmmunition[tower].Item1;
                var ammoBar = TowersToDrawAmmunition[tower].Item2;

                beltBar.mBounds = barRectangle;
                ammoBar.mBounds = barRectangle;

                ammoBar.FillAmount = leftAmmunition / totalAmmunition;

                ammoBar.BarColor = ammoBar.FillAmount < CriticalAmmunitionThreshold ? Color.OrangeRed : default;

                ammoBar.Update(inputs);
                beltBar.Update(inputs);
            }
        }

        internal void Draw(SpriteBatch spriteBatch) {
            if (Mode == TowerAmmunitionMode.None) {
                return;
            }

            foreach (var (tower, (belt, ammo)) in TowersToDrawAmmunition) {
                if (Mode == TowerAmmunitionMode.All) {
                    belt.Draw(spriteBatch, 1);
                    ammo.Draw(spriteBatch, 1);
                } else if (Mode == TowerAmmunitionMode.Critical) {
                    if (ammo.FillAmount <= CriticalAmmunitionThreshold) {
                        belt.Draw(spriteBatch, 1);
                        ammo.Draw(spriteBatch, 1);
                    }
                } else if (Mode == TowerAmmunitionMode.Hover) {
                    if (TowerRange.TowersToShowRange.ContainsKey(tower)) {
                        belt.Draw(spriteBatch, TowerRange.TowersToShowRange[tower]);
                        ammo.Draw(spriteBatch, TowerRange.TowersToShowRange[tower]);
                    }
                }
            }
        }
    }
}
