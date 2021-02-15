using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

// created by Jakob Sailer

namespace SpaceTrouble.World.UserInterface {
    internal sealed class MinionTasksUi : UiElement {
        private Dictionary<MinionAiType, (MenuButton, Label)> AssignButtons { get; set; }
        private MenuButton DefaultAssignButton { get; set; }
        private Label DefaultAssignLabel { get; set; }
        private Label UnassignedCount { get; set; }
        private Dictionary<MinionAiType, MenuBar> AssignedBars { get; set; }
        private Dictionary<MinionAiType, MenuBar> BusyBars { get; set; }
        public MinionTasksUi(Vector4 screenBounds) : base(screenBounds) {
        }

        internal override void LoadContent() {
            var buttonTexture = Assets.Textures.InterfaceTextures.Button;
            var barTexture = Assets.Textures.InterfaceTextures.GuiBar;
            var font = Assets.Fonts.GuiFont01;
            var backgroundTexture = Assets.Textures.InterfaceTextures.GuiMinionTasks;

            AssignButtons = new Dictionary<MinionAiType, (MenuButton, Label)> {
                {MinionAiType.ConstructionMinionAi, (new MenuButton(buttonTexture, font, "+/-") {ToolTip = "LMB: increase, RMB: decrease"}, new Label(font))},
                {MinionAiType.FoodMinionAi, (new MenuButton(buttonTexture, font, "+/-") {ToolTip = "LMB: increase, RMB: decrease"}, new Label(font))},
                {MinionAiType.DefenceMinionAi, (new MenuButton(buttonTexture, font, "+/-") {ToolTip = "LMB: increase, RMB: decrease"}, new Label(font))},
            };

            DefaultAssignButton = new MenuButton(buttonTexture, font, "<>") { ToolTip = "Cycle default assignment" };

            AssignedBars = new Dictionary<MinionAiType, MenuBar> {
                {MinionAiType.ConstructionMinionAi, new MenuBar(barTexture, Color.OrangeRed, font)},
                {MinionAiType.FoodMinionAi, new MenuBar(barTexture, Color.OrangeRed, font)},
                {MinionAiType.DefenceMinionAi, new MenuBar(barTexture, Color.OrangeRed, font)}
            };

            BusyBars = new Dictionary<MinionAiType, MenuBar> {
                {MinionAiType.ConstructionMinionAi, new MenuBar(barTexture, Color.ForestGreen, font)},
                {MinionAiType.FoodMinionAi, new MenuBar(barTexture, Color.ForestGreen, font)},
                {MinionAiType.DefenceMinionAi, new MenuBar(barTexture, Color.ForestGreen, font)}
            };

            var assignPanel = new Panel(new Vector4(0.07f,0.5f,0.2f,1), new Vector2(0, 0.05f), new MenuElement[,] {
                {AssignButtons[MinionAiType.ConstructionMinionAi].Item1, AssignButtons[MinionAiType.ConstructionMinionAi].Item2},
                {AssignButtons[MinionAiType.FoodMinionAi].Item1, AssignButtons[MinionAiType.FoodMinionAi].Item2},
                {AssignButtons[MinionAiType.DefenceMinionAi].Item1, AssignButtons[MinionAiType.DefenceMinionAi].Item2}
            });

            var typeName = new Panel(new Vector4(0.3f, 0.5f, 0.2f, 1), new Vector2(0, 0.05f), new MenuElement[,] {
                {new Label(Assets.Fonts.GuiFont01, default, "Constr. ", 12f)},
                {new Label(Assets.Fonts.GuiFont01, default, "Food    ", 12f)},
                {new Label(Assets.Fonts.GuiFont01, default, "Defence ", 12f)}
            });

            var assignedBarPanel = new Panel(new Vector4(0.9f,0.5f,0.46f,1), new Vector2(0, 0.05f), new MenuElement[,] {
                {AssignedBars[MinionAiType.ConstructionMinionAi]},
                {AssignedBars[MinionAiType.FoodMinionAi]},
                {AssignedBars[MinionAiType.DefenceMinionAi]}
            });

            var idleBarPanel = new Panel(new Vector4(0.9f, 0.5f, 0.46f, 1), new Vector2(0, 0.05f), new MenuElement[,] {
                {BusyBars[MinionAiType.ConstructionMinionAi]},
                {BusyBars[MinionAiType.FoodMinionAi]},
                {BusyBars[MinionAiType.DefenceMinionAi]},
            });

            var assignmentPanel = new Panel(new Vector4(0,0.4f,1,0.52f), Vector2.Zero, new MenuElement[,] {
                {assignPanel},
                {typeName},
                {assignedBarPanel},
                {idleBarPanel}
            });

            UnassignedCount = new Label(Assets.Fonts.GuiFont01);

            var unassignedInfo = new Panel(new Vector4(0.78f,0.89f,0.2f, 0.15f), Vector2.Zero, new MenuElement[,] {
                {UnassignedCount}
            });

            var defaultAssignPanel = new Panel(new Vector4(0.062f,0.89f,0.1f,0.15f), new Vector2(0,0), new MenuElement[,] {
                {DefaultAssignButton}
            });

            DefaultAssignLabel = new Label(Assets.Fonts.GuiFont01);

            var defaultAssignLabel = new Panel(new Vector4(0.25f, 0.89f, 0.3f, 0.15f), Vector2.Zero, new MenuElement[,] {
                {DefaultAssignLabel}
            });

            Panel = new Panel(ScreenBounds, new Vector2(0.1f, 0.1f), new MenuElement[,] {
                {assignmentPanel, defaultAssignPanel, defaultAssignLabel, unassignedInfo}
            }, backgroundTexture);
        }

