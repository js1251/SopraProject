using System.Collections.Generic;
using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Tiles.Interfaces {
    internal interface ICollidable : IBoundingBox {
        List<GameObjectEnum> IgnoreCollision { get; }
        void OnCollide(GameObject collisionObject);
    }
}