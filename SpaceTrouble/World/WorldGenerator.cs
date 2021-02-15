using System;
using Microsoft.Xna.Framework;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.util.Tools;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SpaceTrouble.World {
    internal class WorldGenerator {
        private Point mWorldSize;
        private readonly Vector2 mWorldCenter;
        private readonly byte[,] mSpaceIsUsed;
        private readonly int mMassMinDistance;
        private readonly Random mRandom;

        protected readonly ObjectManager mObjectManager;
        protected readonly NavigationManager mNavigationManager;

        internal WorldGenerator() {
            mObjectManager = WorldGameState.ObjectManager;
            mNavigationManager = WorldGameState.NavigationManager;
            mNavigationManager.IsWorldCreation = true;

            mRandom = new Random();
            mWorldSize = new Point(Global.WorldWidth, Global.WorldHeight);
            mWorldCenter = mWorldSize.ToVector2() / 2;
            mMassMinDistance = 3;
            var spaceAroundStartingArea = 5;
            mSpaceIsUsed = new byte[mWorldSize.X, mWorldSize.Y];
            MarkSpaceAsUsed(mWorldCenter, spaceAroundStartingArea, byte.MaxValue);
        }

        internal void InitializeGrid() {
            for (var yPos = 0; yPos < Global.WorldHeight; yPos++) {
                for (var xPos = 0; xPos < Global.WorldWidth; xPos++) {
                    var tilePos = new Vector2(xPos, yPos);
                    // note: InsertTile will override possible existing tiles
                    mObjectManager.CreateTile(tilePos, GameObjectEnum.EmptyTile);
                }
            }
        }

        internal void CreateNewWorld() {
            mNavigationManager.IsWorldCreation = true; // this stops the navManager from creating new nav-information every time a tile has been created

            InitializeGrid();
            GenerateStartingArea();
            GenerateMassSources();
            GeneratePortals();

            SpawnMinions();

            InitializeNavigation();
        }

        internal void InitializeNavigation() {
            // generate the starting navigation information
            mNavigationManager.IsWorldCreation = false;
            mNavigationManager.UpdateNavMesh();
            mNavigationManager.UpdateAirBiasFieldForAll(GameObjectEnum.MassTile, 64, 0.5f);
            mNavigationManager.UpdateAirBiasFieldForAll(GameObjectEnum.PortalTile, 64, 0.5f);
        }

        private void GenerateStartingArea() {
            mObjectManager.CreateTile(mWorldCenter, GameObjectEnum.PlatformTile, true);
            mObjectManager.CreateTile(mWorldCenter + Vector2.UnitX, GameObjectEnum.PlatformTile, true);
            mObjectManager.CreateTile(mWorldCenter - Vector2.UnitX, GameObjectEnum.PlatformTile, true);
            mObjectManager.CreateTile(mWorldCenter + Vector2.UnitY, GameObjectEnum.PlatformTile, true);
            mObjectManager.CreateTile(mWorldCenter - Vector2.UnitY, GameObjectEnum.PlatformTile, true);
            mObjectManager.CreateTile(mWorldCenter + Vector2.UnitX + Vector2.UnitY, GameObjectEnum.BarrackTile, true);
            mObjectManager.CreateTile(mWorldCenter + Vector2.UnitX - Vector2.UnitY, GameObjectEnum.StockTile, true);
        }

        private void GenerateMassSources() {
            for (var radius = 0; radius < mWorldSize.ToVector2().Length(); radius++) {
                var pos = Vector2.Zero;
                int attempts;
                for (attempts = 10; attempts > 0; attempts--) {
                    var angle = mRandom.NextDouble() * Math.PI * 2;
                    pos = mWorldCenter + radius * new Vector2((float) Math.Sin(angle), (float) Math.Cos(angle));
                    if (pos.X >= 0 && pos.Y >= 0 && pos.X < mWorldSize.X && pos.Y < mWorldSize.Y && mSpaceIsUsed[(int) pos.X, (int) pos.Y] == 0) {
                        break;
                    }
                }

                if (attempts > 0) {
                    mObjectManager.CreateTile(pos, GameObjectEnum.MassTile);
                    MarkSpaceAsUsed(pos, mMassMinDistance, byte.MaxValue);
                }
            }
        }

        protected void GeneratePortals(int minPortalDistance = 20, int minCornerDistanceX = -1, int minCornerDistanceY = -1, int maxBorderDistance = 5) {
            // randomly spawns 4 portals (one near each border). Conditions will be met if possible.

            if (minCornerDistanceX == -1) {
                double calculatedCornerDistance = Math.Round(Global.WorldWidth * 0.25);
                minCornerDistanceX = Convert.ToInt32(calculatedCornerDistance);
            }

            if (minCornerDistanceY == -1) {
                double calculatedCornerDistance = Math.Round(Global.WorldWidth * 0.25);
                minCornerDistanceY = Convert.ToInt32(calculatedCornerDistance);
            }

            int halfWorldWidth = Global.WorldWidth / 2;
            int halfWorldHeight = Global.WorldHeight / 2;

            int spawnAreaXsize = Global.WorldWidth - (2 * minCornerDistanceX);
            int spawnAreaYsize = Global.WorldHeight - (2 * minCornerDistanceY);

            // defines the area-border of where the 2 Portal can spawn along the field-border in X-direction
            int[] spawnAreaXborder = new int[] {
                halfWorldWidth - (spawnAreaXsize / 2),
                halfWorldWidth + (spawnAreaXsize / 2) - 1
            };
            // defines the area-border where the 2 Portal can spawn along the field-border in Y-direction
            int[] spawnAreaYborder = new int[] {
                halfWorldHeight - (spawnAreaYsize / 2),
                halfWorldHeight + (spawnAreaYsize / 2) - 1
            };

            Random rndm = new Random();

            // calculates random position for the portals clockwisely

            int maxPositionX = Global.WorldWidth - 1;
            int maxPositionY = Global.WorldHeight - 1;

            // portal on the upper horizontal(X-direction) field-border
            Vector2 portal1 = new Vector2();
            portal1.X = rndm.Next(spawnAreaXborder[0], spawnAreaXborder[1] + 1);
            portal1.Y = rndm.Next(0, maxBorderDistance + 1);

            // portal on the right vertical(Y-direction) field-border
            Vector2 portal2 = new Vector2();
            portal2.X = maxPositionX - (rndm.Next(0, maxBorderDistance + 1));
            portal2.Y = rndm.Next(spawnAreaYborder[0], spawnAreaYborder[1]);

            while (Vector2.Distance(portal1, portal2) < minPortalDistance) {
                if (Convert.ToInt32(portal2.Y) == spawnAreaXborder[1]) {
                    break;
                }

                portal2.Y = rndm.Next(Convert.ToInt32(portal2.Y) + 1, spawnAreaXborder[1] + 1);
            }

            // portal on the lower horizontal(X-direction) field-border
            Vector2 portal3 = new Vector2();
            portal3.X = rndm.Next(spawnAreaXborder[0], spawnAreaXborder[1] + 1);
            portal3.Y = maxPositionY - rndm.Next(0, maxBorderDistance + 1);

            while (Vector2.Distance(portal2, portal3) < minPortalDistance) {
                if (Convert.ToInt32(portal3.X) == spawnAreaXborder[0]) {
                    break;
                }

                portal3.X = rndm.Next(spawnAreaXborder[0], Convert.ToInt32(portal3.X));
            }

            // portal on the right vertical(Y-direction) field-border
            Vector2 portal4 = new Vector2();
            portal4.X = rndm.Next(0, maxBorderDistance + 1);
            portal4.Y = rndm.Next(spawnAreaYborder[0], spawnAreaYborder[1]);

            while (Vector2.Distance(portal1, portal4) < minPortalDistance) {
                if (Convert.ToInt32(portal4.Y) == spawnAreaYborder[1]) {
                    break;
                }

                portal4.Y = rndm.Next(Convert.ToInt32(portal4.Y), spawnAreaXborder[1] + 1);
            }

            while (Vector2.Distance(portal3, portal4) < minPortalDistance) {
                if (Convert.ToInt32(portal4.Y) == spawnAreaYborder[0]) {
                    break;
                }

                portal4.Y = rndm.Next(spawnAreaXborder[0], Convert.ToInt32(portal4.Y));
            }

            mObjectManager.CreateTile(portal1, GameObjectEnum.PortalTile, true);
            mObjectManager.CreateTile(portal2, GameObjectEnum.PortalTile, true);
            mObjectManager.CreateTile(portal3, GameObjectEnum.PortalTile, true);
            mObjectManager.CreateTile(portal4, GameObjectEnum.PortalTile, true);
        }

        protected void SpawnMinions(MinionAiType defaultAi = MinionAiType.IdleMinionAi) {
            var allBarracks = mObjectManager.GetAllObjects(GameObjectEnum.BarrackTile);

            foreach (var gameObject in allBarracks) {
                if (gameObject is BarrackTile barrack) {
                    barrack.DefaultAi = defaultAi;
                    barrack.AddResource(barrack.RequiredResourcesForSpawn);
                }
            }
        }

        private void MarkSpaceAsUsed(Vector2 center, float radius, byte mask) {
            for (var x = 0; x < mWorldSize.X; x++) {
                for (var y = 0; y < mWorldSize.Y; y++) {
                    if ((center - new Vector2(x, y)).Length() <= radius) {
                        mSpaceIsUsed[x, y] |= mask;
                    }
                }
            }
        }
    }
}