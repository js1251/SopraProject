using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.Tools;

// rewritten by Jakob Sailer

namespace SpaceTrouble.Menu.MenuElements {
    internal sealed class Panel : MenuElement {
        private readonly MenuElement[,] mElements;
        private Vector2 RelativePadding { get; }
        private bool ShowOutLine { get; }
        private Vector4 RelativeBounds { get; }
        internal Vector2 Offset { get; set; }
        internal float Scale { get; set; }
        private Panel ParentPanel { get; set; }
        internal float Alpha { get; set; }
        internal float Rotation { get; set; }

        internal Panel(Vector4 relativeBounds, Vector2 relativePadding, MenuElement[,] elements, Texture2D backgroundTexture = null, bool showOutLine = false) : base(backgroundTexture) {
            mElements = elements;
            Alpha = 1f;
            Scale = 1f;
            Rotation = 0;

            foreach (var element in mElements) {
                if (element is Panel panel) {
                    panel.ParentPanel = this;
                }
            }

            RelativePadding = relativePadding;
            ShowOutLine = showOutLine;
            RelativeBounds = relativeBounds;
        }

        internal override void Update(Dictionary<ActionType, InputAction> inputs) {
            if (RelativeBounds != default) {
                mBounds = GetPanelBounds(ParentPanel);
            }

            ApplyOffset();
            ApplyScale();
            DoLayout();

            foreach (var menuElement in mElements) {
                menuElement?.Update(inputs);
            }
        }

        private Rectangle GetPanelBounds(Panel parentPanel) {
            var parentBounds = parentPanel?.mBounds ?? new Rectangle(0,0, Global.WindowWidth, Global.WindowHeight);

            var rectangleWidth = (int)(parentBounds.Width * RelativeBounds.Z);
            var rectangleHeight = (int)(parentBounds.Height * RelativeBounds.W);
            var rectangleX = (int) (parentBounds.X + (parentBounds.Width - rectangleWidth) * RelativeBounds.X);
            var rectangleY = (int) (parentBounds.Y + (parentBounds.Height - rectangleHeight) * RelativeBounds.Y);
            return new Rectangle(rectangleX, rectangleY, rectangleWidth, rectangleHeight);
        }

        private void ApplyOffset() {
            mBounds.X += (int) (mBounds.Width * Offset.X);
            mBounds.Y += (int) (mBounds.Height * Offset.Y);
        }

        private void ApplyScale() {
            var newWidth = (int) (mBounds.Width * Scale);
            var newHeight = (int)(mBounds.Height * Scale);
            mBounds.X -= (newWidth - mBounds.Width) / 2;
            mBounds.Y -= (newHeight - mBounds.Height) / 2;

            mBounds.Width = newWidth;
            mBounds.Height = newHeight;
        }

        private void DoLayout() {
            for (var x = 0; x < mElements.GetLength(0); x++) {
                for (var y = 0; y < mElements.GetLength(1); y++) {
                    var menuElementAtCell = mElements[x, y];

                    // a cell could be empty or a second panel with its own relations set so skip it
                    if (menuElementAtCell == null || (menuElementAtCell is Panel panel && panel.RelativeBounds != default)) {
                        continue;
                    }

                    // the position of the current cell is the position of the topLeft of the panel...
                    var cellPosition = new Point(mBounds.Left, mBounds.Top);

                    // ... plus some padding on the top and left
                    var padding = GetPadding();
                    cellPosition += padding;

                    // ... plus the offset of the previous cells above and to the left
                    cellPosition += GetCellOffset(x, y);

                    // TODO: what about center or right floating
                    var elementWidth = mBounds.Width / mElements.GetLength(1);
                    var elementHeight = mBounds.Height / mElements.GetLength(0);

                    // ... plus some padding on the right and bottom
                    elementWidth -= padding.X * 2;
                    elementHeight -= padding.Y * 2;

                    menuElementAtCell.mBounds = new Rectangle(cellPosition.X, cellPosition.Y, elementWidth, elementHeight);
                }
            }
        }

        private Point GetPadding() {
            return (new Vector2(mBounds.Width, mBounds.Height) * RelativePadding * 0.5f).ToPoint();
        }

        private Point GetCellOffset(int x, int y) {
            var offset = new Point();
            while (y > 0) {
                offset.X += mBounds.Width / mElements.GetLength(1);
                y--;
            }

            while (x > 0) {
                offset.Y += mBounds.Height / mElements.GetLength(0);
                x--;
            }

            return offset;
        }

        internal void Draw(SpriteBatch spriteBatch) {
            Draw(spriteBatch, Alpha);
        }

        internal override void Draw(SpriteBatch spriteBatch, float alpha) {
            if (!mVisible) {
                return;
            }

            if (mTexture != null) {
                spriteBatch.Draw(mTexture, mBounds, null, Color.White * alpha, Rotation, Vector2.Zero, SpriteEffects.None, 0);
            }

            if (ShowOutLine) {
                var vertices = new[] {
                    new Vector2(mBounds.Left, mBounds.Top),
                    new Vector2(mBounds.Right, mBounds.Top),
                    new Vector2(mBounds.Right, mBounds.Bottom),
                    new Vector2(mBounds.Left, mBounds.Bottom)
                };
                spriteBatch.DrawPolygon(vertices, vertices.Length, Color.Aqua * alpha);
            }

            foreach (var element in mElements) {
                element?.Draw(spriteBatch, alpha);
            }
        }
    }
}