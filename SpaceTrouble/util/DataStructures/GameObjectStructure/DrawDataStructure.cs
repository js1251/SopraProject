using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

// Created by Jakob Sailer

namespace SpaceTrouble.util.DataStructures.GameObjectStructure {
    internal sealed class DrawDataStructure {
        private GameDataStructure ParentStructure { get; } // the overarching data-structure for this game
        private Tile[,] Tiles { get; } // a list of objects to draw on-screen
        private Tile[,] GhostTiles { get; set; }
        public List<GameObject> GameObjectDrawOrder { get; } // a list of objects to draw on-screen
        private List<GameObject> FlyingDrawOrder { get; }
        private RectangleF WindowBounds { get; set; } // a rectangle around the screen-edges
        private Vector2 WindowBufferFactor { get; } // a buffer factor for the clipping rectangle around the screen-edges
        private RectangleF WindowBuffer { get; set; } // the resulting rectangle around the scree-edges including the buffer
        public bool IsDebug { get; set; }

        public DrawDataStructure(GameDataStructure parentStructure) {
            ParentStructure = parentStructure;

            Tiles = ParentStructure.ObjectData.GetAllTiles();
            GhostTiles = new Tile[Global.WorldWidth, Global.WorldHeight];
            GameObjectDrawOrder = new List<GameObject>();
            FlyingDrawOrder = new List<GameObject>();
            WindowBufferFactor = new Vector2(1, 2); // number of tiles in buffer-space
        }

        public void Update() {
            if (!IsDebug) {
                GenerateWindowBounds();
            }

            DrawOrder();
        }

        private void DrawOrder() {
            GameObjectDrawOrder.Clear();
            FlyingDrawOrder.Clear();

            for (var y = 0; y < Global.WorldHeight; y++) {
                for (var x = 0; x < Global.WorldWidth; x++) {
                    // only draw things that are actually on-screen
                    if (!IsOnScreen(x, y)) {
                        continue;
                    }

                    var currentTile = Tiles[x, y];
                    var currentObjects = ParentStructure.ObjectData.ObjectsOnTiles[x, y];
                    var currentGhost = GhostTiles[x, y];
                    // if the currentTile is a "low" tile draw it first, then the objects ontop
                    if (currentTile is PlatformTile || currentTile is EmptyTile || currentTile is GeneratorTile || currentTile is PortalTile) {
                        GameObjectDrawOrder.Add(currentTile);
                        AddGhostTile(currentGhost);
                        AddObjectsOntop(currentObjects);
                    } else {
                        // if its a "tall" tile draw the objects first, then the tall tile
                        AddObjectsOntop(currentObjects);
                        GameObjectDrawOrder.Add(currentTile);
                        AddGhostTile(currentGhost);
                    }
                }
            }
            GameObjectDrawOrder.AddRange(FlyingDrawOrder);
            GhostTiles = new Tile[Global.WorldWidth, Global.WorldHeight];
        }

        private void AddObjectsOntop(List<GameObject> objectsOnTile) {
            if (objectsOnTile != null) {
                objectsOnTile = objectsOnTile.OrderBy(gameObject => gameObject.WorldPosition.Y).ToList();
                foreach (var gameObject in objectsOnTile) {
                    if (gameObject.DrawFlying) {
                        FlyingDrawOrder.Add(gameObject);
                        continue;
                    }
                    GameObjectDrawOrder.Add(gameObject);
                }
            }
        }

