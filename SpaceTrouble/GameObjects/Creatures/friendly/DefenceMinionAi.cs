using SpaceTrouble.World;

namespace SpaceTrouble.GameObjects.Creatures.friendly {
    internal sealed class DefenceMinionAi : TransportMinionAi {
        public DefenceMinionAi(Minion minion) : base(minion) {
            Minion = minion;
            TargetType = ObjectProperty.RequiresAmmunition;
        }
    }
}