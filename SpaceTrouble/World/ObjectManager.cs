using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceTrouble.GameObjects;
using SpaceTrouble.GameObjects.Creatures.enemy;
using SpaceTrouble.GameObjects.Creatures.friendly;
using SpaceTrouble.GameObjects.Projectiles;
using SpaceTrouble.GameObjects.Tiles;
using SpaceTrouble.GameObjects.Tiles.Interfaces;
using SpaceTrouble.util.DataStructures.GameObjectStructure;
using SpaceTrouble.util.Tools;
using SpaceTrouble.util.Tools.Assets;
using Color = Microsoft.Xna.Framework.Color;

namespace SpaceTrouble.World {
    // ObjectProperty Enums HAVE to be called the same as their class counterparts!!!
    internal enum GameObjectEnum {
        EmptyTile,
        PlatformTile,
        TowerTile,
        BarrackTile,
        KitchenTile,
        MassTile,
        ExtractorTile,
        GeneratorTile,
        PortalTile,
        LaboratoryTile,
        StockTile,

        Minion,
        FlyingEnemy,
        WalkingEnemy,

        FakeProjectile,
        RealProjectile,
        ProjectileExplosion
    }

    // ObjectProperty Enums HAVE to be called the same as their interface counterparts except that the "I" prefix is removed!!
    internal enum ObjectProperty {
        Buildable,
        Moving,
        Collidable,
        ResourceContainer,
        ResourceGenerator,
        BoundingBox,
        Animating,
        CanHavePriority,

        Enemy,
        Friendly,

        // Enums that aren't created from interfaces
        UnderConstruction,
        RequiresSpawnResources,
        RequiresAmmunition,
        Walkable,
        Enterable,
        IsRealProjectile, // hack
        Flying,
        Other
    }

    internal sealed class ObjectManager {
        private readonly Dictionary<GameObjectEnum, Tuple<GameObject, Texture2D>> mObjectsDictionary = new Dictionary<GameObjectEnum, Tuple<GameObject, Texture2D>>();
        public GameDataStructure DataStructure { get; private set; } = new GameDataStructure();

        internal void LoadContent() {
            mObjectsDictionary[GameObjectEnum.RealProjectile] = new Tuple<GameObject, Texture2D>(new RealProjectile(), Assets.Textures.Objects.Projectile);
            mObjectsDictionary[GameObjectEnum.FakeProjectile] = new Tuple<GameObject, Texture2D>(new FakeProjectile(), Assets.Textures.Objects.Projectile);
            mObjectsDictionary[GameObjectEnum.ProjectileExplosion] = new Tuple<GameObject, Texture2D>(new ProjectileExplosion(), Assets.Textures.Objects.Explosion);
            mObjectsDictionary[GameObjectEnum.PortalTile] = new Tuple<GameObject, Texture2D>(new PortalTile(), Assets.Textures.Tiles.Portal);
            mObjectsDictionary[GameObjectEnum.LaboratoryTile] = new Tuple<GameObject, Texture2D>(new LaboratoryTile(), Assets.Textures.Tiles.Portal);
            mObjectsDictionary[GameObjectEnum.TowerTile] = new Tuple<GameObject, Texture2D>(new TowerTile(), Assets.Textures.Tiles.Tower);
            mObjectsDictionary[GameObjectEnum.KitchenTile] = new Tuple<GameObject, Texture2D>(new KitchenTile(), Assets.Textures.Tiles.Kitchen);
            mObjectsDictionary[GameObjectEnum.BarrackTile] = new Tuple<GameObject, Texture2D>(new BarrackTile(), Assets.Textures.Tiles.Barrack);
            mObjectsDictionary[GameObjectEnum.MassTile] = new Tuple<GameObject, Texture2D>(new MassTile(), Assets.Textures.Tiles.Mass);
            mObjectsDictionary[GameObjectEnum.EmptyTile] = new Tuple<GameObject, Texture2D>(new EmptyTile(), Assets.Textures.Tiles.Empty);
            mObjectsDictionary[GameObjectEnum.ExtractorTile] = new Tuple<GameObject, Texture2D>(new ExtractorTile(), Assets.Textures.Tiles.Extractor);
            mObjectsDictionary[GameObjectEnum.GeneratorTile] = new Tuple<GameObject, Texture2D>(new GeneratorTile(), Assets.Textures.Tiles.Generator);
            mObjectsDictionary[GameObjectEnum.PlatformTile] = new Tuple<GameObject, Texture2D>(new PlatformTile(), Assets.Textures.Tiles.Platform);
            mObjectsDictionary[GameObjectEnum.StockTile] = new Tuple<GameObject, Texture2D>(new StockTile(), Assets.Textures.Tiles.Platform);
            mObjectsDictionary[GameObjectEnum.Minion] = new Tuple<GameObject, Texture2D>(new Minion(), Assets.Textures.Creatures.Minion);
            mObjectsDictionary[GameObjectEnum.FlyingEnemy] = new Tuple<GameObject, Texture2D>(new FlyingEnemy(), Assets.Textures.Creatures.FlyingEnemy);
            mObjectsDictionary[GameObjectEnum.WalkingEnemy] = new Tuple<GameObject, Texture2D>(new WalkingEnemy(), Assets.Textures.Creatures.WalkingEnemy);
        }

