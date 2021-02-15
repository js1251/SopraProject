using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameState;
using SpaceTrouble.InputOutput;
using SpaceTrouble.Menu.MenuElements;
using SpaceTrouble.util.Tools.Assets;

// Created by Jakob Sailer

namespace SpaceTrouble.Menu {
    internal sealed class TutorialEntry {
        internal string Heading { get; }
        internal string Information { get; }
        internal Texture2D Image { get; }
        internal TutorialEntry(string heading, Texture2D image, string text) {
            Heading = heading;
            Image = image;
            Information = text;
        }
    }

    internal sealed class TutorialMenuState : GameState.GameState {
        private Panel Panel { get; set; }
        private MenuButton BackButton { get; set; }
        private MenuButton LeftButton { get; set; }
        private MenuButton RightButton { get; set; }
        private Image Image { get; set; }
        private Label Heading { get; set; }
        private Label Information { get; set; }
        private List<TutorialEntry> Pages { get; }
        private int CurrentPage { get; set; }

        public TutorialMenuState(string stateName) : base(stateName) {
            CurrentPage = 0;
            Pages = new List<TutorialEntry>();
        }

        internal override void Initialize() {
        }

        internal override void LoadContent() {
            var buttonTexture = Assets.Textures.InterfaceTextures.Button;
            var font = Assets.Fonts.GuiFont01;
            BackButton = new MenuButton(buttonTexture, font, "Back");
            LeftButton = new MenuButton(buttonTexture, font, "<");
            RightButton = new MenuButton(buttonTexture, font, ">");
            Heading = new Label(font, default, "", 28f);
            Information = new Label(font);
            Image = new Image(buttonTexture);

            var imagePanel = new Panel(new Vector4(0.08f,0.5f,0.45f,0.6f), Vector2.Zero, new MenuElement[,] {
                {Image}
            });

            var headingPanel = new Panel(new Vector4(0.1f, 0, 0.42f, 0.15f), Vector2.Zero, new MenuElement[,] {
                {Heading}
            });

            var informationPanel = new Panel(new Vector4(0.95f, 0.8f, 0.46f, 0.8f), new Vector2(0.05f, 0), new MenuElement[,] {
                {Information}
            });

            Information.Parent = informationPanel;

            var buttonPanel = new Panel(new Vector4(0.1f, 0.925f, 0.42f, 0.1f), new Vector2(0.1f,0.05f), new MenuElement[,] {
                {LeftButton, BackButton, RightButton}
            });

            Panel = new Panel(new Vector4(0.5f, 0.5f, 0.9f,0.7f), Vector2.Zero, new MenuElement[,] {
                {imagePanel, headingPanel, informationPanel, buttonPanel}
            }, Assets.Textures.InterfaceTextures.GuiTutorial);

            LoadPages();
        }

        public override void Update(GameTime gameTime, Dictionary<ActionType, InputAction> inputs) {
            LeftButton.TextColor = CurrentPage == 0 ? Color.WhiteSmoke : default;
            RightButton.TextColor = CurrentPage == Pages.Count - 1 ? Color.WhiteSmoke : default;

            if (LeftButton.GetPushState(true) && CurrentPage > 0) {
                CurrentPage--;
                return;
            }

            if (RightButton.GetPushState(true) && CurrentPage < Pages.Count - 1) {
                CurrentPage++;
                return;
            }

            Heading.Text = Pages[CurrentPage].Heading;
            Image.SetTexture(Pages[CurrentPage].Image);
            Information.Text = Pages[CurrentPage].Information;
            Panel.Update(inputs);
        }

