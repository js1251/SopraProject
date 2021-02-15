using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// Created by Jakob Sailer

namespace SpaceTrouble.util.Tools.Assets {
    internal sealed class Tiles {
        internal Texture2D Empty { get; private set; }
        internal Texture2D Platform { get; private set; }
        internal Texture2D Tower { get; private set; }
        internal Texture2D Barrack { get; private set; }
        internal Texture2D Kitchen { get; private set; }
        internal Texture2D Generator { get; private set; }
        internal Texture2D Mass { get; private set; }
        internal Texture2D Extractor { get; private set; }
        internal Texture2D Portal { get; private set; }
        internal Texture2D Laboratory { get; private set; }

        public void LoadContent(ContentManager content) {
            Empty = content.Load<Texture2D>("sprites/tiles/empty");
            Platform = content.Load<Texture2D>("sprites/tiles/platform");
            Tower = content.Load<Texture2D>("sprites/tiles/tower");
            Barrack = content.Load<Texture2D>("sprites/tiles/barrack");
            Kitchen = content.Load<Texture2D>("sprites/tiles/kitchen");
            Generator = content.Load<Texture2D>("sprites/tiles/generator");
            Mass = content.Load<Texture2D>("sprites/tiles/mass");
            Extractor = content.Load<Texture2D>("sprites/tiles/extractor");
            Portal = content.Load<Texture2D>("sprites/tiles/portal");
            Laboratory = content.Load<Texture2D>("sprites/tiles/laboratory");
        }
    }

    internal sealed class Stars {
        internal Texture2D Star01 { get; private set; }
        internal Texture2D Star02 { get; private set; }
        internal Texture2D Star03 { get; private set; }
        internal Texture2D Star04 { get; private set; }
        internal Texture2D Star05 { get; private set; }
        internal Texture2D Star06 { get; private set; }
        internal Texture2D Star07 { get; private set; }
        internal Texture2D Star08 { get; private set; }
        internal Texture2D Star09 { get; private set; }
        internal Texture2D Star10 { get; private set; }
        internal Texture2D Star11 { get; private set; }

        public void LoadContent(ContentManager content) {
            Star01 = content.Load<Texture2D>("sprites/stars/star01");
            Star02 = content.Load<Texture2D>("sprites/stars/star02");
            Star03 = content.Load<Texture2D>("sprites/stars/star03");
            Star04 = content.Load<Texture2D>("sprites/stars/star04");
            Star05 = content.Load<Texture2D>("sprites/stars/star05");
            Star06 = content.Load<Texture2D>("sprites/stars/star06");
            Star07 = content.Load<Texture2D>("sprites/stars/star07");
            Star08 = content.Load<Texture2D>("sprites/stars/star08");
            Star09 = content.Load<Texture2D>("sprites/stars/star09");
            Star10 = content.Load<Texture2D>("sprites/stars/star10");
            Star11 = content.Load<Texture2D>("sprites/stars/star11");
        }
    }

    internal sealed class Creatures {
        internal Texture2D Minion { get; private set; }
        internal Texture2D WalkingEnemy { get; private set; }
        internal Texture2D FlyingEnemy { get; private set; }

        public void LoadContent(ContentManager content) {
            Minion = content.Load<Texture2D>("sprites/creatures/friendly/minion"); // TODO: rename
            WalkingEnemy = content.Load<Texture2D>("sprites/creatures/enemy/walking"); // TODO: rename
            FlyingEnemy = content.Load<Texture2D>("sprites/creatures/enemy/flying"); // TODO: rename
        }
    }

    internal sealed class Thumbnails {
        internal Texture2D Platform { get; private set; }
        internal Texture2D Tower { get; private set; }
        internal Texture2D Barrack { get; private set; }
        internal Texture2D Kitchen { get; private set; }
        internal Texture2D Generator { get; private set; }
        internal Texture2D Extractor { get; private set; }
        internal Texture2D Laboratory { get; private set; }
        internal Texture2D Minion { get; private set; }