        public void Update(GameTime gameTime) {
            DataStructure.Update(gameTime); // the data-structure always has to update first! 
        }

        public void Draw(SpriteBatch spriteBatch) {
            DataStructure.DrawData.Draw(spriteBatch);
        }

        /// <summary>
        /// Creates a new Object and inserts it into the data-structure.
        /// </summary>
        /// <param name="worldPos">World-coordinates.</param>
        /// <param name="type">GameObjectEnum type.</param>
        /// <returns>The newly created object.</returns>
        internal GameObject CreateObject(Vector2 worldPos, GameObjectEnum type) {
            if (!mObjectsDictionary.ContainsKey(type)) {
                System.Diagnostics.Debug.WriteLine("Could not create object: object with the name " + type + " does not exist");
                return default;
            }

            var newObject = CopyObject(type);

            if (newObject != null) {
                newObject.WorldPosition = worldPos;
                DataStructure.Insert(newObject);
            }

            return newObject;
        }

        public GameObject CopyObject(GameObjectEnum type) {
            var newObject = (GameObject) Activator.CreateInstance(mObjectsDictionary[type].Item1.GetType());
            if (newObject != null) {
                newObject.Texture = mObjectsDictionary[type].Item2;
            }

            return newObject;
        }

        /// <summary>
        /// Creates a new Tile and inserts it into the data-structure.
        /// </summary>
        /// <param name="tilePos">Tile-coordinates.</param>
        /// <param name="type">GameObjectEnum type.</param>
        /// <param name="forceBuild">Building is Finished</param>
        /// <returns>The newly created tile.</returns>
        internal Tile CreateTile(Vector2 tilePos, GameObjectEnum type, bool forceBuild = false) {
            var tileWorldPosition = CoordinateManager.TileToWorld(tilePos);
            var newTile = CreateObject(tileWorldPosition, type);

            if (newTile != null && forceBuild && newTile is IBuildable buildable) {
                buildable.AddResource(buildable.RequiredResources);
            }

            return (Tile) newTile;
        }

        internal /*Tile*/ void CreateGhostTile(Vector2 tilePos, GameObjectEnum type, Color color) {
            var ghost = CopyObject(type);
            ghost.WorldPosition = CoordinateManager.TileToWorld(tilePos);
            ghost.Color = color;

            if (!(ghost is Tile tile)) {
                return; //null;
            }

            DataStructure.DrawData.InsertGhostTile(tile);

            //return tile;
        }

        /// <summary>
        /// Loads an already existing object into the data-structure.
        /// </summary>
        /// <param name="gameObject">Object to load</param>
        public void LoadObject(GameObject gameObject) {
            gameObject.Texture = mObjectsDictionary[gameObject.Type].Item2;
            if (gameObject is TowerTile tower)
            {
                DataStructure.CollisionData.UpdateTowerReach(tower);
            }
            DataStructure.Insert(gameObject);
        }

        /// <summary>
        /// Removes a specified Object from the data-structure.
        /// </summary>
        /// <param name="gameObject">Object to remove.</param>
        public void Remove(GameObject gameObject) {
            DataStructure.Remove(gameObject);
        }

        /// <summary>
        /// Clears the whole world from ALL objects. Note that this even includes EmptyTiles!
        /// </summary>
        public void RemoveAll() {
            DataStructure = new GameDataStructure();
        }

        /// <summary>
        /// Returns the tile at given tile-coordinates.
        /// </summary>
        /// <param name="tilePos">Tile-coordinates.</param>
        /// <param name="includeGhosts">Includes ghost tiles.</param>
        /// <returns>Tile at given tile-coordinates.</returns>
        public Tile GetTile(Vector2 tilePos, bool includeGhosts = false) {
            Tile tile = null;
            if (includeGhosts) {
                tile = DataStructure.DrawData.GetGhostTile(tilePos);
            }

            // an actual existing tile should always have priority over a ghost tile
            tile ??= DataStructure.ObjectData.GetTile(tilePos);
            return tile;
        }

        /// <summary>
        /// Returns a list of ALL objects. No distinction is made.
        /// </summary>
        /// <returns>List of all objects.</returns>
        public List<GameObject> GetAllObjects() {
            return DataStructure.ObjectData.GetAllObjects();
        }

        /// <summary>
        /// Returns a list of Objects of given property.
        /// </summary>
        /// <param name="property">The property of the ObjectProperty to return. Use Enum ObjectProperty.
        /// </param>
        /// <returns>List of GameObjects of given property.</returns>
        public List<GameObject> GetAllObjects(ObjectProperty property) {
            return DataStructure.ObjectData.GetAllObjects(property);
        }

        /// <summary>
        /// Returns a list of Objects of given type.
        /// </summary>
        /// <param name="type">The property of the GameObjectEnum to return. Use Enum GameObjectEnum.
        /// </param>
        /// <returns>List of GameObjects of given type.</returns>
        public List<GameObject> GetAllObjects(GameObjectEnum type) {
            return DataStructure.ObjectData.GetAllObjects(type);
        }

        public List<GameObject> GetNearObjects(Vector2 center, float radius) {
            return DataStructure.CollisionData.GetNearObjects(center, radius);
        }
    }
}