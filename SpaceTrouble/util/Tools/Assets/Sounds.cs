using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

// Created by Jakob Sailer

namespace SpaceTrouble.util.Tools.Assets {
    internal sealed class Music {
        internal Song RegularMusic01 { get; private set; }
        internal Song AttackMusic01 { get; private set; }
        internal Song AttackMusic02 { get; private set; }

        internal void LoadContent(ContentManager content) {
            RegularMusic01 = content.Load<Song>("sounds/music/music_regular");
            AttackMusic01 = content.Load<Song>("sounds/music/music_attack01");
            AttackMusic02 = content.Load<Song>("sounds/music/SpaceMusicFinalAttack4");
        }
    }

    internal sealed class Effects {
        internal SoundEffect AttackTransitionIn { get; private set; }
        internal SoundEffect AttackTransitionOut { get; private set; }
        internal SoundEffect Clicking { get; private set; }
        internal SoundEffect PlaceTile { get; private set; }
        internal SoundEffect StartShot { get; private set; }
        internal SoundEffect LoopShot { get; private set; }
        internal SoundEffect EndShot { get; private set; }
        internal SoundEffect MinionDying { get; private set; }
        internal SoundEffect WonGame { get; private set; }
        internal SoundEffect LostGame { get; private set; }
        internal SoundEffect OpenMenu { get; private set; }
        internal SoundEffect TowerIsEmpty { get; private set; }
        internal SoundEffect BuildFinished { get; private set; }
        internal SoundEffect PortalSpawnBegin { get; private set; }
        internal SoundEffect PortalSpawnLoop { get; private set; }
        internal SoundEffect PortalDestroyed { get; private set; }
        internal SoundEffect Achievement { get; private set; }

        internal void LoadContent(ContentManager content) {
            AttackTransitionIn = content.Load<SoundEffect>("sounds/music/ChillAttack");
            AttackTransitionOut = content.Load<SoundEffect>("sounds/music/AttackChill");
            Clicking = content.Load<SoundEffect>("sounds/Click");
            PlaceTile = content.Load<SoundEffect>("sounds/Tile");
            StartShot = content.Load<SoundEffect>("sounds/shootingStart");
            LoopShot = content.Load<SoundEffect>("sounds/shootingLoop");
            EndShot = content.Load<SoundEffect>("sounds/shootingEnd");
            MinionDying = content.Load<SoundEffect>("sounds/Dieing");
            WonGame = content.Load<SoundEffect>("sounds/WonGame");
            TowerIsEmpty = content.Load<SoundEffect>("sounds/TowerEmpty1");
            BuildFinished = content.Load<SoundEffect>("sounds/BuildFinished3");
            OpenMenu = content.Load<SoundEffect>("sounds/OpenMenu2");
            LostGame = content.Load<SoundEffect>("sounds/LostGame");
            PortalSpawnBegin = content.Load<SoundEffect>("sounds/portal_spawnbegin");
            PortalSpawnLoop = content.Load<SoundEffect>("sounds/portal_spawnloop");
            PortalDestroyed = content.Load<SoundEffect>("sounds/portal_destroyed");
            Achievement = content.Load<SoundEffect>("sounds/achievement");
        }
    }

    internal sealed class Sounds {
        internal Music Music { get; } = new Music();
        internal Effects Effects { get; } = new Effects();

        internal void LoadContent(ContentManager content) {
            Music.LoadContent(content);
            Effects.LoadContent(content);
        }
    }
}