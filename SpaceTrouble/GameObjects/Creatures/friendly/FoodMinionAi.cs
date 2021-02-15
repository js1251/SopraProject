using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures.friendly {
    internal sealed class FoodMinionAi : TransportMinionAi {
        public FoodMinionAi(Minion minion) : base(minion) {
            Minion = minion;
            TargetType = ObjectProperty.RequiresSpawnResources;
        }
    }
}