        private bool mLoadSingleton;

        internal override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            if (!mLoadSingleton) {
                DefaultAssignLabel.Text = WorldGameState.TaskManager.DefaultAi.ToString().Replace("MinionAi", "");
                mLoadSingleton = true;
            }
            
            CheckButtonSelection();
            var minionCount = WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.Minion).Count;
            UpdateBars(minionCount);
            UpdateCounter(minionCount);
            base.Update(gameTime, inputs);
        }
        
        private void CheckButtonSelection() {
            if (DefaultAssignButton.GetPushState(true)) {
                UpdateBarrackDefaultSpawn();
                DefaultAssignLabel.Text = WorldGameState.TaskManager.DefaultAi.ToString().Replace("MinionAi", "");
            } else if (DefaultAssignButton.GetPushState(true, ActionType.MouseRightClick)) {
                UpdateBarrackDefaultSpawn(true);
                DefaultAssignLabel.Text = WorldGameState.TaskManager.DefaultAi.ToString().Replace("MinionAi", "");
            }

            foreach (var (type, (button, _)) in AssignButtons) {
                if (button.GetPushState(true)) {
                    WorldGameState.TaskManager.PushTaskFromTo(MinionAiType.IdleMinionAi, type);
                } else if (button.GetPushState(true, ActionType.MouseRightClick)) {
                    WorldGameState.TaskManager.PushTaskFromTo(type, MinionAiType.IdleMinionAi);
                }
            }
        }

        private static void UpdateBarrackDefaultSpawn(bool decrement = false) {
            if (decrement) {
                WorldGameState.TaskManager.DefaultAi -= 1;
            } else {
                WorldGameState.TaskManager.DefaultAi += 1;
            }

            var allAiCount = Enum.GetValues(typeof(MinionAiType)).Length;

            if ((int)WorldGameState.TaskManager.DefaultAi >= allAiCount) {
                WorldGameState.TaskManager.DefaultAi = 0;
            } else if ((int)WorldGameState.TaskManager.DefaultAi < 0) {
                WorldGameState.TaskManager.DefaultAi = (MinionAiType)allAiCount - 1;
            }

            foreach (var gameObject in WorldGameState.ObjectManager.GetAllObjects(GameObjectEnum.BarrackTile)) {
                if (gameObject is BarrackTile barrack) {
                    barrack.DefaultAi = WorldGameState.TaskManager.DefaultAi;
                }
            }
        }

        private void UpdateBars(int minionCount) {
            foreach (var task in AssignedBars.Keys) {
                var assignedBar = AssignedBars[task];
                var busyBar = BusyBars[task];

                var assignedMinions = (float)WorldGameState.TaskManager.AssignedCounter[task];
                var busyMinions = (float)WorldGameState.TaskManager.BusyCounter[task];
                var idleMinions = (float)WorldGameState.TaskManager.IdleCounter[task];

                var assignedFillAmount = minionCount > 0 ? assignedMinions / minionCount : 0;
                var busyFillAmount = (busyMinions + idleMinions) > 0 ? busyMinions / (busyMinions + idleMinions) : 0;

                var efficiency = Math.Round(busyFillAmount * 100) + "%";
                assignedBar.Text = busyFillAmount < 0.25f ? efficiency : "";
                busyBar.Text = busyFillAmount < 0.25f ? "" : efficiency;

                busyFillAmount *= assignedFillAmount;

                assignedBar.FillAmount = assignedFillAmount;
                busyBar.FillAmount = busyFillAmount;

            }
        }

        private void UpdateCounter(int minionCount) {
            foreach (var (task, (_ ,label)) in AssignButtons) {
                label.Text = WorldGameState.TaskManager.AssignedCounter[task] + "";
            }

            var unassignedCount = WorldGameState.TaskManager.AssignedCounter[MinionAiType.IdleMinionAi];

            UnassignedCount.Text = "Unassigned: " + unassignedCount;
            UnassignedCount.TextColor = unassignedCount > minionCount * 0.1f ? Color.OrangeRed : default;
        }
    }
}