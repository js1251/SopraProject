using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects.Creatures;
using SpaceTrouble.GameObjects.Projectiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.InputOutput;
using SpaceTrouble.util.DataStructures;
using SpaceTrouble.util.Tools;
using SpaceTrouble.World;

// Created by Jakob Sailer, expanded by others

namespace SpaceTrouble.GameObjects.Tiles {
    internal sealed class TowerTile : Tile, IBuildable, IAnimating {
        private enum TowerState {
            Idle,
            Starting,
            Active,
            Stopping
        }

        // Resources
        [JsonProperty] public ResourceVector OnTheWayResources { get; set; }
        [JsonProperty] public ResourceVector RequiredResources { get; set; }
        [JsonProperty] public bool BuildingFinished { get; set; }

        // Shooting logic
        [JsonIgnore] private Vector2 MuzzlePosition { get; }
        [JsonProperty] public float AttackRadius { get; set; }
        [JsonProperty] private float AttackRate { get; set; }
        [JsonProperty] private float TowerDamage { get; set; }
        [JsonIgnore] private Creature CurrentTarget { get; set; }
        [JsonProperty] private float TimeSinceLastShot { get; set; }
        [JsonProperty] private int FakeShotsFired { get; set; }
        [JsonProperty] private int NumberOfFakeShotsPerRealShot { get; }
        [JsonIgnore] private float AngleTolerance { get; } // TODO: implement ?

        // Ammunition
        [JsonIgnore] public int NumberOfMagazines { get; }
        [JsonIgnore] public float ShotsPerMagazine { get; }
        [JsonProperty] public int ShotsFiredOfMagazine { get; set; } // reset whenever the projectileCost for all fired shots sum up to a single Resource
        [JsonIgnore] private ResourceVector RequiredResourcesPerMagazine { get; }
        [JsonIgnore] public bool TowerHasAmmo { get; private set; }

        // Animation
        [JsonProperty] public float CurrentFrame { get; set; }
        [JsonProperty] public int CurrentLayer { get; set; }
        [JsonIgnore] public float Angle { get; set; }
        [JsonIgnore] private float TurnRateFactor { get; }
        [JsonProperty] public AnimationEffect Effect { get; set; }
        [JsonIgnore] public float AnimationSpeed { get; }
        [JsonProperty] private TowerState CurrentState { get; set; }

        // Sound
        [JsonIgnore] private Tuple<Sound, SoundEffectInstance> ActiveSound { get; set; }

        [JsonIgnore] private bool mCheckState;    // Variable is just for sound effect.


        public TowerTile() {
            Dimensions = new Point(64, 80);
            Pivot = new Vector2(0.5f, 0.7f);
            var resourceMultiplier = 1 + WorldGameState.DifficultyManager.TowerCount * 0.1f;
            RequiredResources = new ResourceVector(3, 4, 1) * (int) Math.Round(resourceMultiplier);

            // shooting
            AttackRadius = 500f;
            TowerDamage = 25f; // WARNING! not all shots are real and wont do any damage! keep that in mind
            AttackRate = 0.15f;
            // by doing 1 / AttackRate the "interval" of real projectiles roughly stays the same ( -1 because 1 over 1 is 1 and you don't want to have any fakes at that low rate)
            NumberOfFakeShotsPerRealShot = (int) (0.5f / AttackRate) - 1; // how many fake projectiles per real projectile
            MuzzlePosition = Vector2.UnitY * -20f;
            TurnRateFactor = 0.05f; // how fast the turret turns towards its target angle relative to its previous angle (1 is immediate)
            AngleTolerance = 15f; // the turret rotation has to be at least this close towards the target angle relative to the tower for it to shoot
            CurrentState = TowerState.Idle;

            // Ammunition
            NumberOfMagazines = 5;
            ShotsPerMagazine = 30f;
            RequiredResourcesPerMagazine = new ResourceVector(0, 1, 0);

            // Animation
            TotalFrames = new Point(16, 2);
            CurrentFrame = 0;
            CurrentLayer = 0;
            AnimationSpeed = 12f; // 12 fps (nice for pixel-art)
            Angle = 0; // default angle
            Effect = AnimationEffect.None;

            mCheckState = true;
        }

        internal override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (CurrentTarget != null && (CurrentTarget.HitPoints <= 0 || Vector2.Distance(WorldPosition, CurrentTarget.WorldPosition) >= AttackRadius)) {
                CurrentTarget = null;
            }

            ((IAnimating) this).Update(gameTime);

            if (BuildingFinished) {
                HandleTowerState(); // sets the towers current state depending on CurrentTarget and the previous state
                HandleEffects();
                HandleShooting(gameTime);
            }
        }

