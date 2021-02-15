using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.util.Tools;

// created by Jakob Sailer

namespace SpaceTrouble.World {
    internal sealed class TechdemoGenerator : WorldGenerator {
        internal void CreateTechdemo() {
            mNavigationManager.IsWorldCreation = true; // this stops the navManager from creating new nav-information every time a tile has been created

            InitializeGrid();
            GenerateMiddleSquare();

            for (var i = 0; i < 10; i++) {
                GeneratePortals();
            }

            mObjectManager.CreateTile(Global.mWorldOrigin, GameObjectEnum.StockTile, true);
            GenerateConstructions();
            GenerateTowers();

            OverfillStockTile(10);
            InitializeNavigation();

            SpawnMinions(MinionAiType.FoodMinionAi);
            SpawnMinions(MinionAiType.ConstructionMinionAi);
        }

        private void OverfillStockTile(int factor) {
            foreach (var tile in mObjectManager.GetAllObjects(GameObjectEnum.StockTile)) {
                if (tile is StockTile stockTile) {
                    stockTile.Resources *= factor;
                }
            }
        }

        private void GenerateMiddleSquare() {
            const float squareFactor = 20f;
            var radius = new Point((int) (Global.WorldWidth / squareFactor), (int) (Global.WorldHeight / squareFactor));

            for (var x = -radius.X; x <= radius.X; x++) {
                for (var y = -radius.Y; y <= radius.Y; y++) {
                    var tilePos = Global.mWorldOrigin + new Vector2(x, y);
                    mObjectManager.CreateTile(tilePos, GameObjectEnum.PlatformTile, true);
                }
            }
        }

        private void GenerateConstructions() {
            var markedPlatforms = new Dictionary<Tile, int>();
            foreach (var gameObject in mObjectManager.GetAllObjects(GameObjectEnum.PlatformTile).ToList()) {
                if (gameObject is Tile tile) {
                    WalkPlatforms(markedPlatforms, CoordinateManager.WorldToTile(tile.WorldPosition));
                }
            }

            foreach (var gameObject in mObjectManager.GetAllObjects(GameObjectEnum.PlatformTile).ToList()) {
                if (gameObject is Tile tile) {
                    GenerateBuildings(CoordinateManager.WorldToTile(tile.WorldPosition));
                }
            }
        }

        private void WalkPlatforms(Dictionary<Tile, int> markedTiles, Vector2 tilePos, int depth = 0, int maxMarkings = 20) {
            var currentTile = mObjectManager.GetTile(tilePos);
            if (currentTile == null || (markedTiles.ContainsKey(currentTile) && markedTiles[currentTile] > maxMarkings)) {
                return;
            }

            if (!markedTiles.ContainsKey(currentTile)) {
                markedTiles.Add(currentTile, 0);
            }
            markedTiles[currentTile]++;

            if (depth == (Global.WorldWidth + Global.WorldHeight) / 32) {
                return;
            }

            if (currentTile is EmptyTile) {
                mObjectManager.CreateTile(tilePos, GameObjectEnum.PlatformTile, true);
                depth++;
            }

            var direction = new Random().Next(0, 4);
            switch (direction) {
                case 0:
                    WalkPlatforms(markedTiles, tilePos - Vector2.UnitX, depth);
                    break;
                case 1:
                    WalkPlatforms(markedTiles, tilePos + Vector2.UnitX, depth);
                    break;
                case 2:
                    WalkPlatforms(markedTiles, tilePos - Vector2.UnitY, depth);
                    break;
                case 3:
                    WalkPlatforms(markedTiles, tilePos + Vector2.UnitY, depth + 1);
                    break;
            }
        }

        private void GenerateBuildings(Vector2 tilePos) {

            for (var x = -1; x <= 1; x++) {
                for (var y = -1; y <= 1; y++) {
                    var checkTile = mObjectManager.GetTile(tilePos + new Vector2(x, y));
                    if (!(checkTile is PlatformTile)) {
                        return;
                    }
                }
            }

            var random = new Random();
            if (random.Next(0, 3) == 0) {
                var type = random.Next(0, 5);
                switch (type) {
                    case 0:
                        mObjectManager.CreateTile(tilePos, GameObjectEnum.BarrackTile, true);
                        break;
                    case 1:
                        mObjectManager.CreateTile(tilePos, GameObjectEnum.TowerTile);
                        break;
                    case 2:
                        mObjectManager.CreateTile(tilePos, GameObjectEnum.KitchenTile);
                        break;
                    case 3:
                        mObjectManager.CreateTile(tilePos, GameObjectEnum.GeneratorTile);
                        break;
                    case 4:
                        mObjectManager.CreateTile(tilePos, GameObjectEnum.ExtractorTile);
                        break;
                }
            }
        }

        private void GenerateTowers() {
            foreach (var gameObject in mObjectManager.GetAllObjects(GameObjectEnum.PlatformTile)) {
                var tilePos = CoordinateManager.WorldToTile(gameObject.WorldPosition);
                TryPlaceTower(tilePos);
            }
        }

        private void TryPlaceTower(Vector2 tilePos) {
            for (var x = -1; x <= 1; x++) {
                for (var y = -1; y <= 1; y++) {
                    var checkTile = mObjectManager.GetTile(tilePos + new Vector2(x, y));
                    if (checkTile is EmptyTile) {
                        mObjectManager.CreateTile(tilePos + new Vector2(x, y), GameObjectEnum.TowerTile, new Random().Next(0, 1) == 0);
                        return;
                    }
                }
            }
        }
    }
}