        internal void LoadContent(ContentManager content) {
            Platform = content.Load<Texture2D>("sprites/thumbnail/platform");
            Tower = content.Load<Texture2D>("sprites/thumbnail/tower");
            Barrack = content.Load<Texture2D>("sprites/thumbnail/barrack");
            Kitchen = content.Load<Texture2D>("sprites/thumbnail/kitchen");
            Generator = content.Load<Texture2D>("sprites/thumbnail/generator");
            Extractor = content.Load<Texture2D>("sprites/thumbnail/extractor");
            Laboratory = content.Load<Texture2D>("sprites/thumbnail/laboratory"); 
            Minion = content.Load<Texture2D>("sprites/thumbnail/minion");
        }
    }

    internal sealed class Objects {
        internal Texture2D Projectile { get; private set; }
        internal Texture2D Explosion { get; private set; }
        internal Texture2D EnergyIcon { get; private set; }
        internal Texture2D MassIcon { get; private set; }
        internal Texture2D FoodIcon { get; private set; }


        internal void LoadContent(ContentManager content) {
            Projectile = content.Load<Texture2D>("sprites/objects/projectile");
            Explosion = content.Load<Texture2D>("sprites/objects/explosion");
            EnergyIcon = content.Load<Texture2D>("sprites/objects/energyIcon");
            MassIcon = content.Load<Texture2D>("sprites/objects/massIcon");
            FoodIcon = content.Load<Texture2D>("sprites/objects/foodIcon");
        }
    }

    internal sealed class UtilTextures {
        internal Texture2D TowerRange { get; private set; }
        internal Texture2D PriorityHalf { get; private set; }
        internal Texture2D PriorityFull { get; private set; }
        internal Texture2D BarBulletsEmpty { get; private set; }
        internal Texture2D BarBulletsFull { get; private set; }
        internal Texture2D Excalamation { get; private set; }
        internal Texture2D Question { get; private set; }

        internal void LoadContent(ContentManager content) {
            TowerRange = content.Load<Texture2D>("sprites/util/range");
            PriorityHalf = content.Load<Texture2D>("sprites/util/priority_half");
            PriorityFull = content.Load<Texture2D>("sprites/util/priority_full");
            BarBulletsEmpty = content.Load<Texture2D>("sprites/util/bar_bullets_empty");
            BarBulletsFull = content.Load<Texture2D>("sprites/util/bar_bullets_full");
            Excalamation = content.Load<Texture2D>("sprites/util/exclamationpoint");
            Question = content.Load<Texture2D>("sprites/util/questionmark");
        }
    }

    internal sealed class InterfaceTextures {
        internal Texture2D CursorRegular { get; private set; }
        internal Texture2D CursorDelete { get; private set; }
        internal Texture2D Button { get; private set; }
        internal Texture2D ButtonPause { get; private set; }
        internal Texture2D Button1X { get; private set; }
        internal Texture2D Button2X { get; private set; }
        internal Texture2D Button3X { get; private set; }
        internal Texture2D ButtonDelete { get; private set; }
        internal Texture2D GameTitle { get; private set; }
        internal Texture2D LostTitle { get; private set; }
        internal Texture2D WonTitle { get; private set; }
        internal Texture2D PausedTitle { get; private set; }
        internal Texture2D GuiCard { get; private set; }
        internal Texture2D GuiGameSpeed { get; private set; }
        internal Texture2D GuiResources { get; private set; }
        internal Texture2D GuiConstruction { get; private set; }
        internal Texture2D GuiBar { get; private set; }
        internal Texture2D GuiMinionTasks { get; private set; }
        internal Texture2D GuiTime { get; private set; }
        internal Texture2D GuiTutorial { get; private set; }
        internal Texture2D GuiAchievements { get; private set; }
        internal Texture2D GuiPopup { get; private set; }
        internal Texture2D GuiMenu { get; private set; }
        internal Texture2D Background { get; private set; }

