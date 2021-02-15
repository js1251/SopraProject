using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures.friendly {
    internal sealed class ConstructionMinionAi : TransportMinionAi {
        public ConstructionMinionAi(Minion minion) : base(minion) {
            Minion = minion;
            TargetType = ObjectProperty.UnderConstruction;
        }
    }
}