        private void HandleTowerState()
        {
            // if the tower isn't fully built it cannot do anything
            if (!BuildingFinished) {
                CurrentState = TowerState.Idle;
                return;
            }

            // if the Requires Resources are smaller to the resourceRequirement for the total amount of magazines the tower has ammunition and can shoot
            TowerHasAmmo = RequiredResources.AllLessOrEqualThan(RequiredResourcesPerMagazine * (NumberOfMagazines - 1));

            if (!TowerHasAmmo && mCheckState) {
                SpaceTrouble.SoundManager.PlaySound(Sound.TowerIsEmpty);
                mCheckState = false;
            }
            
            // if the tower has finished shutting down set it back to idle
            if (CurrentState == TowerState.Stopping && CurrentLayer == 0 && CurrentFrame < 0.125f) {
                CurrentState = TowerState.Idle;
            }
            
            if (CurrentTarget != null && TowerHasAmmo) {
                mCheckState = true;
                // if the tower is idle start it up
                if (CurrentState == TowerState.Idle) {
                    CurrentState = TowerState.Starting;
                    return;
                }
                
                // if the tower was starting to shut down but was still returning to its default rotation just set it back to active
                if (CurrentState == TowerState.Stopping && CurrentLayer == 1) {
                    CurrentState = TowerState.Active;
                    return;
                }

                // if the tower has finished starting up set it to active
                if (CurrentState == TowerState.Starting && CurrentFrame >= TotalFrames.X / 2f - 1) {
                    Angle = 0;
                    CurrentState = TowerState.Active;
                }
            } else {
                // if the tower doesn't have a target but is starting up or active shut the tower down
                if (CurrentState == TowerState.Starting || CurrentState == TowerState.Active) {
                    CurrentState = TowerState.Stopping;
                }
            }
        }

        private void HandleEffects() {
            switch (CurrentState) {
                case TowerState.Idle:
                    DoIdleTower();
                    return;

                case TowerState.Starting:
                    ActivateTower();
                    return;

                case TowerState.Stopping:
                    DeactivateTower();
                    return;

                case TowerState.Active:
                    DoActiveTower();
                    return;

                default:
                    System.Diagnostics.Debug.WriteLine("Tower has an invalid state! You should never be here!");
                    return;
            }
        }

        private void DoIdleTower() {
            // reset the animation to be none and reset the tower angle and sound-effect
            Effect = AnimationEffect.None;
            CurrentLayer = 0;
            CurrentFrame = 0;
            Angle = 0;
            ActiveSound?.Item2?.Stop(false);
            ActiveSound = null;
        }

        private void ActivateTower() {
            if (CurrentLayer != 1) {
                // set the animation layer to be the layer with the start/ shutdown frames
                Effect = AnimationEffect.PlayOnce;
            }
        }

        private void DeactivateTower() {
            if (CurrentLayer == 1) {
                // reset the tower to face forward again
                if (Math.Round(CurrentFrame) > 0) {
                    Angle = VectorMath.LerpDegrees(Angle, 0, TurnRateFactor);
                    ((IAnimating)this).SetFrameFromAngle();
                    return;
                }

                // set the animation layer to be the layer with the start/ shutdown frames
                // 0° rotation is on the very left of the texture atlas so offset again to the end of the first layer frames
                CurrentLayer = 0;
                CurrentFrame = TotalFrames.X / 2f - 1;
            }

            Effect = AnimationEffect.PlayOnceReverse;
        }

        private void DoActiveTower() {
            // set the animation layer to be the layer with the rotation frames
            CurrentLayer = 1;

            // get the current rotation frame based on the angle from the tower to its target
            ((IAnimating) this).GetAngleFromHeading(Vector2.Normalize(CurrentTarget.WorldPosition - WorldPosition), TurnRateFactor);
            ((IAnimating) this).SetFrameFromAngle();
        }

        private void HandleShooting(GameTime gameTime) {
            // always increase the time since last shot
            TimeSinceLastShot += (float) gameTime.ElapsedGameTime.TotalSeconds;

            // if the time since last shot is still smaller than the given attack-rate return
            if (TimeSinceLastShot <= AttackRate) {
                return;
            }

            // here a new shot can be fired so reset the time since last shot
            TimeSinceLastShot -= AttackRate;

            // the tower can only shoot if its active however
            if (CurrentState != TowerState.Active) {
                HandleSound(false);
                return;
            }

            // only shoot if the tower is rotated close enough towards the target
            var muzzlePos = WorldPosition + MuzzlePosition;
            var angleTargetRelativeTurret = VectorMath.VectorToAngle(CurrentTarget.WorldPosition - muzzlePos);
            var angleDifferenceTurretToTarget = VectorMath.MinAngleBetweenAngles(Angle, angleTargetRelativeTurret);
            if (angleDifferenceTurretToTarget > AngleTolerance) {
                HandleSound(false);
                return;
            }

            HandleSound(true);

            // shoot a projectile
            SpawnProjectile();

            // every spawned shot costs some ammunition
            ShotsFiredOfMagazine++;

            // if a magazine is exhausted add the resources required for a new magazine to the towers required resources
            if ((int) (ShotsFiredOfMagazine / ShotsPerMagazine) > 0) {
                RequiredResources += RequiredResourcesPerMagazine;
                ShotsFiredOfMagazine = 0;
            }
        }