        internal void LoadContent(ContentManager content) {
            CursorRegular = content.Load<Texture2D>("sprites/interface/cursor");
            CursorDelete = content.Load<Texture2D>("sprites/interface/Hammer");
            Button = content.Load<Texture2D>("sprites/interface/button");
            ButtonPause = content.Load<Texture2D>("sprites/interface/gui_ButtonPause");
            Button1X = content.Load<Texture2D>("sprites/interface/gui_Button1x");
            Button2X = content.Load<Texture2D>("sprites/interface/gui_Button2x");
            Button3X = content.Load<Texture2D>("sprites/interface/gui_Button3x");
            ButtonDelete = content.Load<Texture2D>("sprites/interface/gui_ButtonDelete");
            GameTitle = content.Load<Texture2D>("sprites/interface/gametitle");
            PausedTitle = content.Load<Texture2D>("sprites/interface/pause");
            WonTitle = content.Load<Texture2D>("sprites/interface/congrats");
            LostTitle = content.Load<Texture2D>("sprites/interface/gameover");
            GuiCard = content.Load<Texture2D>("sprites/interface/gui_card");
            GuiGameSpeed = content.Load<Texture2D>("sprites/interface/gui_fastforward");
            GuiResources = content.Load<Texture2D>("sprites/interface/gui_left");
            GuiConstruction = content.Load<Texture2D>("sprites/interface/gui_middle");
            GuiBar = content.Load<Texture2D>("sprites/interface/gui_bar");
            GuiMinionTasks = content.Load<Texture2D>("sprites/interface/gui_right");
            GuiTime = content.Load<Texture2D>("sprites/interface/gui_timer");
            GuiTutorial = content.Load<Texture2D>("sprites/interface/gui_tutorial");
            GuiAchievements = content.Load<Texture2D>("sprites/interface/gui_achievements");
            GuiPopup = content.Load<Texture2D>("sprites/interface/gui_popup");
            Background = content.Load<Texture2D>("sprites/interface/background");
            GuiMenu = content.Load<Texture2D>("sprites/interface/gui_menu");
        }
    }

    internal sealed class TutorialScreens {
        internal Texture2D GameLogo { get; private set; }
        internal Texture2D ResourceDisplay { get; private set; }
        internal Texture2D ConstructionExample { get; private set; }
        internal Texture2D InterfaceOverview { get; private set; }
        internal Texture2D GeneratorKitchen { get; private set; }
        internal Texture2D ExtractorMass { get; private set; }
        internal Texture2D ExampleKitchen { get; private set; }
        internal Texture2D ExampleTower { get; private set; }
        internal Texture2D PortalLaboratory { get; private set; }
        internal Texture2D MinonControle { get; private set; }
        internal Texture2D ControlsIcon { get; private set; }

        internal void LoadContent(ContentManager content) {
            GameLogo = content.Load < Texture2D >("sprites/tutorial/GameLogo");
            ResourceDisplay = content.Load<Texture2D>("sprites/tutorial/resourceDisplay");
            ConstructionExample = content.Load<Texture2D>("sprites/tutorial/constructionExample");
            InterfaceOverview = content.Load<Texture2D>("sprites/tutorial/interfaceOverview");
            GeneratorKitchen = content.Load<Texture2D>("sprites/tutorial/generatorKitchen");
            ExtractorMass = content.Load<Texture2D>("sprites/tutorial/extractorMass");
            ExampleKitchen = content.Load<Texture2D>("sprites/tutorial/kitchenExample");
            ExampleTower = content.Load<Texture2D>("sprites/tutorial/towerExample");
            PortalLaboratory = content.Load<Texture2D>("sprites/tutorial/portalLaboratory");
            MinonControle = content.Load<Texture2D>("sprites/tutorial/minionControle");
            ControlsIcon = content.Load<Texture2D>("sprites/tutorial/controlsIcon");

        }
    }

    internal sealed class Textures {
        internal Tiles Tiles { get; } = new Tiles();
        internal Thumbnails Thumbnails { get; } = new Thumbnails();
        internal Creatures Creatures { get; } = new Creatures();
        internal Stars Stars { get; } = new Stars();
        internal Objects Objects { get; } = new Objects();
        internal UtilTextures UtilTextures { get; } = new UtilTextures();
        internal InterfaceTextures InterfaceTextures { get; } = new InterfaceTextures();
        internal TutorialScreens TutorialScreens { get; } = new TutorialScreens();

        public void LoadContent(ContentManager content) {
            Tiles.LoadContent(content);
            Creatures.LoadContent(content);
            Thumbnails.LoadContent(content);
            Stars.LoadContent(content);
            Objects.LoadContent(content);
            UtilTextures.LoadContent(content);
            InterfaceTextures.LoadContent(content);
            TutorialScreens.LoadContent(content);
        }
    }
}