        internal override void CheckForStateChanges(GameStateManager stateManager, Dictionary<ActionType, InputAction> inputs) {
            if (BackButton.GetPushState(true)) {
                stateManager.RemoveActiveGameState();
            }

            base.CheckForStateChanges(stateManager, inputs);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Begin();
            Panel.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void LoadPages() {
            var text = "In Spacetrouble you have to manage resource flows, create your own base by placing buildings/ platforms and set ";
            text += "up a functional defense. Your final goal is to stabilize all portals (enemy spawns) and to have at least one ";
            text += "Minion survive.\n\nIn the following pages you will get to know the game mechanics and controls. If you don't ";
            text += "fully understand a page, don't worry about it! Just go ahead and read the next.\nYou can acces the tutorial ";
            text += "screen any time during the game.";
            Pages.Add(new TutorialEntry("Welcome to Spacetrouble!", Assets.Textures.TutorialScreens.GameLogo, text));

            text = "Let's take a quick look at your interface. Here you see a quick overview of what it's all about\n\n";
            text += "1. Your current amount of resources and how much will be reduced\n";
            text += "2. The buildings selection\n";
            text += "3. Delete button to delete construction sites\n";
            text += "4. Amount of Minions you have and the current maximum";
            text += "5. Task manager shows how many minions are assigned to each task and how many are idle\n";
            text += "6. Visual settings for Towers and ammo\n";
            text += "7. Current game time and wave countdown/ enemies left of the wave\n";
            text += "8. Settings for game speed\n";
            Pages.Add(new TutorialEntry("Interface overview", Assets.Textures.TutorialScreens.InterfaceOverview, text));

            text = "Overview of what you can do.\n\n";
            text += "Building:\n";
            text += "Place building -> LMB on building thumbnail to select and place it with LMB\n";
            text += "Delete construction site -> LMB on DELETE-button and LMB on construction site\n";
            text += "Cancel selection -> RMB anywhere\n";
            text += "\nMinions:\n";
            text += "Assign Minion to a task: LMB on the '+/-'-button beside the task\n";
            text += "Remove Minion from a task: RMB on the '+/-'-button beside the task\n";
            text += "Change auto-assignment for new minions: LMB on '<>'-button\n";
            text += "Set priority: LMB (without any selection) on a construction site/ tower/ barrack\n";
            text += "\nGeneral:\n";
            text += "Pause game: P-key or pause-button below the game time\n";
            text += "Open Menu: ESC-key\n";
            text += "Screen scroll: WASD-keys or move mouse to screen border";
            Pages.Add(new TutorialEntry("Controls", Assets.Textures.TutorialScreens.ControlsIcon, text));

            text = "There are 3 types of resource in Spacetrouble: Energy, Mass and Food. You can see how many resources you have. ";
            text += "You can also see, how many will be spend/ are currently needed.\n\n";
            text += "\nEach resource can be generated and used differently (details explained later)\n";
            text += "Food: from KITCHEN, used for building and spawning Minions\n";
            text += "Energy: from GENERATOR, used for building and loading towers\n";
            text += "Mass: from EXTRACTOR, used for building\n";
            text += "\nYou will start with a certain amount of each resources, which are stored on a single tile, the STOCK.";
            text += "Yet every other resource is stored in building, where it is produced.";
            Pages.Add(new TutorialEntry("Resources", Assets.Textures.TutorialScreens.ResourceDisplay, text));

            text = "Now let's talk about buildings. Each building has different costs which can be seen by hovering over ";
            text += "a building in the selection bar (1.). Some cost, such as of the tower, will be increased the more ";
            text += "buildings of that type you build.\n";
            text += "You can select/ place a building by clicking (LMB) on it and canceling your Selection with RMB. Your ";
            text += "base must be connected, so buildings can only be placed beside a PLATFORM (unlike 2.).\n\n";
            text += "When placing a building on the map, it will become a construction site and construction Minions ";
            text += "will carry resources to it. The amount of resources which is still needed, is displayed on the ";
            text += "Construction area itself (3.). Once the last resource arrived, the building will be finished ";
            text += "and gain it's full functionality. As long a building is still unfinished, you can delete ";
            text += "it any time by using the DELETE-button beside the selection bar(4.).\n\n";
            text += "But once a building is finished, it will stay there forever!\n\n";
            text += "In the following pages, each building will be explained in detail.";
            Pages.Add(new TutorialEntry("Construction and buildings", Assets.Textures.TutorialScreens.ConstructionExample, text));

            text = "Well, your Minions need to walk somewhere, therefor Platforms exist. They are the main";
            text += "component of your base, since all buildings must border to a Platform.";
            Pages.Add(new TutorialEntry("Platform", Assets.Textures.Thumbnails.Platform, text));

            text = "These two buildings have a fairly the same functionality.\nA Generator (left) ";
            text += "produces and stocks Energy while A Kitchen (right) does the same with Food.";
            Pages.Add(new TutorialEntry("Generator and Kitchen", Assets.Textures.TutorialScreens.GeneratorKitchen, text));

            text = "The extractor (left) produces and stores a resource, just like the kitchen and the ";
            text += "generator, but it can only be build on a Mass ore source (right).\n\n";
            text += "The resource a Extractor produces is Mass, which is needed to build any other building.";
            Pages.Add(new TutorialEntry("Extractor", Assets.Textures.TutorialScreens.ExtractorMass, text));

            text = "Barracks are the home of your Minions. If you want to have more Minions, ";
            text += "you have to build more Barracks. The amount of Barracks you build defines the ";
            text += "maximum of Minions you can have. Both current and maximum amount of Minions is ";
            text += "displayed in the button left Corner of the screen.\n\n";
            text += "When a Barrack is build, Food Minions will start to carry the resource Food to ";
            text += "a the Barrack. For each Food which arrives at a Barrack, 1 new Minion will spawn ";
            text += "there. One Barrack can shelter up to 4 Minions.\n";
            text += "You can see how many free rooms a Barrack still has by checking the amount of food ";
            text += "icons on the Barrack.\n";
            text += "(left barrack still has 2 free rooms, the right one is full)\n\n";
            text += "The left barrack still has 2 free rooms, while the right one is full.";
            Pages.Add(new TutorialEntry("Barrack", Assets.Textures.TutorialScreens.ExampleKitchen, text));

            text = "Tower are your defensive buildings, they shoot at all invasive";
            text += "enemies within their range.\n\n";
            text += "A Tower needs Energy to shoot. When a Tower is build first, it has a ";
            text += "full Energy ammo but it will need to be reloaded over time. You can ";
            text += "see the amount of Energy, which is currently needed and the ammo, ";
            text += "on the tower itself.\n";
            text += "(left Tower needs 0 Energy and has full ammo, right tower needs 4 energy";
            text += "and has nearly 0 ammo)\n\n";
            text += "Defense Minions will carry Energy resources to the tower ";
            text += "to reload it.";
            Pages.Add(new TutorialEntry("Tower", Assets.Textures.TutorialScreens.ExampleTower, text));

            text = "Portals:\n";
            text += "The Portals are the source of evil. They appear near the border of the map ";
            text += "and constantly spawn flying aliens in waves. Aliens are your enemies and will ";
            text += "try to kill your Minions, so you must kill them first.\n";
            text += "Once your base borders to a Portal, it will start spawning walking enemies ";
            text += "too. The wave countdown decreases after each wave during the game.\n\n";
            text += "Laboratory:\n";
            text += "Laboratory can only be build on a portal. Once a Laboratory was";
            text += "build, the portal it was build on will be stabilized.  A stabilized ";
            text += "Portal only spawns walking Aliens, which will make it easier";
            text += "to control.\n\n"; 
            text += "Your final goal is to build a Laboratory on each portal and survive";
            text += "the final wave before the portals get closed.";
            Pages.Add(new TutorialEntry("Portals and Laboratory", Assets.Textures.TutorialScreens.PortalLaboratory, text));

            text = "You control your Minions by giving assigning them to specific tasks. The Tasks are listed on the right ";
            text += "corner of the screen (1.).\n";
            text += "The amount of Minions you can have depends on how many Barracks were build and is shown in the left";
            text += "corner of the screen.\n";
            text += "2: current Minions, 3: maximum Minions)\n\n";
            text += "The tasks are...\n";
            text += "Building:\nConstruct the buildings you place on the map.\n";
            text += "Defense:\nReloading the towers which lack of Energy.\n";
            text += "Food:\nBring food to your Barrack to your a free Barrack to spawn new Minions.\n\n";
            text += "Assign a Minion to a task group by left-clicking (LMB) and remove one by right-clicking ";
            text += "(RMB) on the ' +/ -' button (4.) beside that task group. \n";
            text += "It's important to organize your Minions to make them work productively. You can also ";
            text += "see how effective they are: The green bar and the number (5.) show, how many Minions ";
            text += "have something to do in their task.\n";
            text += "When a Minions spawns, it will be auto-assigned to the task on the bottom (6.). You can ";
            text += "change that task by left-clicking on the '<>' button beside it (7.)";
            Pages.Add(new TutorialEntry("Minions", Assets.Textures.TutorialScreens.MinonControle, text));

        }
    }
}