        private void AddGhostTile(Tile tile) {
            if (tile != null) {
                if (tile is LaboratoryTile labGhost) {
                    labGhost.UpdateLaser(new GameTime());
                }
                GameObjectDrawOrder.Add(tile);
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            foreach (var gameObject in GameObjectDrawOrder) {
                gameObject.Draw(spriteBatch);
            }
        }

        public void InsertGhostTile(Tile tile) {
            var tilePos = CoordinateManager.WorldToTile(tile.WorldPosition).ToPoint();
            if (IsOnScreen(tilePos.X, tilePos.Y)) {
                GhostTiles[tilePos.X, tilePos.Y] = tile;
            }
        }

        public Tile GetGhostTile(Vector2 tilePos) {
            return IsOnScreen((int) tilePos.X, (int) tilePos.Y) ? GhostTiles[(int) tilePos.X, (int) tilePos.Y] : null;
        }

        public void DrawDebug(SpriteBatch spriteBatch, SpriteFont font, Vector2 cursorPos) {
            // screen-bounds
            var vertices = new[] {
                new Vector2 {X = WindowBounds.Left, Y = WindowBounds.Top}, // top left
                new Vector2 {X = WindowBounds.Right, Y = WindowBounds.Top}, // top right
                new Vector2 {X = WindowBounds.Right, Y = WindowBounds.Bottom}, // bottom right
                new Vector2 {X = WindowBounds.Left, Y = WindowBounds.Bottom} // bottom left
            };
            spriteBatch.DrawPolygon(vertices, vertices.Length, Color.Red, 5f);

            // view culling threshold
            vertices = new[] {
                new Vector2 {X = WindowBuffer.Left, Y = WindowBuffer.Top}, // top left
                new Vector2 {X = WindowBuffer.Right, Y = WindowBuffer.Top}, // top right
                new Vector2 {X = WindowBuffer.Right, Y = WindowBuffer.Bottom}, // bottom right
                new Vector2 {X = WindowBuffer.Left, Y = WindowBuffer.Bottom} // bottom left
            };
            spriteBatch.DrawPolygon(vertices, vertices.Length, Color.Orange, 5f);

            // draw renderOrder
            var count = 0;
            foreach (var gameObject in WorldGameState.ObjectManager.DataStructure.DrawData.GameObjectDrawOrder) {
                if (gameObject is Tile) {
                    spriteBatch.DrawString(font, count + "", gameObject.WorldPosition + Vector2.UnitY * 5f, Color.White, 0f, Vector2.Zero, .25f, SpriteEffects.None, 0);
                } else {
                    spriteBatch.DrawString(font, count + "", gameObject.WorldPosition, Color.Yellow, 0f, Vector2.Zero, .3f, SpriteEffects.None, 0);
                }

                count++;
            }

            // tile - Cell under cursor
            var currentCell = CoordinateManager.ScreenToCell(cursorPos);
            currentCell *= new Vector2(Global.TileWidth, Global.TileHeight);
            currentCell -= new Vector2(Global.TileWidth / 2f, 0);
            var cellRectangle = new Rectangle((int) currentCell.X, (int) currentCell.Y, Global.TileWidth, Global.TileHeight);
            vertices = new[] {
                new Vector2 {X = cellRectangle.Left, Y = cellRectangle.Top}, // top left
                new Vector2 {X = cellRectangle.Right, Y = cellRectangle.Top}, // top right
                new Vector2 {X = cellRectangle.Right, Y = cellRectangle.Bottom}, // bottom right
                new Vector2 {X = cellRectangle.Left, Y = cellRectangle.Bottom} // bottom left
            };
            spriteBatch.DrawPolygon(vertices, vertices.Length, Color.Blue);

            // Screen-center
            var screenCenter = CoordinateManager.ScreenToWorld(new Vector2(Global.WindowWidth, Global.WindowHeight) / 2f);
            spriteBatch.DrawPoint(screenCenter, 3, Color.Blue);
        }

        private void GenerateWindowBounds() {
            // the current window needs to be updated every time since the camera moves
            var worldTopLeft = CoordinateManager.ScreenToWorld(Vector2.Zero);
            var worldBottomRight = CoordinateManager.ScreenToWorld(new Vector2(Global.WindowWidth, Global.WindowHeight));
            WindowBounds = RectangleF.FromLTRB(worldTopLeft.X, worldTopLeft.Y, worldBottomRight.X, worldBottomRight.Y);

            var bufferOffset = new Vector2(Global.TileWidth, Global.TileHeight) * WindowBufferFactor;

            WindowBuffer = RectangleF.FromLTRB(
                worldTopLeft.X - bufferOffset.X,
                worldTopLeft.Y - bufferOffset.Y,
                worldBottomRight.X + bufferOffset.X,
                worldBottomRight.Y + bufferOffset.Y
            );
        }

        private bool IsOnScreen(int x, int y) {
            // check for valid x, y into Tiles[,]
            if (x < 0 || x >= Global.WorldWidth || y < 0 || y >= Global.WorldHeight) {
                return false;
            }

            var pos = Tiles[x, y].WorldPosition;
            return pos.X > WindowBuffer.Left && pos.X < WindowBuffer.Right && pos.Y > WindowBuffer.Top && pos.Y < WindowBuffer.Bottom;
        }
    }
}