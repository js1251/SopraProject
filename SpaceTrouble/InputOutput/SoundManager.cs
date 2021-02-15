using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using SpaceTrouble.SaveGameManager;
using SpaceTrouble.util.Tools.Assets;

namespace SpaceTrouble.InputOutput {
    internal enum Sound {
        Clicking,
        Tile,
        StartShot,
        LoopShot,
        EndShot,
        Explosion,
        Dying,
        WonGame,
        LostGame,
        OpenMenu,
        TowerIsEmpty,
        BuildFinished,
        Achievement,
        PortalSpawnBegin,
        PortalSpawnLoop,
        PortalDestroyed
    }

    internal enum Music {
        Regular,
        Attacking,
        FinalWave
    }

    internal sealed class SoundManager {
        private Music? CurrentMusic { get; set; }
        private float mVolMain;
        private float mVolMusic;
        private float mVolEffect;
        private readonly float mPit;
        private readonly float mPan;

        private Dictionary<Sound, SoundEffect> SoundEffects { get; set; }
        private Dictionary<Music, Song> Songs { get; set; }
        private Dictionary<Sound, HashSet<SoundEffectInstance>> PlayingSounds { get; set; }
        private int MaxTotalSounds { get; }
        private int MaxSameSounds { get; }

        internal SoundManager() {
            mPit = 0.0f;
            mPan = 0.0f;

            MaxTotalSounds = 6;
            MaxSameSounds = 2;
        }

        internal void LoadContent()
        {
            SoundEffects = new Dictionary<Sound, SoundEffect>
            {
                {Sound.Clicking, Assets.Sounds.Effects.Clicking},
                {Sound.Tile, Assets.Sounds.Effects.PlaceTile},
                {Sound.StartShot, Assets.Sounds.Effects.StartShot},
                {Sound.LoopShot, Assets.Sounds.Effects.LoopShot},
                {Sound.EndShot, Assets.Sounds.Effects.EndShot},
                {Sound.Explosion, Assets.Sounds.Effects.Clicking},
                {Sound.Dying, Assets.Sounds.Effects.MinionDying},
                {Sound.WonGame, Assets.Sounds.Effects.WonGame},
                {Sound.LostGame, Assets.Sounds.Effects.LostGame},
                {Sound.OpenMenu, Assets.Sounds.Effects.OpenMenu},
                {Sound.TowerIsEmpty, Assets.Sounds.Effects.TowerIsEmpty},
                {Sound.BuildFinished, Assets.Sounds.Effects.BuildFinished},
                {Sound.Achievement, Assets.Sounds.Effects.Achievement},
                {Sound.PortalSpawnBegin, Assets.Sounds.Effects.PortalSpawnBegin},
                {Sound.PortalSpawnLoop, Assets.Sounds.Effects.PortalSpawnLoop},
                {Sound.PortalDestroyed, Assets.Sounds.Effects.PortalDestroyed}

            };

            Songs = new Dictionary<Music, Song> {
                {Music.Regular, Assets.Sounds.Music.RegularMusic01},
                {Music.Attacking, Assets.Sounds.Music.AttackMusic01},
                {Music.FinalWave, Assets.Sounds.Music.AttackMusic02},
            };

            PlayingSounds = new Dictionary<Sound, HashSet<SoundEffectInstance>> {
                {Sound.Clicking, new HashSet<SoundEffectInstance>()},
                {Sound.Tile, new HashSet<SoundEffectInstance>()},
                {Sound.StartShot, new HashSet<SoundEffectInstance>()},
                {Sound.LoopShot, new HashSet<SoundEffectInstance>()},
                {Sound.EndShot, new HashSet<SoundEffectInstance>()},
                {Sound.Explosion, new HashSet<SoundEffectInstance>()},
                {Sound.Dying, new HashSet<SoundEffectInstance>()},
                {Sound.WonGame, new HashSet<SoundEffectInstance>()},
                {Sound.LostGame, new HashSet<SoundEffectInstance>()},
                {Sound.OpenMenu, new HashSet<SoundEffectInstance>()},
                {Sound.TowerIsEmpty, new HashSet<SoundEffectInstance>()},
                {Sound.BuildFinished, new HashSet<SoundEffectInstance>()},
                {Sound.Achievement, new HashSet<SoundEffectInstance>()},
                {Sound.PortalSpawnBegin, new HashSet<SoundEffectInstance>()},
                {Sound.PortalSpawnLoop, new HashSet<SoundEffectInstance>()},
                {Sound.PortalDestroyed, new HashSet<SoundEffectInstance>()}
            };
        }

        internal void Update() {
            foreach (var (_, set) in PlayingSounds) {
                foreach (var soundInstance in set.ToList()) {
                    if (soundInstance.State == SoundState.Stopped) {
                        set.Remove(soundInstance);
                        soundInstance.Dispose();
                    }
                }
            }
        }

        internal void PlayMusic(Music music) {
            if (CurrentMusic == music) {
                return;
            }

            CurrentMusic = music;
            MediaPlayer.Stop();

            MediaPlayer.Volume = mVolMusic * mVolMain;
            MediaPlayer.IsRepeating = true;

            if (music == Music.Regular) {
                Assets.Sounds.Effects.AttackTransitionOut.Play(mVolMusic * mVolMain, mPit, mPan);
            } else {
                Assets.Sounds.Effects.AttackTransitionIn.Play(mVolMusic * mVolMain, mPit, mPan);
            }

            MediaPlayer.Play(Songs[music]);
        }

        internal SoundEffectInstance PlaySound(Sound sound, float volume = 1f, bool isLooping = false, float pitch = 0f)
        {
            volume *= mVolEffect * mVolMain;
            // limit total number of sound effects
            var totalSoundsPlaying = PlayingSounds.Values.Sum(currentlyPlaying => currentlyPlaying.Count);
            if (totalSoundsPlaying > MaxTotalSounds) {
                return default;
            }

            // limit number of same sound effects
            if (PlayingSounds[sound].Count > MaxSameSounds) {
                return default;
            }

            var newSound = SoundEffects[sound].CreateInstance();
            newSound.Volume = volume;
            newSound.Pitch = pitch;
            newSound.IsLooped = isLooping;
            PlayingSounds[sound].Add(newSound);
            newSound.Play();

            return newSound;
        }


        internal void SetVolume(float volumeMain, float volumeMusic, float volumeEffect) {
            if (Math.Abs(mVolMain - volumeMain) > float.Epsilon) {
                SaveLoadManager.SaveSetting("MainVolume", volumeMain);
                mVolMain = volumeMain;
            }

            if (Math.Abs(mVolMusic - volumeMusic) > float.Epsilon) {
                SaveLoadManager.SaveSetting("MusicVolume", volumeMusic);
                mVolMusic = volumeMusic;
            }

            if (Math.Abs(mVolEffect - volumeEffect) > float.Epsilon) {
                SaveLoadManager.SaveSetting("EffectVolume", volumeEffect);
                mVolEffect = volumeEffect;
            }

            MediaPlayer.Volume = mVolMusic * mVolMain;
        }
    }
}