        private void HandleSound(bool canShoot) {
            if (canShoot) {
                // if the tower was not playing a sound before play the start-shooting sound
                if (ActiveSound == null || ActiveSound.Item1 == Sound.EndShot) {
                    ActiveSound?.Item2?.Stop(false);
                    var activeSoundEffect = SpaceTrouble.SoundManager.PlaySound(Sound.StartShot);
                    ActiveSound = new Tuple<Sound, SoundEffectInstance>(Sound.StartShot, activeSoundEffect);
                    return;
                }

                // if the active sound was the start shoot sound and it has finished playing start playing the loop shoot sound
                if (ActiveSound.Item1 == Sound.StartShot && ActiveSound.Item2?.State == SoundState.Stopped) {
                    var activeSoundEffect = SpaceTrouble.SoundManager.PlaySound(Sound.LoopShot);
                    ActiveSound = new Tuple<Sound, SoundEffectInstance>(Sound.LoopShot, activeSoundEffect);
                    return;
                }

                // if the active sound is the loop shooting sound and it has finished playing. Play it again
                if (ActiveSound.Item1 == Sound.LoopShot && ActiveSound.Item2?.State == SoundState.Stopped) {
                    var activeSoundEffect = SpaceTrouble.SoundManager.PlaySound(Sound.LoopShot);
                    ActiveSound = new Tuple<Sound, SoundEffectInstance>(Sound.LoopShot, activeSoundEffect);
                }
            } else {
                // if the tower was currently playing the looping shooting sound or the start shooting sound stop it and play the end sound
                if (ActiveSound?.Item1 == Sound.LoopShot || ActiveSound?.Item1 == Sound.StartShot) {
                    ActiveSound.Item2?.Stop(false);
                    var activeSoundEffect = SpaceTrouble.SoundManager.PlaySound(Sound.EndShot);
                    ActiveSound = new Tuple<Sound, SoundEffectInstance>(Sound.EndShot, activeSoundEffect);
                }
            }
        }

        private void SpawnProjectile() {
            // alternate between real and fake bullets to reduce performance strain
            var type = FakeShotsFired == NumberOfFakeShotsPerRealShot ? GameObjectEnum.RealProjectile : GameObjectEnum.FakeProjectile;
            FakeShotsFired = FakeShotsFired == NumberOfFakeShotsPerRealShot ? 0 : FakeShotsFired + 1;
            var gameObject = WorldGameState.ObjectManager.CreateObject(WorldPosition + MuzzlePosition, type);

            if (gameObject == null || !(gameObject is Projectile projectile)) {
                return;
            }

            // calculate a new target position that both extends the vector so the bullet can fly past the target
            // and also leads the target to increase probability of hitting the actual target

            // predict target position when bullet arrives target
            var source = WorldPosition + MuzzlePosition;
            var targetDirection = Vector2.Normalize(CurrentTarget.Heading);
            var predictedTargetPosition = VectorMath.PredictIntersection(source, projectile.Speed, CurrentTarget.WorldPosition, targetDirection, CurrentTarget.Speed);
            var projectileDistance = CoordinateManager.CartesianToIsometricLength(AttackRadius) * 2f;

            // extend the position so bullets can fly past the target
            var targetPosition = VectorMath.ExtendVectorFromTo(WorldPosition + MuzzlePosition, predictedTargetPosition, projectileDistance);
            projectile.Target = targetPosition;

            // finally if the projectile is a real projectile also set its damage and alliance
            if (projectile is RealProjectile realProjectile) {
                realProjectile.ProjectileDamage = TowerDamage;
                realProjectile.IsFriendly = true;
            }
        }

        internal override void Draw(SpriteBatch spriteBatch) {
            ((IAnimating) this).Draw(spriteBatch);
            ((IBuildable) this).DrawResources(spriteBatch);
            ((ICanHavePriority)this).Draw(spriteBatch);
        }

        public bool SetTarget(Creature enemy) {
            CurrentTarget ??= enemy;
            return CurrentTarget != null;
        }
    }
}